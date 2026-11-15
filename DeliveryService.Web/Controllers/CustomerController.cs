using System.Security.Claims;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Infrastructure;
using DeliveryService.Services;
using DeliveryService.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Web.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : Controller
{
    private readonly DeliveryDbContext _db;
    private readonly OrderService _orders;

    public CustomerController(DeliveryDbContext db, OrderService orders)
    {
        _db = db;
        _orders = orders;
    }

    private long CurrentUserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == CurrentUserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateOrderViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel m)
    {
        ValidateItems(m);
        if (!ModelState.IsValid) return View(m);

        var items = m.Items.Where(i => !i.IsEmpty)
            .Select(i => new OrderItemData(i.Name!.Trim(), i.Quantity!.Value, i.WeightKg!.Value,
                i.LengthCm!.Value, i.WidthCm!.Value, i.HeightCm!.Value))
            .ToList();

        var data = new CreateOrderData(m.ExternalNumber.Trim(), m.DeliveryAddress.Trim(),
            m.RecipientFirstName.Trim(), m.RecipientLastName.Trim(),
            string.IsNullOrWhiteSpace(m.RecipientMiddleName) ? null : m.RecipientMiddleName.Trim(),
            m.RecipientPhone.Trim(), items);

        CreateOrderResult result;
        try
        {
            result = await _orders.CreateAsync(CurrentUserId, data, EventChannel.Web);
        }
        catch (DeliveryNotPossibleException)
        {
            ModelState.AddModelError("", "Ни один курьер не может перевезти такой заказ (вес, объём или габариты)");
            return View(m);
        }

        TempData["Message"] = result.Status == CreateOrderStatus.Duplicate
            ? $"Заказ с номером {data.ExternalNumber} уже существует, дубль не создан (код {result.Order.PublicCode})"
            : $"Заказ создан, публичный код {result.Order.PublicCode}";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateItems(CreateOrderViewModel m)
    {
        if (m.Items.All(i => i.IsEmpty))
        {
            ModelState.AddModelError("", "Добавьте хотя бы одну позицию");
            return;
        }
        for (int i = 0; i < m.Items.Count; i++)
        {
            var it = m.Items[i];
            if (it.IsEmpty) continue;
            if (string.IsNullOrWhiteSpace(it.Name))
                ModelState.AddModelError($"Items[{i}].Name", "Укажите название");
            if (it.Quantity is null or <= 0 or > 100000)
                ModelState.AddModelError($"Items[{i}].Quantity", "Количество от 1");
            if (it.WeightKg is null or < 0.01m or > 99999m)
                ModelState.AddModelError($"Items[{i}].WeightKg", "Вес от 0,01 кг");
            if (it.LengthCm is null or < 0.01m or > 9999m)
                ModelState.AddModelError($"Items[{i}].LengthCm", "Длина от 0,01 см");
            if (it.WidthCm is null or < 0.01m or > 9999m)
                ModelState.AddModelError($"Items[{i}].WidthCm", "Ширина от 0,01 см");
            if (it.HeightCm is null or < 0.01m or > 9999m)
                ModelState.AddModelError($"Items[{i}].HeightCm", "Высота от 0,01 см");
        }
    }
}