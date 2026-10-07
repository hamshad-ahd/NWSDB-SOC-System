using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.DTOs;
using NWSDB.Api.Models;

namespace NWSDB.Api.Services
{
    public class BillService : IBillService
    {
        private readonly NwsdbDbContext _context;

        public BillService(NwsdbDbContext context)
        {
            _context = context;
        }

        public async Task<BillResponseDto?> GetCurrentBillAsync(string accountNumber)
        {
            var acc = (accountNumber ?? string.Empty).Trim();
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == acc);

            if (customer == null) return null;

            // Prioritize bills with outstanding balance (Pending or Partially Paid)
            var pendingBill = await _context.Bills
                .Include(b => b.Customer)
                .Where(b => b.CustomerId == customer.Id && (b.Status == "Pending" || b.Status == "Partially Paid"))
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            var bill = pendingBill ?? await _context.Bills
                .Include(b => b.Customer)
                .Where(b => b.CustomerId == customer.Id)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (bill == null) return null;

            return MapToDto(bill);
        }

        public async Task<IEnumerable<BillResponseDto>> GetBillHistoryAsync(string accountNumber)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (customer == null) return Enumerable.Empty<BillResponseDto>();

            var bills = await _context.Bills
                .Include(b => b.Customer)
                .Where(b => b.CustomerId == customer.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bills.Select(MapToDto);
        }

        public async Task<BillResponseDto?> GetBillByNumberAsync(string billNumber)
        {
            var bill = await _context.Bills
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BillNumber == billNumber);

            if (bill == null) return null;

            return MapToDto(bill);
        }

        private static BillResponseDto MapToDto(Bill bill)
        {
            return new BillResponseDto
            {
                Id = bill.Id,
                CustomerId = bill.CustomerId,
                BillNumber = bill.BillNumber,
                BillingMonth = bill.BillingMonth,
                UnitsUsed = bill.UnitsUsed,
                BillAmount = bill.BillAmount,
                PaidAmount = bill.PaidAmount,
                DueDate = bill.DueDate,
                Status = bill.Status,
                CreatedAt = bill.CreatedAt,
                Customer = bill.Customer != null ? new CustomerResponseDto
                {
                    Id = bill.Customer.Id,
                    AccountNumber = bill.Customer.AccountNumber,
                    Name = bill.Customer.Name,
                    Address = bill.Customer.Address,
                    Phone = bill.Customer.Phone,
                    Email = bill.Customer.Email
                } : null
            };
        }
    }
}
