using Microsoft.AspNetCore.Mvc;
using NWSDB.Api.DTOs;
using NWSDB.Api.Services;

namespace NWSDB.Api.Controllers
{
    [ApiController]
    [Route("api/thirdparty")]
    public class ThirdPartyController : ControllerBase
    {
        private readonly IThirdPartyPaymentService _thirdPartyPaymentService;

        public ThirdPartyController(IThirdPartyPaymentService thirdPartyPaymentService)
        {
            _thirdPartyPaymentService = thirdPartyPaymentService;
        }

        /// <summary>
        /// Process Card collection at Third-Party counter via Bank.Api before transfer.
        /// </summary>
        [HttpPost("collect-card")]
        public async Task<IActionResult> CollectCardPayment([FromBody] ThirdPartyCardCollectionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ThirdPartyCardCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = "Invalid card collection payload."
                });
            }

            var result = await _thirdPartyPaymentService.CollectCardPaymentAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// STAGE 1 (CASH): Collects cash payment at third-party counter and records pending collection.
        /// Does NOT call Bank.Api (Bank.Api is never involved in cash collections).
        /// </summary>
        [HttpPost("collect-cash")]
        public async Task<IActionResult> CollectCashPayment([FromBody] ThirdPartyCashCollectionRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ThirdPartyCashCollectionResponseDto
                {
                    Success = false,
                    CollectionStatus = "Failed",
                    Message = "Invalid cash collection payload."
                });
            }

            var result = await _thirdPartyPaymentService.CollectCashPaymentAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// STAGE 2 (TRANSFER / SETTLEMENT): Settles an existing collected third-party transaction into NWSDB using CollectionId.
        /// CRITICAL RULES:
        /// - Receives CollectionId of an existing collection with CollectionStatus == "Collected" and TransferStatus == "Pending".
        /// - MUST NOT process another payment.
        /// - MUST NOT call Bank.Api.
        /// - MUST NOT deduct money from the bank during transfer.
        /// - Updates bill PaidAmount/status, generates official NWSDB Payment record, and marks collection as "Transferred".
        /// </summary>
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferCollection([FromBody] ThirdPartyTransferRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new PaymentReceiptResultDto
                {
                    Success = false,
                    Message = "Invalid transfer request payload. Valid CollectionId is required."
                });
            }

            var result = await _thirdPartyPaymentService.TransferCollectionAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
