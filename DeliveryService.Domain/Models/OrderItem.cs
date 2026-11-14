using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Models
{
    public class OrderItem
    {
        private OrderItem () { }
        public OrderItem(string name, int quantity, decimal unitWeightKg, decimal unitLengthCm, decimal unitWidthCm, decimal unitHeightCm)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название обязательно", nameof(name));
            Name = name;
            Name = name;
            Quantity = Positive(quantity, nameof(quantity));
            UnitWeightKg = Positive(unitWeightKg, nameof(unitWeightKg));
            UnitLengthCm = Positive(unitHeightCm, nameof(unitHeightCm));
            UnitWidthCm = Positive(unitWidthCm, nameof(unitWidthCm));
            UnitHeightCm = Positive(unitHeightCm, nameof(unitHeightCm));
        }
        public long Id { get; private set; }
        public long OrderId { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitWeightKg { get; private set; }
        public decimal UnitLengthCm { get; private set; }
        public decimal UnitWidthCm { get; private set; }
        public decimal UnitHeightCm { get; private set; }
        public decimal UnitVolume => Services.OrderCalculator.CalculateOrderItemUnitVolume(this);
        public decimal TotalVolume => Services.OrderCalculator.CalculateOrderItemTotalVolume(this);
        public decimal TotalWeight => Services.OrderCalculator.CalculateOrderItemTotalWeight(this);

        private static T Positive<T>(T value, string name) where T : struct, IComparable<T> =>
    value.CompareTo(default) > 0 ? value
        : throw new ArgumentOutOfRangeException(name, "Значение должно быть больше 0");
    }
}
