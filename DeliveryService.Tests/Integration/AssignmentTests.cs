using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DeliveryService.Tests.Integration;

[TestFixture]
public class AssignmentTests : DbTestBase
{
    // заказ А: 0,0216 м³, 2,2 кг
    private static OrderItemData[] OrderA => new[]
    {
        new OrderItemData("Кроссовки", 2, 0.5m, 30, 20, 10),
        new OrderItemData("Куртка", 1, 1.2m, 40, 30, 8)
    };
    // заказ В: холодильник, 0,819 м³, 60 кг, влезает только в большое ТС
    private static OrderItemData[] Fridge => new[] { new OrderItemData("Холодильник", 1, 60m, 70, 65, 180) };
    // заказ Б: книги
    private static OrderItemData[] Books => new[] { new OrderItemData("Книга", 3, 0.35m, 21.5m, 14.5m, 2.5m) };
    private async Task<Order> GetOrder(string ext)
    {
        await using var db = NewDb();
        return await db.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .SingleAsync(o => o.ExternalNumber == ext);
    }

    private async Task<Courier> GetCourier(string login)
    {
        await using var db = NewDb();
        return await db.Couriers.AsNoTracking().SingleAsync(c => c.Login == login);
    }

    [Test]
    public async Task OrderA_GoesToSmallVan_AndWritesEvents()
    {
        await CreateOrder("A", OrderA);

        var order = await GetOrder("A");
        var small = await GetCourier("courier2");
        Assert.That(order.Status, Is.EqualTo(OrderStatus.InDelivery));
        Assert.That(order.CourierId, Is.EqualTo(small.Id));
        Assert.That(order.AssignedAt, Is.Not.Null);
        Assert.That(small.Status, Is.EqualTo(CourierStatus.Busy));
        Assert.That(order.OrderTotalWeight, Is.EqualTo(2.2m));
        Assert.That(order.OrderTotalVolume, Is.EqualTo(0.0216m));

        await using var db = NewDb();
        var events = await db.OrderEvents.Where(e => e.OrderId == order.Id).OrderBy(e => e.Id).ToListAsync();
        Assert.That(events.Select(e => e.Type),
            Is.EqualTo(new[] { OrderEventType.Created, OrderEventType.Assigned }));
        Assert.That(events[1].OrderStatusBeforeEvent, Is.EqualTo(OrderStatus.Created));
        Assert.That(events[1].OrderStatusAfterEvent, Is.EqualTo(OrderStatus.InDelivery));
        Assert.That(events[1].Channel, Is.EqualTo(EventChannel.System));
        Assert.That(events[1].UserInitiatorId, Is.Null);
    }

    [Test]
    public async Task Fridge_GoesToBigVan_WhenSmallIsBusy()
    {
        await CreateOrder("A", OrderA);
        await CreateOrder("BIG", Fridge);

        var big = await GetCourier("courier1");
        var order = await GetOrder("BIG");
        Assert.That(order.CourierId, Is.EqualTo(big.Id));
        Assert.That(order.OrderTotalVolume, Is.EqualTo(0.819m));
    }

    [Test]
    public async Task Order_StaysInQueue_WhenNoFreeCouriers()
    {
        await CreateOrder("A", OrderA);
        await CreateOrder("BIG", Fridge);
        await CreateOrder("BOOKS", Books);

        var order = await GetOrder("BOOKS");
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Created));
        Assert.That(order.CourierId, Is.Null);
        Assert.That(order.AssignedAt, Is.Null);
    }

    [Test]
    public async Task Courier_WithFewerDeliveries_IsChosenFirst()
    {
        await using (var db = NewDb())
            await db.Database.ExecuteSqlRawAsync("UPDATE users SET deliveries_today = 5 WHERE login = 'courier2'");

        await CreateOrder("A", OrderA);

        var big = await GetCourier("courier1");
        Assert.That((await GetOrder("A")).CourierId, Is.EqualTo(big.Id));
    }

    [Test]
    public void TooLongItem_IsRejected_AndNothingSaved()
    {
        Assert.ThatAsync(() => CreateOrder("LONG", new OrderItemData("Балка", 1, 1m, 400, 10, 10)),
            Throws.TypeOf<DeliveryNotPossibleException>());
        AssertNoOrders();
    }

    [Test]
    public void TooHeavyItem_IsRejected_AndNothingSaved()
    {
        Assert.ThatAsync(() => CreateOrder("HEAVY", new OrderItemData("Слиток", 1, 2000m, 10, 10, 10)),
            Throws.TypeOf<DeliveryNotPossibleException>());
        AssertNoOrders();
    }

    [Test]
    public async Task DuplicateExternalNumber_ReturnsExisting_AndDoesNotAssignAgain()
    {
        var first = await CreateOrder("A", OrderA);
        var second = await CreateOrder("A", OrderA);

        Assert.That(first.Status, Is.EqualTo(CreateOrderStatus.Created));
        Assert.That(second.Status, Is.EqualTo(CreateOrderStatus.Duplicate));
        Assert.That(second.Order.PublicCode, Is.EqualTo(first.Order.PublicCode));

        await using var db = NewDb();
        Assert.That(await db.Orders.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.OrderEvents.CountAsync(), Is.EqualTo(2));  
    }

    private void AssertNoOrders()
    {
        using var db = NewDb();
        Assert.That(db.Orders.Count(), Is.EqualTo(0));
    }
}