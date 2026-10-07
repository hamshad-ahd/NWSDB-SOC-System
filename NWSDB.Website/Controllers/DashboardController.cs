using Microsoft.AspNetCore.Mvc;
using NWSDB.Website.Services;

namespace NWSDB.Website.Controllers
{
    public class DashboardController : Controller
    {
        private readonly INwsdbApiClient _apiClient;

        public DashboardController(INwsdbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        private string? GetCurrentAccount()
        {
            return HttpContext.Session.GetString("AccountNumber");
        }

        public async Task<IActionResult> Index()
        {
            var acc = GetCurrentAccount();
            if (acc == null) return RedirectToAction("Login", "Auth");

            var dashboard = await _apiClient.GetDashboardAsync(acc);
            if (dashboard == null)
            {
                ViewBag.ErrorMessage = "Unable to connect to NWSDB Core API (http://localhost:5002) to load dashboard data. Please ensure NWSDB.Api is running on port 5002.";
                return View("ApiUnavailable");
            }

            return View(dashboard);
        }

        public async Task<IActionResult> AccountDetails()
        {
            var acc = GetCurrentAccount();
            if (acc == null) return RedirectToAction("Login", "Auth");

            var dashboard = await _apiClient.GetDashboardAsync(acc);
            if (dashboard == null)
            {
                ViewBag.ErrorMessage = "Unable to connect to NWSDB Core API (http://localhost:5002) to load account profile. Please ensure NWSDB.Api is running on port 5002.";
                return View("ApiUnavailable");
            }

            return View(dashboard.Customer);
        }

        public async Task<IActionResult> PaymentHistory()
        {
            var acc = GetCurrentAccount();
            if (acc == null) return RedirectToAction("Login", "Auth");

            var payments = await _apiClient.GetPaymentHistoryAsync(acc);
            return View(payments);
        }

        public async Task<IActionResult> Usage()
        {
            var acc = GetCurrentAccount();
            if (acc == null) return RedirectToAction("Login", "Auth");

            var usage = await _apiClient.GetWaterUsageAsync(acc);
            return View(usage);
        }
    }
}
