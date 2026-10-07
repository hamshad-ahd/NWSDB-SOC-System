using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public interface IThirdPartyPaymentService
    {
        Task<ThirdPartyCardCollectionResponseDto> CollectCardPaymentAsync(ThirdPartyCardCollectionRequestDto request);
        Task<ThirdPartyCashCollectionResponseDto> CollectCashPaymentAsync(ThirdPartyCashCollectionRequestDto request);
        Task<PaymentReceiptResultDto> TransferCollectionAsync(ThirdPartyTransferRequestDto request);
    }
}
