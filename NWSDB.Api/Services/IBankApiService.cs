using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public interface IBankApiService
    {
        Task<BankPaymentResponseDto> ProcessPaymentAsync(BankPaymentRequestDto request);
    }
}
