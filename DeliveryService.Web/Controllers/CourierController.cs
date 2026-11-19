using System.Security.Claims;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Infrastructure;
using DeliveryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Web.Controllers;

[Authorize(Roles = "Courier")]
public class CourierController : Controller
{
    private readonly DeliveryDbContext _db;
    private readonly CourierService _courier;

    public CourierController(DeliveryDbContext db, CourierService courier)
    {
        _db = db;
        _courier = courier;
    }

    private long CurrentUserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var courier = await _db.Couriers.AsNoTracking().FirstAsync(c => c.Id == CurrentUserId);
        return View(courier);
    }

    public async Task<IActionResult> Current()
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.CourierId == CurrentUserId && o.Status == OrderStatus.InDelivery);
        return View(order);   // null: заказа нет, view покажет сообщение
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> StartShift() => Run(() => _courier.StartShiftAsync(CurrentUserId), nameof(Index));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> EndShift() => Run(() => _courier.EndShiftAsync(CurrentUserId), nameof(Index));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> FinishDelivery() =>
        Run(() => _courier.CompleteDeliveryAsync(CurrentUserId), nameof(Index));

    private async Task<IActionResult> Run(Func<Task> action, string redirectTo)
    {
        try { await action(); }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Заказ только что изменился. Проверьте текущее состояние и повторите действие";
        }
        return RedirectToAction(redirectTo);
    }
}