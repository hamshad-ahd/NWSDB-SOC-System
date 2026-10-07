using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public class UsageService : IUsageService
    {
        private readonly NwsdbDbContext _context;

        public UsageService(NwsdbDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WaterUsageResponseDto>> GetUsageHistoryAsync(string accountNumber)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (customer == null) return Enumerable.Empty<WaterUsageResponseDto>();

            var bills = await _context.Bills
                .Where(b => b.CustomerId == customer.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bills.Select(b => new WaterUsageResponseDto
            {
                MonthYear = b.BillingMonth,
                UnitsConsumed = b.UnitsUsed,
                ReadingDate = b.CreatedAt
            });
        }
    }
}
