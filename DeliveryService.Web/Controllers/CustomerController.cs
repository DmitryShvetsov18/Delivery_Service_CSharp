using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Web.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    public IActionResult Index() => View();
}