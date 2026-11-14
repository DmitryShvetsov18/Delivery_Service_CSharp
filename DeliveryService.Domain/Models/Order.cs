using DeliveryService.Domain.Models.Statuses;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DeliveryService.Domain.Models
{
    public class Order
    {
        private Order() { } 
        public Order(long customerId, string externalNumber, string pickupAddress, string deliveryAddress, string recipientsFirstName, string recipientsLastName, string recipientsMiddleName, string recipientsPhoneNumber, IReadOnlyList<OrderItem> orderItems)
        {
            if (orderItems is null || orderItems.Count == 0)
                throw new ArgumentException("Заказ должен содержать хотя бы одну позицию", nameof(orderItems));

            CustomerId = customerId;
            ExternalNumber = externalNumber;
            PickupAddress = pickupAddress;
            DeliveryAddress = deliveryAddress;
            Status = OrderStatus.Created;
            RecipientsFirstName = recipientsFirstName;
            RecipientsLastName = recipientsLastName;
            RecipientsMiddleName = recipientsMiddleName;
            RecipientsPhoneNumber = recipientsPhoneNumber;
            CreatedAt = DateTime.UtcNow;
            _orderItems.AddRange(orderItems);
        }

        public long Id { get; private set; }
        public long CustomerId { get; private set; }
        public long? CourierId { get; private set; }
        public string? PublicCode { get; private set; }
        public string ExternalNumber { get; private set; }
        public string PickupAddress { get; private set; }
        public string DeliveryAddress { get; set; } = null!;
        public OrderStatus Status { get; private set; }
        public string RecipientsFirstName { get; private set; } = null!;
        public string RecipientsLastName { get; private set; } = null!;
        public string? RecipientsMiddleName { get; private set; } = null!;
        public string RecipientsPhoneNumber { get; private set; } = null!;
        private readonly List<OrderItem> _orderItems = new List<OrderItem>();
        public IReadOnlyList<OrderItem> OrderItems => _orderItems;
        public DateTime CreatedAt { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        
        public decimal OrderTotalWeight => Services.OrderCalculator.CalculateOrderTotalWeight(this.OrderItems);
        public decimal OrderTotalVolume => Services.OrderCalculator.CalculateOrderTotalVolume(this.OrderItems);
        public TimeSpan WaitTime =>
            (AssignedAt ?? CompletedAt ?? DateTime.UtcNow) - CreatedAt;
        public TimeSpan? DeliveryTime =>
    AssignedAt is null ? null : (CompletedAt ?? DateTime.UtcNow) - AssignedAt.Value;
    }
}
