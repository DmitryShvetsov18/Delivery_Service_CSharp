using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services;

public class CourierService
{
    private readonly DeliveryDbContext _db;
    private readonly AssignmentService _assignment;

    public CourierService(DeliveryDbContext db, AssignmentService assignment)
    {
        _db = db;
        _assignment = assignment;
    }

    public async Task StartShiftAsync(long courierId)
    {
        var courier = await GetCourierAsync(courierId);
        courier.StartShift();
        await _db.SaveChangesAsync();
        await _assignment.RunAsync();  
    }

    public async Task EndShiftAsync(long courierId)
    {
        var courier = await GetCourierAsync(courierId);
        courier.EndShift();
        await _db.SaveChangesAsync();
    }

    public async Task CompleteDeliveryAsync(long courierId)
    {
        var courier = await GetCourierAsync(courierId);
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.CourierId == courierId && o.Status == OrderStatus.InDelivery)
            ?? throw new InvalidOperationException("У вас нет заказа в доставке");

        var before = order.Status;
        order.Complete();
        courier.FinishDelivery();
        _db.OrderEvents.Add(new OrderEvent(order.Id, OrderEventType.Delivered,
            before, order.Status, courier, null, EventChannel.Web));

        await _db.SaveChangesAsync();   
        await _assignment.RunAsync();
    }

    private async Task<Domain.Models.Roles.Courier> GetCourierAsync(long id) =>
        await _db.Couriers.FirstOrDefaultAsync(c => c.Id == id && c.IsActive)
            ?? throw new InvalidOperationException("Курьер не найден или заблокирован");
}