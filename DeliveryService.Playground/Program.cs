using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var cs = Environment.GetEnvironmentVariable("DELIVERY_DB")
    ?? throw new InvalidOperationException(
        "Не задана переменная окружения DELIVERY_DB. Задайте её и перезапустите IDE.");

using var db = new DeliveryDbContext(DeliveryDbContext.CreateOptions(cs));
var suffix = Guid.NewGuid().ToString("N")[..6];

// 1. Чтение: первый администратор из скрипта
var admin = db.Users.OfType<Admin>().First();
Console.WriteLine($"Админ найден: {admin.Login}, роль {admin.Role}");

// 2. Запись пользователей
var customer = new Customer($"shop_{suffix}", "hash", "Магазин", "ул. Ленина, 1", "+79990000000");
var courier = new Courier($"c_{suffix}", "hash", "Иван", "Петров", null,
                          "+79991111111", 120, 80, 100, 300);   
db.AddRange(customer, courier);
db.SaveChanges();
Console.WriteLine($"Заказчик Id={customer.Id}, курьер Id={courier.Id}, статус курьера {courier.Status}");

// 3. Запись заказа с позицией
var items = new List<OrderItem> { new("Стул", 2, 5m, 50m, 50m, 90m) };
var order = new Order(customer.Id, $"EXT-{suffix}", customer.PickupAddress, "ул. Мира, 5",
                      "Анна", "Сидорова", null, "+79992222222", items);
db.Add(order);
db.SaveChanges();
Console.WriteLine($"Заказ Id={order.Id}, статус {order.Status}");

// 4. Чтение заказа обратно вместе с позициями
var loaded = db.Orders.Include(o => o.OrderItems).First(o => o.Id == order.Id);
Console.WriteLine($"Позиций: {loaded.OrderItems.Count}, вес {loaded.OrderTotalWeight} кг, " +
                  $"объём {loaded.OrderTotalVolume} м³, ждёт {loaded.WaitTime:hh\\:mm\\:ss}");