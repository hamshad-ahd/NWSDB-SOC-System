using NWSDB.Website.Models.ViewModels;

namespace NWSDB.Website.Services
{
    public interface INwsdbApiClient
    {
        Task<LoginApiResponse> LoginAsync(string accountNumber, string customerName);
        Task<CustomerDashboardDto?> GetDashboardAsync(string accountNumber);
        Task<BillDto?> GetBillAsync(string billNumber);
        Task<IEnumerable<BillDto>> GetBillHistoryAsync(string accountNumber);
        Task<IEnumerable<WaterUsageDto>> GetWaterUsageAsync(string accountNumber);
        Task<IEnumerable<PaymentRecordDto>> GetPaymentHistoryAsync(string accountNumber);
        Task<PaymentReceiptViewModel> ProcessCardPaymentAsync(CardPaymentViewModel model);
    }
}
