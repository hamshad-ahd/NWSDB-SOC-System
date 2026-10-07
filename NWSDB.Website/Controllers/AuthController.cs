using Microsoft.AspNetCore.Mvc;
using NWSDB.Website.Models.ViewModels;
using NWSDB.Website.Services;

namespace NWSDB.Website.Controllers
{
    public class AuthController : Controller
    {
        private readonly INwsdbApiClient _apiClient;

        public AuthController(INwsdbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AccountNumber") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.LoginAsync(model.AccountNumber, model.CustomerName);
            if (!response.Success)
            {
                model.ErrorMessage = response.Message;
                return View(model);
            }

            HttpContext.Session.SetString("AccountNumber", response.AccountNumber!);
            HttpContext.Session.SetString("CustomerName", response.Name ?? "Customer");
            HttpContext.Session.SetString("CustomerEmail", response.Email ?? "");

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
