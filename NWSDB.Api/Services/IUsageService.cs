using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public interface IUsageService
    {
        Task<IEnumerable<WaterUsageResponseDto>> GetUsageHistoryAsync(string accountNumber);
    }
}
