using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public interface IBillService
    {
        Task<BillResponseDto?> GetCurrentBillAsync(string accountNumber);
        Task<IEnumerable<BillResponseDto>> GetBillHistoryAsync(string accountNumber);
        Task<BillResponseDto?> GetBillByNumberAsync(string billNumber);
    }
}
