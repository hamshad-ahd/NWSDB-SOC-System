using NWSDB.Api.DTOs;

namespace NWSDB.Api.Services
{
    public interface ICustomerService
    {
        Task<CustomerVerifyResponseDto> VerifyCustomerAsync(CustomerVerifyRequestDto request);
        Task<CustomerResponseDto?> GetCustomerByAccountAsync(string accountNumber);
    }
}
