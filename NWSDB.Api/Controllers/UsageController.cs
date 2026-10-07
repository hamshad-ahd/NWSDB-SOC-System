using Microsoft.AspNetCore.Mvc;
using NWSDB.Api.Services;

namespace NWSDB.Api.Controllers
{
    [ApiController]
    [Route("api/usage")]
    public class UsageController : ControllerBase
    {
        private readonly IUsageService _usageService;

        public UsageController(IUsageService usageService)
        {
            _usageService = usageService;
        }

        /// <summary>
        /// Get water usage history for a customer account.
        /// </summary>
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> GetWaterUsage(string accountNumber)
        {
            var usage = await _usageService.GetUsageHistoryAsync(accountNumber);
            return Ok(usage);
        }
    }
}
