using Bank.Api.DTOs;
using Bank.Api.Models;

namespace Bank.Api.Services
{
    public interface IBankService
    {
        Task<BankPaymentResponseDto> ProcessPaymentAsync(BankPaymentRequestDto request);
        Task<IEnumerable<BankAccount>> GetAccountsAsync();
        Task<IEnumerable<BankTransaction>> GetTransactionsAsync();
    }
}
