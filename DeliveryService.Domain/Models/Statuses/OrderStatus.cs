using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Models.Statuses
{
    public enum OrderStatus
    {
        Created,
        InDelivery,
        Delivered,
        Cancelled
    }
}
