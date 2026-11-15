using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeliveryService.Domain.Models;
using DeliveryService.Domain.Services;

namespace DeliveryService.Tests.Services
{
    [TestFixture]
    public class OrderCalculatorTests
    {
        [Test]
        public void CalculateOrderItemTotalVolume_TwoBoxs_ReturnsBoxVolume() 
        {
            // Arrange (подготовка)
            OrderItem item = new OrderItem("Коробка", 2, 2.5m, 50m, 40m, 30m);

            // Act (действие)
            decimal result = OrderCalculator.CalculateOrderItemTotalVolume(item);

            // Assert (проверка)
            Assert.That(result, Is.EqualTo(0.12m));

        }

        [Test]
        public void CalculateOrderItemUnitVolume_TwoBoxs_ReturnsOrderItemVolume()
        {
            OrderItem item = new OrderItem("Коробка", 2, 2.5m, 50m, 40m, 30m);
            
            decimal result = OrderCalculator.CalculateOrderItemUnitVolume(item);

            Assert.That(result, Is.EqualTo(0.06m));
        }


        [Test]
        public void CalculateOrderTotalVolume_2Pos_ReturnsTotalVolume()
        {
            OrderItem item1 = new OrderItem("Коробка", 2, 2.5m, 50m, 40m, 30m);
            OrderItem item2 = new OrderItem("Телевизор", 1, 7.5m, 120m, 70m, 30m);
            
            List<OrderItem> orders = new List<OrderItem>();
            orders.Add(item1);
            orders.Add(item2);

            decimal result = OrderCalculator.CalculateOrderTotalVolume(orders);

            Assert.That(result, Is.EqualTo(0.372m));
        }

        [Test]
        public void CalculateOrderTotalWeight_2Pos_ReturnsTotalWeight()
        {
            OrderItem item1 = new OrderItem("Коробка", 2, 2.5m, 50m, 40m, 30m);
            OrderItem item2 = new OrderItem("Телевизор", 1, 7.5m, 120m, 70m, 30m);

            List<OrderItem> orders = new List<OrderItem>();
            orders.Add(item1);
            orders.Add(item2);

            decimal result = OrderCalculator.CalculateOrderTotalWeight(orders);

            Assert.That(result, Is.EqualTo(12.5m));
        }
    }
}
