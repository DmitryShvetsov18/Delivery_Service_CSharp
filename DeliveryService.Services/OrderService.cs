using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Domain.Services;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services;

public class OrderService
{
    private readonly DeliveryDbContext _db;
    private readonly AssignmentService _assignment;

    public OrderService(DeliveryDbContext db, AssignmentService assignment)
    {
        _db = db;
        _assignment = assignment;
    }

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

        var fleet = await _db.Couriers.AsNoTracking().Where(c => c.IsActive).ToListAsync();
        if (!fleet.Any(c => CapacityChecker.Fits(order, c)))
            throw new DeliveryNotPossibleException();

        await using (var tx = await _db.Database.BeginTransactionAsync())
        {
            try
            {
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                _db.OrderEvents.Add(new OrderEvent(order.Id, OrderEventType.Created,
                    null, OrderStatus.Created, customer, null, channel));
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
            }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                var dup = await FindAsync(customerId, data.ExternalNumber);
                if (dup is not null) return new(CreateOrderStatus.Duplicate, dup);
                throw;
            }
        }

        await _assignment.RunAsync();   

        var fresh = await FindAsync(customerId, data.ExternalNumber);
        return new(CreateOrderStatus.Created, fresh ?? order);
    }

    public async Task<CancelResult> CancelAsync(long customerId, long orderId, EventChannel channel)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({AssignmentService.LockKey})");
        _db.ChangeTracker.Clear();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);
        if (order is null) return CancelResult.NotFound;
        if (!OrderStateRules.CanCancelByCustomer(order.Status)) return CancelResult.NotAllowed;

        var customer = await _db.Customers.FirstAsync(c => c.Id == customerId);
        var before = order.Status;
        order.Cancel();
        _db.OrderEvents.Add(new OrderEvent(order.Id, OrderEventType.Cancelled,
            before, order.Status, customer, null, channel));

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return CancelResult.Cancelled;
    }


    private Task<Order?> FindAsync(long customerId, string externalNumber) =>
        _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.ExternalNumber == externalNumber);
}