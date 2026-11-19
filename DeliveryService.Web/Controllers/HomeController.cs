using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Customer")) return RedirectToAction("Index", "Customer");
            if (User.IsInRole("Courier")) return RedirectToAction("Index", "Courier");
            if (User.IsInRole("Support") || User.IsInRole("Admin")) return RedirectToAction("Index", "Support");
            return RedirectToAction("Login", "Account");
        }
    }
}
