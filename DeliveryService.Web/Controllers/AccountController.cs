using DeliveryService.Domain.Models.Roles;
using DeliveryService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DeliveryService.Web.Models;

namespace DeliveryService.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _auth;
        public AccountController(AuthService auth) => _auth = auth;

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel m)
        {
            if (!ModelState.IsValid) return View(m);

            var user = await _auth.ValidateAsync(m.Login, m.Password);
            if (user is null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(m);
            }

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role.ToString())   // "Customer" / "Courier"
        };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return user.Role switch
            {
                UserRole.Customer => RedirectToAction("Index", "Customer"),
                UserRole.Courier => RedirectToAction("Index", "Courier"),
                _ => RedirectToAction("Index", "Support")   // "Support" и "Admin"
            }; ;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult Denied() => View();
    }
}
