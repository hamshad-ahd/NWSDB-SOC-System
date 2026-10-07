using ThirdParty.WebApp.Models.ViewModels;

namespace ThirdParty.WebApp.Services
{
    public interface INwsdbApiClient
    {
        Task<CustomerDto?> SearchCustomerAsync(string searchTerm);
        Task<BillDto?> GetActiveBillAsync(string accountNumber);
        Task<BillDto?> GetBillAsync(string billNumber);
        Task<List<BillDto>> GetBillHistoryAsync(string accountNumber);
        Task<CardCollectionResultViewModel> CollectCardPaymentAsync(PaymentCollectionViewModel model);
        Task<CashCollectionResultViewModel> CollectCashPaymentAsync(PaymentCollectionViewModel model);
        Task<SettlementReceiptViewModel> TransferCollectionAsync(TransferViewModel model);
    }
}
