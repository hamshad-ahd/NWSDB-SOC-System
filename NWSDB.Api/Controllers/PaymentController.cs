using Microsoft.AspNetCore.Mvc;
using NWSDB.Api.DTOs;
using NWSDB.Api.Services;

namespace NWSDB.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Get past payment history for a customer account.
        /// </summary>
        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> GetPaymentHistory(string accountNumber)
        {
            var payments = await _paymentService.GetPaymentHistoryAsync(accountNumber);
            return Ok(payments);
        }

        /// <summary>
        /// Record/update a successful card payment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecordPayment([FromBody] ProcessCardPaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = "Invalid payment request payload."
                });
            }

            var result = await _paymentService.ProcessCardPaymentAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
