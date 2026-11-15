using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Services;
using NUnit.Framework;

namespace DeliveryService.Tests.Services
{
    [TestFixture]
    public class CapacityCheckerTests
    {
        // 100×50×50 см = 0,25 м³ (допустимо 0,225), грузоподъёмность 100 кг
        private static Courier Van() =>
            new("c", "h", "Иван", "Иванов", null, "+7000", 100, 50, 50, 100);

        private static Order Make(OrderItem item) =>
            new(1, "N", "CODE", "a", "b", "Имя", "Фамилия", null, "+7000", new[] { item });

        [Test]
        public void Fits_WithinAllLimits_ReturnsTrue()
        {
            var order = Make(new OrderItem("Коробка", 1, 10m, 50m, 40m, 30m));

            Assert.That(CapacityChecker.Fits(order, Van()), Is.True);
        }

        [Test]
        public void Fits_VolumeExceeded_ReturnsFalse()   // 10 × 60 000 см³ = 0,6 м³
        {
            var order = Make(new OrderItem("Коробки", 10, 1m, 50m, 40m, 30m));

            Assert.That(CapacityChecker.Fits(order, Van()), Is.False);
        }

        [Test]
        public void Fits_WeightExceeded_ReturnsFalse()
        {
            var order = Make(new OrderItem("Груз", 1, 150m, 10m, 10m, 10m));

            Assert.That(CapacityChecker.Fits(order, Van()), Is.False);
        }

        [Test]
        public void Fits_DimensionExceeded_ReturnsFalse()   // 120 см > 100 см
        {
            var order = Make(new OrderItem("Труба", 1, 1m, 120m, 10m, 10m));

            Assert.That(CapacityChecker.Fits(order, Van()), Is.False);
        }

        [Test]
        public void Fits_ItemNeedsRotation_ReturnsTrue()
        {
            var order = Make(new OrderItem("Панель", 1, 1m, 50m, 100m, 40m));

            Assert.That(CapacityChecker.Fits(order, Van()), Is.True);
        }
    }
}