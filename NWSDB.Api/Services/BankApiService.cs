using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public class BankApiService : IBankApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BankApiService> _logger;

        public BankApiService(HttpClient httpClient, ILogger<BankApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<BankPaymentResponseDto> ProcessPaymentAsync(BankPaymentRequestDto request)
        {
            try
            {
                var payload = new
                {
                    cardNumber = request.CardNumber,
                    cardHolderName = request.CardHolderName,
                    expiryDate = request.CardExpiry,
                    cvv = request.CardCvc,
                    amount = request.Amount
                };

                var response = await _httpClient.PostAsJsonAsync("api/bank/payments", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<BankPaymentResponseDto>();
                    return result ?? new BankPaymentResponseDto { Success = false, Message = "Failed to deserialize bank response." };
                }

                BankPaymentResponseDto? errorResult = null;
                try
                {
                    errorResult = await response.Content.ReadFromJsonAsync<BankPaymentResponseDto>();
                }
                catch { }

                return errorResult ?? new BankPaymentResponseDto { Success = false, Message = $"Bank API returned HTTP status {response.StatusCode}." };
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Bank.Api request timed out.");
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Bank API service request timed out after 15 seconds. Payment not processed."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with Bank.Api");
                return new BankPaymentResponseDto
                {
                    Success = false,
                    Message = $"Bank API Communication Error: {ex.Message}"
                };
            }
        }
    }
}
