using DeliveryService.Domain.Models;

namespace DeliveryService.Services;

public record OrderItemData(string Name, int Quantity, decimal WeightKg,
    decimal LengthCm, decimal WidthCm, decimal HeightCm);

public record CreateOrderData(string ExternalNumber, string DeliveryAddress,
    string RecipientFirstName, string RecipientLastName, string? RecipientMiddleName,
    string RecipientPhone, IReadOnlyList<OrderItemData> Items);

public enum CreateOrderStatus { Created, Duplicate }

public record CreateOrderResult(CreateOrderStatus Status, Order Order);