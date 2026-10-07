using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.DTOs;
using NWSDB.Api.Models;

namespace NWSDB.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly NwsdbDbContext _context;
        private readonly IBankApiService _bankApiService;

        public PaymentService(NwsdbDbContext context, IBankApiService bankApiService)
        {
            _context = context;
            _bankApiService = bankApiService;
        }

        public async Task<PaymentReceiptResultDto> ProcessCardPaymentAsync(ProcessCardPaymentRequestDto request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber);

            if (customer == null)
            {
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Customer account '{request.AccountNumber}' not found."
                };
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillNumber == request.BillNumber && b.CustomerId == customer.Id);

            if (bill == null)
            {
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Bill number '{request.BillNumber}' not found for account '{request.AccountNumber}'."
                };
            }

            var remainingDue = bill.BillAmount - bill.PaidAmount;
            if (remainingDue <= 0 || bill.Status == "Paid")
            {
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = "Bill is already fully paid."
                };
            }

            var amountToPay = request.Amount > 0 ? request.Amount : remainingDue;

            // Call Bank.Api to process card payment
            var bankRequest = new BankPaymentRequestDto
            {
                CardNumber = request.CardNumber,
                CardHolderName = string.IsNullOrWhiteSpace(request.CardHolderName) ? customer.Name : request.CardHolderName,
                CardExpiry = request.ExpiryDate,
                CardCvc = request.CVV,
                Amount = amountToPay
            };

            var bankResponse = await _bankApiService.ProcessPaymentAsync(bankRequest);

            if (!bankResponse.Success)
            {
                return new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = $"Bank payment authorization failed: {bankResponse.Message}",
                    BillNumber = bill.BillNumber,
                    AccountNumber = customer.AccountNumber,
                    CustomerName = customer.Name,
                    AmountPaid = 0,
                    PaymentMethod = "Card",
                    Channel = "NWSDB Website",
                    RemainingBillBalance = remainingDue,
                    BillStatus = bill.Status
                };
            }

            // Update NWSDB database bill & payment records
            bill.PaidAmount += amountToPay;
            bill.Status = (bill.PaidAmount >= bill.BillAmount) ? "Paid" : "Partially Paid";

            var receiptNo = "REC-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(1000, 9999);
            var paymentRecord = new Payment
            {
                BillId = bill.Id,
                CustomerId = customer.Id,
                ReceiptNumber = receiptNo,
                Amount = amountToPay,
                PaymentMethod = "Card",
                TransactionId = bankResponse.ReferenceNumber,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            };

            _context.Payments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            return new PaymentReceiptResultDto
            {
                Success = true,
                Message = "Payment recorded successfully.",
                ReceiptNumber = receiptNo,
                BillNumber = bill.BillNumber,
                AccountNumber = customer.AccountNumber,
                CustomerName = customer.Name,
                AmountPaid = amountToPay,
                PaymentMethod = "Card",
                Channel = "NWSDB Website",
                BankTransactionId = bankResponse.ReferenceNumber,
                PaidAt = paymentRecord.PaymentDate,
                RemainingBillBalance = bill.BillAmount - bill.PaidAmount,
                BillStatus = bill.Status
            };
        }

        public async Task<IEnumerable<PaymentRecordResponseDto>> GetPaymentHistoryAsync(string accountNumber)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (customer == null) return Enumerable.Empty<PaymentRecordResponseDto>();

            var payments = await _context.Payments
                .Where(p => p.CustomerId == customer.Id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(p => new PaymentRecordResponseDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                BillId = p.BillId,
                ReceiptNumber = p.ReceiptNumber,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionId = p.TransactionId,
                PaymentDate = p.PaymentDate,
                Status = p.Status
            });
        }
    }
}
