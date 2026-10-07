using Microsoft.EntityFrameworkCore;
using Bank.Api.Data;
using Bank.Api.DTOs;
using Bank.Api.Models;

namespace Bank.Api.Services
{
    public class BankService : IBankService
    {
        private readonly BankDbContext _context;

        public BankService(BankDbContext context)
        {
            _context = context;
        }

        public async Task<BankPaymentResponseDto> ProcessPaymentAsync(BankPaymentRequestDto request)
        {
            // 0. Validate Amount
            if (request.Amount <= 0)
            {
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid transaction amount. Payment amount must be greater than zero.",
                    Amount = request.Amount
                };
            }

            var cleanCardNumber = request.CardNumber.Replace(" ", "").Replace("-", "");

            // 1. Validate Card Existence
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.CardNumber == cleanCardNumber);

            if (account == null)
            {
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid card. Card number was not found in core banking system.",
                    Amount = request.Amount
                };
            }

            // 2. Validate Cardholder Name
            if (!account.CardHolderName.Equals(request.CardHolderName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid cardholder name. Name provided does not match account records.",
                    Amount = request.Amount
                };
            }

            // 3. Validate CVV
            if (account.CVV != request.CVV.Trim())
            {
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid CVV. Card security code check failed.",
                    Amount = request.Amount
                };
            }

            // 4. Validate Expiry Date
            if (IsCardExpired(account.ExpiryDate) || IsCardExpired(request.ExpiryDate))
            {
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Expired card. Transaction declined due to expired card status.",
                    Amount = request.Amount
                };
            }

            // 5. Check Active Status
            if (!account.IsActive)
            {
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Inactive card. Account has been deactivated or frozen by issuer.",
                    Amount = request.Amount
                };
            }

            // 6. Check Sufficient Balance
            if (account.Balance < request.Amount)
            {
                var refFailed = "TXN-BANK-DEC-" + Random.Shared.Next(100000, 999999);
                var failedTx = new BankTransaction
                {
                    BankAccountId = account.Id,
                    Amount = request.Amount,
                    TransactionDate = DateTime.UtcNow,
                    TransactionType = "Debit",
                    Status = "Declined_Insufficient_Funds",
                    ReferenceNumber = refFailed
                };
                _context.BankTransactions.Add(failedTx);
                await _context.SaveChangesAsync();

                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = $"Insufficient balance. Available: Rs. {account.Balance:N2}, Required: Rs. {request.Amount:N2}",
                    ReferenceNumber = refFailed,
                    Amount = request.Amount,
                    RemainingBalance = account.Balance
                };
            }

            // 7. Deduct Amount & Process Transaction
            account.Balance -= request.Amount;

            var uniqueRef = "TXN-BANK-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(100000, 999999);
            var transaction = new BankTransaction
            {
                BankAccountId = account.Id,
                Amount = request.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionType = "Debit",
                Status = "Success",
                ReferenceNumber = uniqueRef
            };

            _context.BankTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // 8. Return Successful Response
            return new BankPaymentResponseDto
            {
                Success = true,
                Message = "Bank payment processed successfully.",
                ReferenceNumber = uniqueRef,
                Amount = request.Amount,
                RemainingBalance = account.Balance,
                Timestamp = transaction.TransactionDate
            };
        }

        public async Task<IEnumerable<BankAccount>> GetAccountsAsync()
        {
            return await _context.BankAccounts.ToListAsync();
        }

        public async Task<IEnumerable<BankTransaction>> GetTransactionsAsync()
        {
            return await _context.BankTransactions
                .Include(t => t.BankAccount)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        private static bool IsCardExpired(string expiryDateStr)
        {
            try
            {
                var parts = expiryDateStr.Split('/');
                if (parts.Length != 2) return true;

                if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                {
                    return true;
                }

                // Assume 2000s for YY format
                int fullYear = year < 100 ? 2000 + year : year;

                // Card is valid through end of expiry month
                var lastDayOfMonth = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month), 23, 59, 59);
                return DateTime.UtcNow > lastDayOfMonth;
            }
            catch
            {
                return true; // Parse error treat as expired
            }
        }
    }
}
