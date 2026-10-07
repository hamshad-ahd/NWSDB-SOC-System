using Microsoft.AspNetCore.Mvc;
using NWSDB.Website.Models.ViewModels;
using NWSDB.Website.Services;

namespace NWSDB.Website.Controllers
{
    public class PaymentController : Controller
    {
        private readonly INwsdbApiClient _apiClient;

        public PaymentController(INwsdbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> PayCard(string billNumber)
        {
            var acc = HttpContext.Session.GetString("AccountNumber");
            if (acc == null) return RedirectToAction("Login", "Auth");

            var bill = await _apiClient.GetBillAsync(billNumber);
            if (bill == null)
            {
                ViewBag.ErrorMessage = $"Unable to retrieve bill '{billNumber}' from NWSDB Core API (http://localhost:5002). Please verify that the bill exists and that NWSDB.Api is running on port 5002.";
                return View("ApiUnavailable");
            }

            var model = new CardPaymentViewModel
            {
                BillNumber = bill.BillNumber,
                AccountNumber = acc,
                CustomerName = HttpContext.Session.GetString("CustomerName") ?? "Valued Customer",
                AmountToPay = bill.DueAmount > 0 ? bill.DueAmount : bill.BillAmount,
                CardHolderName = HttpContext.Session.GetString("CustomerName") ?? "Kavindu Perera",
                CardNumber = "4532718293841029",
                CardExpiry = "12/28",
                CardCvc = "123"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PayCard(CardPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var receipt = await _apiClient.ProcessCardPaymentAsync(model);
            if (!receipt.Success)
            {
                model.ErrorMessage = receipt.Message;
                return View(model);
            }

            TempData["ReceiptJson"] = System.Text.Json.JsonSerializer.Serialize(receipt);
            return RedirectToAction("Receipt");
        }

        [HttpGet]
        public IActionResult Receipt()
        {
            var json = TempData["ReceiptJson"] as string;
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var receipt = System.Text.Json.JsonSerializer.Deserialize<PaymentReceiptViewModel>(json);
            return View(receipt);
        }
    }
}
