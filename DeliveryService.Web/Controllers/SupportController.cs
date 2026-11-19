using DeliveryService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Web.Controllers;

[Authorize(Roles = AppRoles.Staff)]
public class SupportController : Controller
{
    private readonly DeliveryDbContext _db;
    public SupportController(DeliveryDbContext db) => _db = db;

    public IActionResult Index() => View();
}