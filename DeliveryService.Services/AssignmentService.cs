using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Domain.Services;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services;

public class AssignmentService
{
    public const long LockKey = 1001;
    private readonly DeliveryDbContext _db;
    public AssignmentService(DeliveryDbContext db) => _db = db;

    public async Task<int> RunAsync()
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({LockKey})");

        _db.ChangeTracker.Clear();

        var queue = await _db.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == OrderStatus.Created)
            .OrderBy(o => o.CreatedAt).ThenBy(o => o.Id)
            .ToListAsync();

        var free = await _db.Couriers
            .Where(c => c.IsActive && c.Status == CourierStatus.Free)
            .ToListAsync();

        int assigned = 0;
        foreach (var order in queue)
        {
            if (free.Count == 0) break;

            var courier = free
                .Where(c => CapacityChecker.Fits(order, c))
                .OrderBy(c => c.DeliveriesToday)
                .ThenBy(c => c.BodyVolume * CapacityChecker.FillFactor - order.OrderTotalVolume)
                .ThenBy(c => c.Id)
                .FirstOrDefault();
            if (courier is null) continue;  

            var before = order.Status;
            order.AssignTo(courier);
            courier.TakeOrder();
            _db.OrderEvents.Add(new OrderEvent(order.Id, OrderEventType.Assigned,
                before, order.Status, null, null, EventChannel.System));

            free.Remove(courier);
            assigned++;
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return assigned;
    }
}