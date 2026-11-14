using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryService.Domain.Models.Statuses
{
    public enum OrderEventType
    {
        Created,
        Assigned,
        Delivered,
        Cancelled,
        ReturnedToQueue,
        DataChanged
    }
}
