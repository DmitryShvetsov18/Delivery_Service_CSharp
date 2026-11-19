using DeliveryService.Domain.Models.Statuses;

namespace DeliveryService.Domain.Services;

public static class OrderStateRules
{
    public static bool IsFinal(OrderStatus s) => s is OrderStatus.Delivered or OrderStatus.Cancelled;
    public static bool CanAssign(OrderStatus s) => s == OrderStatus.Created;
    public static bool CanComplete(OrderStatus s) => s == OrderStatus.InDelivery;
    public static bool CanCancelByCustomer(OrderStatus s) => s == OrderStatus.Created;
    public static bool CanCancelBySupport(OrderStatus s) => s is OrderStatus.Created or OrderStatus.InDelivery;
    public static bool CanChangeDeliveryData(OrderStatus s) => s is OrderStatus.Created or OrderStatus.InDelivery;
    public static bool CanReturnToQueue(OrderStatus s) => s == OrderStatus.InDelivery;
}