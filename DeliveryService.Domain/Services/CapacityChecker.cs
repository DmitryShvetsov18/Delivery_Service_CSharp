using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Services
{
    public static class CapacityChecker
    {
        public const decimal FillFactor = 0.9m;

        public static bool Fits(Order order, Courier courier)
        {
            if (order.OrderTotalVolume > courier.BodyVolume * FillFactor) return false;
            if (order.OrderTotalWeight > courier.CarryingCapacityKg) return false;
            return order.OrderItems.All(i => ItemFitsBody(i, courier));
        }
        public static bool ItemFitsBody(OrderItem item, Courier courier)
        {
            decimal[] ItemSizes = { item.UnitLengthCm, item.UnitWidthCm, item.UnitHeightCm };

            decimal[] BodySizes = { courier.BodyLengthCm, courier.BodyWidthCm, courier.BodyHeightCm};

            Array.Sort(ItemSizes);
            Array.Sort(BodySizes);
            return ((ItemSizes[0] <= BodySizes[0]) && (ItemSizes[1] <= BodySizes[1]) && (ItemSizes[2] <= BodySizes[2]));
        }
    }
}
