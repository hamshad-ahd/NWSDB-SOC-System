using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public interface IPaymentService
    {
        Task<PaymentReceiptResultDto> ProcessCardPaymentAsync(ProcessCardPaymentRequestDto request);
        Task<IEnumerable<PaymentRecordResponseDto>> GetPaymentHistoryAsync(string accountNumber);
    }
}
