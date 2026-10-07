using System.Net.Http.Json;
using ThirdParty.WebApp.Models.ViewModels;

namespace ThirdParty.WebApp.Services
{
    public class NwsdbApiClient : INwsdbApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NwsdbApiClient> _logger;

        public NwsdbApiClient(HttpClient httpClient, ILogger<NwsdbApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CustomerDto?> SearchCustomerAsync(string searchTerm)
        {
            try
            {
                var term = searchTerm.Trim();
                return await _httpClient.GetFromJsonAsync<CustomerDto>($"api/customers/{term}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Customer lookup failed for {Term}", searchTerm);
                return null;
            }
        }

        public async Task<BillDto?> GetActiveBillAsync(string accountNumber)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<BillDto>($"api/bills/{accountNumber}/current");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active bill for {Account}", accountNumber);
                return null;
            }
        }

        public async Task<BillDto?> GetBillAsync(string billNumber)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<BillDto>($"api/bills/detail/{billNumber.Trim()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bill {BillNumber}", billNumber);
                return null;
            }
        }

        public async Task<List<BillDto>> GetBillHistoryAsync(string accountNumber)
        {
            try
            {
                var list = await _httpClient.GetFromJsonAsync<List<BillDto>>($"api/bills/{accountNumber.Trim()}/history");
                return list ?? new List<BillDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bill history for {Account}", accountNumber);
                return new List<BillDto>();
            }
        }

        public async Task<CardCollectionResultViewModel> CollectCardPaymentAsync(PaymentCollectionViewModel model)
        {
            try
            {
                var cardHolder = !string.IsNullOrWhiteSpace(model.CardHolderName) 
                    ? model.CardHolderName.Trim() 
                    : (model.CustomerName?.Trim() ?? string.Empty);

                var payload = new
                {
                    billNumber = model.BillNumber?.Trim(),
                    accountNumber = model.AccountNumber?.Trim(),
                    amount = model.AmountToCollect,
                    cardNumber = model.CardNumber?.Trim(),
                    cardHolderName = cardHolder,
                    expiryDate = model.CardExpiry?.Trim(),
                    cvv = model.CardCvc?.Trim(),
                    collectorId = model.CollectorId?.Trim()
                };

                var response = await _httpClient.PostAsJsonAsync("api/thirdparty/collect-card", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await TryReadFromJsonAsync<CardCollectionResultViewModel>(response);
                    return result ?? new CardCollectionResultViewModel { Success = false, CollectionStatus = "Failed", Message = "Failed to parse card collection response." };
                }

                var errRes = await TryReadFromJsonAsync<CardCollectionResultViewModel>(response);
                if (errRes != null) return errRes;

                var rawBody = await response.Content.ReadAsStringAsync();
                return new CardCollectionResultViewModel 
                { 
                    Success = false, 
                    CollectionStatus = "Failed", 
                    Message = !string.IsNullOrWhiteSpace(rawBody) ? rawBody : $"Card collection failed at backend (Status {(int)response.StatusCode})." 
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to reach NWSDB.Api at {BaseAddress} during card collection", _httpClient.BaseAddress);
                return new CardCollectionResultViewModel 
                { 
                    Success = false, 
                    CollectionStatus = "Failed", 
                    Message = $"Unable to connect to NWSDB API at {_httpClient.BaseAddress ?? new Uri("http://localhost:5002")}. Please verify NWSDB.Api is running on port 5002." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting card payment via NWSDB.Api");
                return new CardCollectionResultViewModel { Success = false, CollectionStatus = "Failed", Message = $"Connection error: {ex.Message}" };
            }
        }

        public async Task<CashCollectionResultViewModel> CollectCashPaymentAsync(PaymentCollectionViewModel model)
        {
            try
            {
                var payload = new
                {
                    billNumber = model.BillNumber?.Trim(),
                    accountNumber = model.AccountNumber?.Trim(),
                    amount = model.AmountToCollect,
                    collectorId = model.CollectorId?.Trim()
                };

                var response = await _httpClient.PostAsJsonAsync("api/thirdparty/collect-cash", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await TryReadFromJsonAsync<CashCollectionResultViewModel>(response);
                    return result ?? new CashCollectionResultViewModel { Success = false, CollectionStatus = "Failed", Message = "Failed to parse cash collection response." };
                }

                var errRes = await TryReadFromJsonAsync<CashCollectionResultViewModel>(response);
                if (errRes != null) return errRes;

                var rawBody = await response.Content.ReadAsStringAsync();
                return new CashCollectionResultViewModel 
                { 
                    Success = false, 
                    CollectionStatus = "Failed", 
                    Message = !string.IsNullOrWhiteSpace(rawBody) ? rawBody : $"Cash collection failed at backend (Status {(int)response.StatusCode})." 
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to reach NWSDB.Api at {BaseAddress} during cash collection", _httpClient.BaseAddress);
                return new CashCollectionResultViewModel 
                { 
                    Success = false, 
                    CollectionStatus = "Failed", 
                    Message = $"Unable to connect to NWSDB API at {_httpClient.BaseAddress ?? new Uri("http://localhost:5002")}. Please verify NWSDB.Api is running on port 5002." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting cash payment via NWSDB.Api");
                return new CashCollectionResultViewModel { Success = false, CollectionStatus = "Failed", Message = $"Connection error: {ex.Message}" };
            }
        }

        public async Task<SettlementReceiptViewModel> TransferCollectionAsync(TransferViewModel model)
        {
            try
            {
                var payload = new
                {
                    collectionId = model.CollectionId
                };

                var response = await _httpClient.PostAsJsonAsync("api/thirdparty/transfer", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await TryReadFromJsonAsync<SettlementReceiptViewModel>(response);
                    return result ?? new SettlementReceiptViewModel { Success = false, Message = "Failed to parse receipt from NWSDB.Api." };
                }

                var errRes = await TryReadFromJsonAsync<SettlementReceiptViewModel>(response);
                if (errRes != null) return errRes;

                var rawBody = await response.Content.ReadAsStringAsync();
                return new SettlementReceiptViewModel 
                { 
                    Success = false, 
                    Message = !string.IsNullOrWhiteSpace(rawBody) ? rawBody : $"Transfer failed at NWSDB.Api backend (Status {(int)response.StatusCode})." 
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to reach NWSDB.Api at {BaseAddress} during transfer", _httpClient.BaseAddress);
                return new SettlementReceiptViewModel 
                { 
                    Success = false, 
                    Message = $"Unable to connect to NWSDB API at {_httpClient.BaseAddress ?? new Uri("http://localhost:5002")}. Please verify NWSDB.Api is running on port 5002." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring collection to NWSDB.Api");
                return new SettlementReceiptViewModel { Success = false, Message = $"Connection error: {ex.Message}" };
            }
        }

        private static async Task<T?> TryReadFromJsonAsync<T>(HttpResponseMessage response) where T : class
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}
