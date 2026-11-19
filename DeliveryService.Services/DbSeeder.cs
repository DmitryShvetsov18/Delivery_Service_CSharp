using DeliveryService.Domain.Models.Roles;
using DeliveryService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(DeliveryDbContext db)
        {
            static string H(string p) => BCrypt.Net.BCrypt.HashPassword(p);

            if (!await db.Users.AnyAsync(u => u.Login == "shop"))
                db.Users.Add(new Customer("shop", H("shop123"),
                    "Тестовый магазин", "г. Владимир, ул. Складская, 1", "+70000000001"));

            if (!await db.Users.AnyAsync(u => u.Login == "shop2"))
                db.Users.Add(new Customer("shop2", H("shop123"),
                    "Второй магазин", "г. Владимир, ул. Мира, 2", "+70000000004"));

            // большой фургон (м) и малое ТС
            if (!await db.Users.AnyAsync(u => u.Login == "courier1"))
                db.Users.Add(new Courier("courier1", H("courier123"),
                    "Иван", "Большов", null, "+70000000002", 300, 180, 180, 1000));

            if (!await db.Users.AnyAsync(u => u.Login == "courier2"))
                db.Users.Add(new Courier("courier2", H("courier123"),
                    "Пётр", "Малов", null, "+70000000003", 50, 40, 40, 15));

            if (!await db.Users.AnyAsync(u => u.Login == "support"))
                db.Users.Add(new SupportEmployee("support", H("support123"),
                    "Анна", "Поддержкина", null));

            await db.SaveChangesAsync();
        }
    }
}
