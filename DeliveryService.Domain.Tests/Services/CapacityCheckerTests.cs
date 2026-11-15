using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Services;

namespace DeliveryService.Domain.Tests.Services
{
    public class CapacityCheckerTests
    {
        [Test]
        public void ItemsFitsBody_FitsAny_True()
        {
            OrderItem item = new OrderItem("Телевизор", 1, 7.5m, 120m, 70m, 30m);
            Courier courier = new Courier($"c_тест", "hash", "Иван", "Петров", null,
                          "+79991111111", 120, 80, 100, 300);

            bool result = CapacityChecker.ItemFitsBody(item, courier);

            Assert.That(result, Is.True);
        }
        [Test]
        public void ItemsFitsBody_NotFits_False()
        {
            OrderItem item = new OrderItem("Телевизор", 1, 7.5m, 120m, 70m, 30m);
            Courier courier = new Courier($"c_тест", "hash", "Иван", "Петров", null,
                          "+79991111111", 120, 80, 100, 300);

            bool result = CapacityChecker.ItemFitsBody(item, courier);

            Assert.That(result, Is.False);
        }
        [Test]
        public void ItemsFitsBody_FitsWithRotate_True()
        {
            OrderItem item = new OrderItem("Телевизор", 1, 7.5m, 120m, 70m, 30m);
            Courier courier = new Courier($"c_тест", "hash", "Иван", "Петров", null,
                          "+79991111111", 120, 80, 100, 300);

            bool result = CapacityChecker.ItemFitsBody(item, courier);

            Assert.That(result, Is.True);
        }

    }
}
