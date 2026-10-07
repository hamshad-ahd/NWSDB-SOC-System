using Microsoft.AspNetCore.Mvc;
using Bank.Api.DTOs;
using Bank.Api.Services;

namespace Bank.Api.Controllers
{
    [ApiController]
    [Route("api/bank")]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;

        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        /// <summary>
        /// Processes a card payment against bank accounts and returns reference number or failure reason.
        /// </summary>
        [HttpPost("payments")]
        public async Task<IActionResult> ProcessPayment([FromBody] BankPaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BankPaymentResponseDto
                {
                    Success = false,
                    Message = "Invalid request payload or missing parameters."
                });
            }

            var result = await _bankService.ProcessPaymentAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all registered bank card accounts for audit/testing.
        /// </summary>
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _bankService.GetAccountsAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Retrieves all processed bank transactions for audit/testing.
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var txs = await _bankService.GetTransactionsAsync();
            return Ok(txs);
        }
    }
}
