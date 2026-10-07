using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.DTOs;
using NWSDB.Api.Models;

namespace NWSDB.Api.Services
{
    public class ThirdPartyPaymentService : IThirdPartyPaymentService
    {
        private readonly NwsdbDbContext _context;
        private readonly IBankApiService _bankApiService;
        private readonly ILogger<ThirdPartyPaymentService> _logger;

        public ThirdPartyPaymentService(
            NwsdbDbContext context, 
            IBankApiService bankApiService)
            : this(context, bankApiService, Microsoft.Extensions.Logging.Abstractions.NullLogger<ThirdPartyPaymentService>.Instance)
        {
        }

        public ThirdPartyPaymentService(
            NwsdbDbContext context, 
            IBankApiService bankApiService,
            ILogger<ThirdPartyPaymentService> logger)
        {
            _context = context;
            _bankApiService = bankApiService;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ThirdPartyPaymentService>.Instance;
        }

        public async Task<ThirdPartyCardCollectionResponseDto> CollectCardPaymentAsync(ThirdPartyCardCollectionRequestDto request)
        {
            var accountNum = (request.AccountNumber ?? string.Empty).Trim();
            var billNum = (request.BillNumber ?? string.Empty).Trim();
            var maskedCard = MaskCardNumber(request.CardNumber);

            _logger.LogInformation(
                "CARD Collection Request received: Account '{Account}', Bill '{Bill}', Amount {Amount:C}, Card {Card}, Collector '{Collector}'",
                accountNum, billNum, request.Amount, maskedCard, request.CollectorId);

            // 1. Verify Customer and Bill
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNum);

            if (customer == null)
            {
                _logger.LogWarning("CARD Collection failed: Customer account '{Account}' not found.", accountNum);
                return new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Customer account '{accountNum}' not found."
                };
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillNumber == billNum && b.CustomerId == customer.Id);

            if (bill == null)
            {
                _logger.LogWarning("CARD Collection failed: Bill '{Bill}' not found for account '{Account}'.", billNum, accountNum);
                return new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Bill number '{billNum}' not found for account '{accountNum}'."
                };
            }

            var dueAmount = bill.BillAmount - bill.PaidAmount;
            if (dueAmount <= 0 || bill.Status == "Paid")
            {
                _logger.LogWarning("CARD Collection failed: Bill '{Bill}' is already fully paid.", billNum);
                return new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = "Bill is already fully paid."
                };
            }

            if (request.Amount <= 0)
            {
                _logger.LogWarning("CARD Collection failed: Non-positive amount {Amount} for Bill '{Bill}'.", request.Amount, billNum);
                return new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = "Collection amount must be greater than zero."
                };
            }

            if (request.Amount > dueAmount)
            {
                _logger.LogWarning("CARD Collection failed: Amount {Amount:C} exceeds outstanding due balance {Due:C} for Bill '{Bill}'.", 
                    request.Amount, dueAmount, billNum);
                return new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Collection amount (Rs. {request.Amount:N2}) cannot exceed the outstanding bill balance (Rs. {dueAmount:N2})."
                };
            }

            // 2. Authorize and deduct amount via Bank.Api
            var cardHolder = string.IsNullOrWhiteSpace(request.CardHolderName) ? customer.Name : request.CardHolderName.Trim();
            var bankReq = new BankPaymentRequestDto
            {
                CardNumber = request.CardNumber.Trim(),
                CardHolderName = cardHolder,
                CardExpiry = request.ExpiryDate.Trim(),
                CardCvc = request.CVV.Trim(),
                Amount = request.Amount
            };

            _logger.LogInformation(
                "Forwarding CARD collection payment of {Amount:C} for Bill '{Bill}' to Bank.Api (CardHolder: '{Holder}', Card: {Card})...",
                request.Amount, billNum, cardHolder, maskedCard);

            var bankRes = await _bankApiService.ProcessPaymentAsync(bankReq);
            if (!bankRes.Success)
            {
                _logger.LogWarning("Bank.Api rejected CARD payment for Bill '{Bill}': {BankMessage}", billNum, bankRes.Message);
                return new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Card processing failed at Bank: {bankRes.Message}"
                };
            }

            _logger.LogInformation(
                "Bank.Api transaction approved! Bank Reference: {Ref}, Amount: {Amount:C}. Proceeding to record collection in NWSDB.",
                bankRes.ReferenceNumber, request.Amount);

            // 3. Record Third-Party Collection as Collected (TransferStatus = Pending)
            var collection = new ThirdPartyCollection
            {
                BillId = bill.Id,
                CustomerId = customer.Id,
                Amount = request.Amount,
                PaymentMethod = "Card",
                BankTransactionId = bankRes.ReferenceNumber,
                CollectionStatus = "Collected",
                CollectedDate = DateTime.UtcNow,
                TransferStatus = "Pending"
            };

            _context.ThirdPartyCollections.Add(collection);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "STAGE 1 Complete: CARD collection created with CollectionId #{Id}, BankTxnRef '{Ref}', Status = 'Collected', TransferStatus = 'Pending'.",
                collection.Id, bankRes.ReferenceNumber);

            return new ThirdPartyCardCollectionResponseDto
            {
                Success = true,
                CollectionId = collection.Id,
                BankTransactionId = bankRes.ReferenceNumber,
                CollectionStatus = "Collected",
                TransferStatus = "Pending",
                Message = "Card payment collected successfully at counter."
            };
        }

        public async Task<ThirdPartyCashCollectionResponseDto> CollectCashPaymentAsync(ThirdPartyCashCollectionRequestDto request)
        {
            var accountNum = (request.AccountNumber ?? string.Empty).Trim();
            var billNum = (request.BillNumber ?? string.Empty).Trim();

            _logger.LogInformation(
                "CASH Collection Request received: Account '{Account}', Bill '{Bill}', Amount {Amount:C}, Collector '{Collector}'",
                accountNum, billNum, request.Amount, request.CollectorId);

            // 1. Verify Customer and Bill
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNum);

            if (customer == null)
            {
                _logger.LogWarning("CASH Collection failed: Customer account '{Account}' not found.", accountNum);
                return new ThirdPartyCashCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Customer account '{accountNum}' not found."
                };
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillNumber == billNum && b.CustomerId == customer.Id);

            if (bill == null)
            {
                _logger.LogWarning("CASH Collection failed: Bill '{Bill}' not found for account '{Account}'.", billNum, accountNum);
                return new ThirdPartyCashCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Bill number '{billNum}' not found for account '{accountNum}'."
                };
            }

            var dueAmount = bill.BillAmount - bill.PaidAmount;
            if (dueAmount <= 0 || bill.Status == "Paid")
            {
                _logger.LogWarning("CASH Collection failed: Bill '{Bill}' is already fully paid.", billNum);
                return new ThirdPartyCashCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = "Bill is already fully paid."
                };
            }

            if (request.Amount <= 0)
            {
                _logger.LogWarning("CASH Collection failed: Non-positive amount {Amount} for Bill '{Bill}'.", request.Amount, billNum);
                return new ThirdPartyCashCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = "Collection amount must be greater than zero."
                };
            }

            if (request.Amount > dueAmount)
            {
                _logger.LogWarning("CASH Collection failed: Amount {Amount:C} exceeds outstanding due balance {Due:C} for Bill '{Bill}'.",
                    request.Amount, dueAmount, billNum);
                return new ThirdPartyCashCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = $"Collection amount (Rs. {request.Amount:N2}) cannot exceed the outstanding bill balance (Rs. {dueAmount:N2})."
                };
            }

            // 2. Record Third-Party Collection as Collected (TransferStatus = Pending)
            // Bank.Api is NEVER called for Cash collections.
            var collection = new ThirdPartyCollection
            {
                BillId = bill.Id,
                CustomerId = customer.Id,
                Amount = request.Amount,
                PaymentMethod = "Cash",
                BankTransactionId = null,
                CollectionStatus = "Collected",
                CollectedDate = DateTime.UtcNow,
                TransferStatus = "Pending"
            };

            _context.ThirdPartyCollections.Add(collection);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "STAGE 1 Complete: CASH collection created with CollectionId #{Id}, Amount {Amount:C}, Status = 'Collected', TransferStatus = 'Pending'. (Bank.Api was NOT called).",
                collection.Id, request.Amount);

            return new ThirdPartyCashCollectionResponseDto
            {
                Success = true,
                CollectionId = collection.Id,
                CollectionStatus = "Collected",
                TransferStatus = "Pending",
                Message = "Cash payment collected successfully at counter."
            };
        }

        public async Task<PaymentReceiptResultDto> TransferCollectionAsync(ThirdPartyTransferRequestDto request)
        {
            _logger.LogInformation("STAGE 2 Settlement Transfer requested for CollectionId #{CollectionId}...", request.CollectionId);

            if (request.CollectionId <= 0)
            {
                _logger.LogWarning("Transfer failed: Invalid CollectionId #{CollectionId}", request.CollectionId);
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = "A valid Collection ID is required for settlement transfer."
                };
            }

            // 1. Receive CollectionId & Find the corresponding ThirdPartyCollection
            var collection = await _context.ThirdPartyCollections
                .Include(c => c.Customer)
                .Include(c => c.Bill)
                .FirstOrDefaultAsync(c => c.Id == request.CollectionId);

            if (collection == null)
            {
                _logger.LogWarning("Transfer failed: Collection record #{CollectionId} not found.", request.CollectionId);
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Third-party collection record with ID '{request.CollectionId}' was not found."
                };
            }

            // 2. Verify CollectionStatus == "Collected"
            if (collection.CollectionStatus != "Collected")
            {
                _logger.LogWarning("Transfer failed: Collection #{Id} has invalid status '{Status}'.", collection.Id, collection.CollectionStatus);
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Invalid collection status '{collection.CollectionStatus}'. Collection must be 'Collected' before transfer."
                };
            }

            // 3. DUPLICATE TRANSFER PROTECTION: Verify TransferStatus == "Pending"
            if (collection.TransferStatus == "Transferred")
            {
                _logger.LogWarning(
                    "DUPLICATE TRANSFER REJECTED: Collection #{Id} for Bill '{Bill}' has already been transferred (Ref: {Ref}).",
                    collection.Id, collection.Bill?.BillNumber ?? collection.BillId.ToString(), collection.TransferReference);

                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Duplicate transfer rejected! Collection for Bill '{collection.Bill?.BillNumber ?? collection.BillId.ToString()}' has already been transferred (Ref: {collection.TransferReference})."
                };
            }

            if (collection.TransferStatus != "Pending")
            {
                _logger.LogWarning("Transfer failed: Collection #{Id} has unexpected transfer status '{Status}'.", collection.Id, collection.TransferStatus);
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Invalid transfer status '{collection.TransferStatus}'. Only 'Pending' collections can be transferred to NWSDB."
                };
            }

            // 4. Find related customer and bill
            var bill = collection.Bill ?? await _context.Bills.FindAsync(collection.BillId);
            var customer = collection.Customer ?? await _context.Customers.FindAsync(collection.CustomerId);

            if (bill == null || customer == null)
            {
                _logger.LogError("Transfer failed: Customer or Bill record missing for Collection #{Id}.", collection.Id);
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = "Related customer or bill record could not be found for this collection."
                };
            }

            // CRITICAL ARCHITECTURE RULE: /api/thirdparty/transfer MUST NOT call Bank.Api!
            // Funds were already collected during STAGE 1 (Cash or Card).
            // Do NOT deduct money from the bank during transfer.

            // 5. Update NWSDB bill PaidAmount and status
            var transferAmount = collection.Amount;
            bill.PaidAmount += transferAmount;
            bill.Status = (bill.PaidAmount >= bill.BillAmount) ? "Paid" : "Partially Paid";

            // 6. Create the official NWSDB Payment record
            var receiptNo = "REC-TP-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(1000, 9999);
            var transferRef = "TRF-TP-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(100000, 999999);

            var paymentRecord = new Payment
            {
                BillId = bill.Id,
                CustomerId = customer.Id,
                ReceiptNumber = receiptNo,
                Amount = transferAmount,
                PaymentMethod = collection.PaymentMethod,
                TransactionId = collection.BankTransactionId ?? transferRef,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            };

            // 7. Mark ThirdPartyCollection as Transferred
            collection.TransferStatus = "Transferred";
            collection.TransferDate = DateTime.UtcNow;
            collection.TransferReference = transferRef;

            _context.Payments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "STAGE 2 Complete: Collection #{Id} successfully settled into NWSDB. Receipt: {Receipt}, TransferRef: {Ref}, Bill: {Bill}, New Due: {Due:C}, Status: {Status}.",
                collection.Id, receiptNo, transferRef, bill.BillNumber, bill.BillAmount - bill.PaidAmount, bill.Status);

            return new PaymentReceiptResultDto
            {
                Success = true,
                Message = "Third-party collected funds transferred to NWSDB successfully.",
                ReceiptNumber = receiptNo,
                TransferReference = transferRef,
                BillNumber = bill.BillNumber,
                AccountNumber = customer.AccountNumber,
                CustomerName = customer.Name,
                AmountPaid = transferAmount,
                PaymentMethod = collection.PaymentMethod,
                Channel = "ThirdParty WebApp",
                BankTransactionId = collection.BankTransactionId,
                PaidAt = paymentRecord.PaymentDate,
                RemainingBillBalance = bill.BillAmount - bill.PaidAmount,
                BillStatus = bill.Status
            };
        }

        private static string MaskCardNumber(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return "N/A";
            var clean = cardNumber.Replace(" ", "").Replace("-", "");
            if (clean.Length <= 4) return "****";
            return new string('*', clean.Length - 4) + clean[^4..];
        }
    }
}
