using System;
using System.Collections.Generic;
using System.Text;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Models.Statuses;

namespace DeliveryService.Domain.Models
{
    public class OrderEvent
    {
        private OrderEvent () { }
        public OrderEvent(long orderId,OrderEventType type, OrderStatus? orderStatusBeforeEvent, OrderStatus? orderStatusAfterEvent, SystemUser? userInitiator, string? comment, EventChannel eventChannel)
        {
            if (userInitiator is SupportEmployee && string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Для действий поддержки нужен комментарий", nameof(comment));
            OrderId = orderId;
            Type = type;
            OrderStatusBeforeEvent = orderStatusBeforeEvent;
            OrderStatusAfterEvent = orderStatusAfterEvent;
            EventDateTime = DateTime.UtcNow;
            UserInitiator = userInitiator;
            Comment = comment;
            Channel = eventChannel;
        }

        public long Id { get; private set; }
        public long OrderId { get; private set; }
        public long? UserInitiatorId { get; private set; }
        public OrderEventType Type { get; set; }

        public OrderStatus? OrderStatusBeforeEvent { get; set; }
        public OrderStatus? OrderStatusAfterEvent { get; set; }
        public DateTime EventDateTime { get; set; }
        public SystemUser? UserInitiator { get; set; }
        public string? Comment { get; set; }
        public EventChannel Channel { get; set; }
    }
}
