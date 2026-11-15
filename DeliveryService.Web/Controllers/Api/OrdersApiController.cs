using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Services;
using DeliveryService.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Web.Controllers.Api;

[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly ApiKeyService _keys;
    private readonly OrderService _orders;

    public OrdersApiController(ApiKeyService keys, OrderService orders)
    {
        _keys = keys;
        _orders = orders;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApiOrderRequest req)
    {

        if (!Request.Headers.TryGetValue("X-Api-Key", out var key) || string.IsNullOrWhiteSpace(key))
            return Unauthorized(new { error = "Не передан заголовок X-Api-Key" });

        var customer = await _keys.FindCustomerAsync(key.ToString());
        if (customer is null)
            return Unauthorized(new { error = "Неверный ключ" });
        if (!customer.IsActive)
            return StatusCode(403, new { error = "Заказчик заблокирован" });

        var items = req.Items
            .Select(i => new OrderItemData(i.Name.Trim(), i.Quantity, i.WeightKg,
                i.LengthCm, i.WidthCm, i.HeightCm))
            .ToList();

        var data = new CreateOrderData(req.ExternalNumber.Trim(), req.DeliveryAddress.Trim(),
            req.Recipient.FirstName.Trim(), req.Recipient.LastName.Trim(),
            string.IsNullOrWhiteSpace(req.Recipient.MiddleName) ? null : req.Recipient.MiddleName.Trim(),
            req.Recipient.Phone.Trim(), items);

        try
        {
            var result = await _orders.CreateAsync(customer.Id, data, EventChannel.Api);
            var body = new
            {
                publicCode = result.Order.PublicCode,
                status = StatusName(result.Order.Status),
                externalNumber = result.Order.ExternalNumber
            };
            return result.Status == CreateOrderStatus.Created ? StatusCode(201, body) : Ok(body);
        }
        catch (DeliveryNotPossibleException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    private static string StatusName(OrderStatus s) => s switch
    {
        OrderStatus.Created => "created",
        OrderStatus.InDelivery => "in_delivery",
        OrderStatus.Delivered => "delivered",
        OrderStatus.Cancelled => "cancelled",
        _ => s.ToString()
    };
}