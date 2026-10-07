using Microsoft.AspNetCore.Mvc;
using NWSDB.Api.DTOs;
using NWSDB.Api.Services;

namespace NWSDB.Api.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Customer verification endpoint (Verify login credentials).
        /// </summary>
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCustomer([FromBody] CustomerVerifyRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CustomerVerifyResponseDto
                {
                    Success = false,
                    Message = "Invalid request payload."
                });
            }

            var result = await _customerService.VerifyCustomerAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get customer profile by account number.
        /// </summary>
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> GetCustomer(string accountNumber)
        {
            var customer = await _customerService.GetCustomerByAccountAsync(accountNumber);
            if (customer == null)
            {
                return NotFound(new { Message = $"Customer account '{accountNumber}' not found." });
            }
            return Ok(customer);
        }
    }
}
