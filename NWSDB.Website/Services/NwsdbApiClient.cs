using NWSDB.Website.Models.ViewModels;

namespace NWSDB.Website.Services
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

        public async Task<LoginApiResponse> LoginAsync(string accountNumber, string customerName)
        {
            try
            {
                var payload = new
                {
                    accountNumber = (accountNumber ?? string.Empty).Trim(),
                    name = (customerName ?? string.Empty).Trim()
                };

                var response = await _httpClient.PostAsJsonAsync("api/customers/verify", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CustomerVerifyResult>();
                    if (result != null && result.Success && result.Customer != null)
                    {
                        return new LoginApiResponse
                        {
                            Success = true,
                            Message = result.Message,
                            AccountNumber = result.Customer.AccountNumber,
                            Name = result.Customer.Name,
                            Email = result.Customer.Email
                        };
                    }
                }

                try
                {
                    var errRes = await response.Content.ReadFromJsonAsync<CustomerVerifyResult>();
                    if (errRes != null && !string.IsNullOrEmpty(errRes.Message))
                    {
                        return new LoginApiResponse { Success = false, Message = errRes.Message };
                    }
                }
                catch
                {
                    // Fallback when response content cannot be parsed as JSON
                }

                return new LoginApiResponse
                {
                    Success = false,
                    Message = $"Customer verification failed (HTTP {(int)response.StatusCode}: {response.ReasonPhrase}). Please check your Account Number and Name."
                };
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Cannot reach NWSDB.Api at {BaseAddress} during login verification", _httpClient.BaseAddress);
                return new LoginApiResponse
                {
                    Success = false,
                    Message = $"Unable to connect to NWSDB Core API at {_httpClient.BaseAddress}. Please ensure the NWSDB.Api service is running on port 5002. ({hex.Message})"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in via NWSDB.Api");
                return new LoginApiResponse { Success = false, Message = $"Server connection error: {ex.Message}" };
            }
        }

        public async Task<CustomerDashboardDto?> GetDashboardAsync(string accountNumber)
        {
            try
            {
                var safeAccount = Uri.EscapeDataString((accountNumber ?? string.Empty).Trim());
                var customer = await _httpClient.GetFromJsonAsync<CustomerDto>($"api/customers/{safeAccount}");
                if (customer == null) return null;

                BillDto? currentBill = null;
                try
                {
                    currentBill = await _httpClient.GetFromJsonAsync<BillDto>($"api/bills/{safeAccount}/current");
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Normal when no pending bill exists
                    _logger.LogInformation("No active bill found for account {Account}", safeAccount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error retrieving current bill for {Account}", safeAccount);
                }

                var payments = await GetPaymentHistoryAsync(safeAccount);

                return new CustomerDashboardDto
                {
                    Customer = customer,
                    ActiveBill = currentBill,
                    PaymentHistory = payments.ToList()
                };
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Unable to reach NWSDB.Api at {BaseAddress} while fetching dashboard for {Account}", _httpClient.BaseAddress, accountNumber);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard for {Account}", accountNumber);
                return null;
            }
        }

        public async Task<BillDto?> GetBillAsync(string billNumber)
        {
            try
            {
                var safeBillNumber = Uri.EscapeDataString((billNumber ?? string.Empty).Trim());
                return await _httpClient.GetFromJsonAsync<BillDto>($"api/bills/detail/{safeBillNumber}");
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Unable to reach NWSDB.Api at {BaseAddress} while fetching bill {BillNumber}", _httpClient.BaseAddress, billNumber);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bill {BillNumber}", billNumber);
                return null;
            }
        }

        public async Task<IEnumerable<BillDto>> GetBillHistoryAsync(string accountNumber)
        {
            try
            {
                var safeAccount = Uri.EscapeDataString((accountNumber ?? string.Empty).Trim());
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<BillDto>>($"api/bills/{safeAccount}/history");
                return result ?? Enumerable.Empty<BillDto>();
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Unable to reach NWSDB.Api at {BaseAddress} while fetching bill history for {Account}", _httpClient.BaseAddress, accountNumber);
                return Enumerable.Empty<BillDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bill history for account {AccountNumber}", accountNumber);
                return Enumerable.Empty<BillDto>();
            }
        }

        public async Task<IEnumerable<WaterUsageDto>> GetWaterUsageAsync(string accountNumber)
        {
            try
            {
                var safeAccount = Uri.EscapeDataString((accountNumber ?? string.Empty).Trim());
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<WaterUsageDto>>($"api/usage/{safeAccount}");
                return result ?? Enumerable.Empty<WaterUsageDto>();
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Unable to reach NWSDB.Api at {BaseAddress} while fetching water usage for {Account}", _httpClient.BaseAddress, accountNumber);
                return Enumerable.Empty<WaterUsageDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching water usage for account {AccountNumber}", accountNumber);
                return Enumerable.Empty<WaterUsageDto>();
            }
        }

        public async Task<IEnumerable<PaymentRecordDto>> GetPaymentHistoryAsync(string accountNumber)
        {
            try
            {
                var safeAccount = Uri.EscapeDataString((accountNumber ?? string.Empty).Trim());
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<PaymentRecordDto>>($"api/payments/{safeAccount}");
                return result ?? Enumerable.Empty<PaymentRecordDto>();
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Unable to reach NWSDB.Api at {BaseAddress} while fetching payment history for {Account}", _httpClient.BaseAddress, accountNumber);
                return Enumerable.Empty<PaymentRecordDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment history for {Account}", accountNumber);
                return Enumerable.Empty<PaymentRecordDto>();
            }
        }

        public async Task<PaymentReceiptViewModel> ProcessCardPaymentAsync(CardPaymentViewModel model)
        {
            try
            {
                var payload = new
                {
                    billNumber = (model.BillNumber ?? string.Empty).Trim(),
                    accountNumber = (model.AccountNumber ?? string.Empty).Trim(),
                    cardNumber = (model.CardNumber ?? string.Empty).Replace(" ", "").Trim(),
                    cardHolderName = (model.CardHolderName ?? string.Empty).Trim(),
                    expiryDate = (model.CardExpiry ?? string.Empty).Trim(),
                    cvv = (model.CardCvc ?? string.Empty).Trim(),
                    amount = model.AmountToPay
                };

                var response = await _httpClient.PostAsJsonAsync("api/payments", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PaymentReceiptViewModel>();
                    return result ?? new PaymentReceiptViewModel { Success = false, Message = "Failed to parse receipt from API." };
                }

                try
                {
                    var errRes = await response.Content.ReadFromJsonAsync<PaymentReceiptViewModel>();
                    if (errRes != null && !string.IsNullOrEmpty(errRes.Message))
                    {
                        return errRes;
                    }
                }
                catch
                {
                    // Fallback when response content cannot be parsed as JSON
                }

                return new PaymentReceiptViewModel
                {
                    Success = false,
                    Message = $"Payment processing error from API (HTTP {(int)response.StatusCode}: {response.ReasonPhrase})."
                };
            }
            catch (HttpRequestException hex)
            {
                _logger.LogError(hex, "Unable to reach NWSDB.Api at {BaseAddress} while processing card payment", _httpClient.BaseAddress);
                return new PaymentReceiptViewModel
                {
                    Success = false,
                    Message = $"Unable to connect to NWSDB Core API at {_httpClient.BaseAddress}. Please verify that NWSDB.Api is running on port 5002. ({hex.Message})"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing card payment API call");
                return new PaymentReceiptViewModel { Success = false, Message = $"Connection error: {ex.Message}" };
            }
        }

        private class CustomerVerifyResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public CustomerDto? Customer { get; set; }
        }
    }
}
