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
            if (user.Role is not (UserRole.Customer or UserRole.Courier))
            {
                ModelState.AddModelError("", "Для этой роли кабинета в MVP нет");
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

            return RedirectToAction("Index", user.Role == UserRole.Customer ? "Customer" : "Courier");
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
