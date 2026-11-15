using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Models.Statuses;
using DeliveryService.Infrastructure;
using DeliveryService.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NUnit.Framework;

namespace DeliveryService.Tests.Integration;

public abstract class DbTestBase
{
    private NpgsqlDataSource? _ds;
    protected long CustomerId;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var cs = Environment.GetEnvironmentVariable("DELIVERY_TEST_DB");
        if (string.IsNullOrWhiteSpace(cs))
            Assert.Ignore("Не задана DELIVERY_TEST_DB, интеграционные тесты пропущены");
        if (cs == Environment.GetEnvironmentVariable("DELIVERY_DB"))
            Assert.Fail("DELIVERY_TEST_DB совпадает с DELIVERY_DB: тесты очищают таблицы!");

        _ds = DeliveryDbContext.CreateDataSource(cs!);
        await using var db = NewDb();
        await db.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_ds is not null)
        {
            await _ds.DisposeAsync();
            _ds = null;
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        await using var db = NewDb();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE order_events, order_items, orders RESTART IDENTITY");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM users WHERE role <> 'admin'");

        var shop = new Customer("shop", "x", "Магазин", "Склад, 1", "+70000000001");
        var big = new Courier("courier1", "x", "Иван", "Большов", null, "+70000000002", 300, 180, 180, 1000);
        var small = new Courier("courier2", "x", "Пётр", "Малов", null, "+70000000003", 50, 40, 40, 15);
        big.StartShift();
        small.StartShift();
        db.AddRange(shop, big, small);
        await db.SaveChangesAsync();
        CustomerId = shop.Id;
    }

    protected DeliveryDbContext NewDb()
    {
        var ob = new DbContextOptionsBuilder<DeliveryDbContext>();
        DeliveryDbContext.Configure(ob, _ds!);
        return new DeliveryDbContext(ob.Options);
    }

    // каждый вызов работает в своём контексте, как отдельный веб-запрос
    protected async Task<CreateOrderResult> CreateOrder(string externalNumber, params OrderItemData[] items)
    {
        await using var db = NewDb();
        var service = new OrderService(db, new AssignmentService(db));
        var data = new CreateOrderData(externalNumber, "Адрес доставки", "Сергей", "Иванов",
            null, "+79001234567", items);
        return await service.CreateAsync(CustomerId, data, EventChannel.Web);
    }
}