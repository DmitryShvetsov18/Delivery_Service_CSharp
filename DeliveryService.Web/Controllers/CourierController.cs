using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Web.Controllers;

[Authorize(Roles = "Courier")]
public class CourierController : Controller
{
    public IActionResult Index() => View();
}