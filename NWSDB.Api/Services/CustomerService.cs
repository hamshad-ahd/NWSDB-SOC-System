using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly NwsdbDbContext _context;

        public CustomerService(NwsdbDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerVerifyResponseDto> VerifyCustomerAsync(CustomerVerifyRequestDto request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber.Trim());

            if (customer == null || (!string.IsNullOrWhiteSpace(request.Name) && !customer.Name.Trim().Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return new CustomerVerifyResponseDto
                {
                    Success = false,
                    Message = "Invalid Account Number or Customer Name."
                };
            }

            return new CustomerVerifyResponseDto
            {
                Success = true,
                Message = "Customer verification successful.",
                Customer = new CustomerResponseDto
                {
                    Id = customer.Id,
                    AccountNumber = customer.AccountNumber,
                    Name = customer.Name,
                    Address = customer.Address,
                    Phone = customer.Phone,
                    Email = customer.Email
                }
            };
        }

        public async Task<CustomerResponseDto?> GetCustomerByAccountAsync(string accountNumber)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber.Trim());

            if (customer == null) return null;

            return new CustomerResponseDto
            {
                Id = customer.Id,
                AccountNumber = customer.AccountNumber,
                Name = customer.Name,
                Address = customer.Address,
                Phone = customer.Phone,
                Email = customer.Email
            };
        }
    }
}
