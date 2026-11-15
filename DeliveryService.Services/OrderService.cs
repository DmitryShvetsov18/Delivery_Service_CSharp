using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services;

public class OrderService
{
    private readonly DeliveryDbContext _db;
    public OrderService(DeliveryDbContext db) => _db = db;

    public async Task<CreateOrderResult> CreateAsync(long customerId, CreateOrderData data, EventChannel channel)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.IsActive)
            ?? throw new InvalidOperationException("Заказчик не найден или заблокирован");

        var existing = await FindAsync(customerId, data.ExternalNumber);
        if (existing is not null) return new(CreateOrderStatus.Duplicate, existing);

        var items = data.Items
            .Select(i => new OrderItem(i.Name, i.Quantity, i.WeightKg, i.LengthCm, i.WidthCm, i.HeightCm))
            .ToList();

        var order = new Order(customer.Id, data.ExternalNumber, PublicCodeGenerator.Generate(),
            customer.PickupAddress, data.DeliveryAddress, data.RecipientFirstName,
            data.RecipientLastName, data.RecipientMiddleName, data.RecipientPhone, items);

        // TODO: проверка возможности доставки (ни один курьер не подходит => отказ)

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();   

            _db.OrderEvents.Add(new OrderEvent(order.Id, OrderEventType.Created,
                null, OrderStatus.Created, customer, null, channel));
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return new(CreateOrderStatus.Created, order);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            var dup = await FindAsync(customerId, data.ExternalNumber);
            if (dup is not null) return new(CreateOrderStatus.Duplicate, dup);
            throw;
        }
    }

    private Task<Order?> FindAsync(long customerId, string externalNumber) =>
        _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.ExternalNumber == externalNumber);
}