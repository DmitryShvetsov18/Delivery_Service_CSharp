using System;
using System.Collections.Generic;
using System.Text;
using DeliveryService.Domain.Models;

namespace DeliveryService.Domain.Services
{
    public static class OrderCalculator
    {
        private const decimal CubicCmInCubicM = 1_000_000m;
        public static decimal CalculateOrderItemTotalVolume(OrderItem item)
        {
            return item.Quantity * item.UnitLengthCm * item.UnitWidthCm * item.UnitHeightCm / CubicCmInCubicM;
        }
        public static decimal CalculateOrderItemUnitVolume(OrderItem item)
        {
            return item.UnitHeightCm * item.UnitLengthCm * item.UnitWidthCm / CubicCmInCubicM;
        }

        public static decimal CalculateOrderTotalVolume(IEnumerable<OrderItem> orderItems)
        {   
            decimal sum = 0;
            foreach (OrderItem item in orderItems)
            {
                sum = sum + CalculateOrderItemTotalVolume(item);
            }
            return sum;
        }

        public static decimal CalculateOrderItemTotalWeight(OrderItem item)
        {
            return item.Quantity * item.UnitWeightKg;
        }

        public static decimal CalculateOrderTotalWeight(IEnumerable<OrderItem> orders)
        {
            decimal sum = 0;
            foreach (OrderItem item in orders)
            {
                sum = sum + CalculateOrderItemTotalWeight(item);
            }
            return sum; 
        }
    }
}
