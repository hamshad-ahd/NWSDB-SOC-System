using Microsoft.AspNetCore.Mvc;
using NWSDB.Website.Services;

namespace NWSDB.Website.Controllers
{
    public class BillController : Controller
    {
        private readonly INwsdbApiClient _apiClient;

        public BillController(INwsdbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> CurrentBill()
        {
            var acc = HttpContext.Session.GetString("AccountNumber");
            if (acc == null) return RedirectToAction("Login", "Auth");

            var dashboard = await _apiClient.GetDashboardAsync(acc);
            if (dashboard == null)
            {
                ViewBag.ErrorMessage = "Unable to connect to NWSDB Core API (http://localhost:5002) to load current bill. Please ensure NWSDB.Api is running on port 5002.";
                return View("ApiUnavailable");
            }
            if (dashboard.ActiveBill == null)
            {
                ViewBag.Message = "No active pending bill found.";
                return View(null);
            }

            return View(dashboard.ActiveBill);
        }

        public async Task<IActionResult> History()
        {
            var acc = HttpContext.Session.GetString("AccountNumber");
            if (acc == null) return RedirectToAction("Login", "Auth");

            var bills = await _apiClient.GetBillHistoryAsync(acc);
            return View(bills);
        }
    }
}
