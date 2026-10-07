using Microsoft.AspNetCore.Mvc;
using NWSDB.Api.Services;

namespace NWSDB.Api.Controllers
{
    [ApiController]
    [Route("api/bills")]
    public class BillController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService;
        }

        /// <summary>
        /// Get current active bill for a customer account.
        /// </summary>
        [HttpGet("{accountNumber}/current")]
        public async Task<IActionResult> GetCurrentBill(string accountNumber)
        {
            var bill = await _billService.GetCurrentBillAsync(accountNumber);
            if (bill == null)
            {
                return NotFound(new { Message = $"No active bill found for account '{accountNumber}'." });
            }
            return Ok(bill);
        }

        /// <summary>
        /// Get complete bill history for a customer account.
        /// </summary>
        [HttpGet("{accountNumber}/history")]
        public async Task<IActionResult> GetBillHistory(string accountNumber)
        {
            var bills = await _billService.GetBillHistoryAsync(accountNumber);
            return Ok(bills);
        }

        /// <summary>
        /// Get specific bill details by bill number.
        /// </summary>
        [HttpGet("detail/{billNumber}")]
        public async Task<IActionResult> GetBillByNumber(string billNumber)
        {
            var bill = await _billService.GetBillByNumberAsync(billNumber);
            if (bill == null)
            {
                return NotFound(new { Message = $"Bill number '{billNumber}' not found." });
            }
            return Ok(bill);
        }
    }
}
