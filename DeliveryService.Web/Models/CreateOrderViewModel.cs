using System.ComponentModel.DataAnnotations;

namespace DeliveryService.Web.Models;

public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Введите внешний номер"), StringLength(64)]
    public string ExternalNumber { get; set; } = "";

    [Required(ErrorMessage = "Введите адрес доставки")]
    public string DeliveryAddress { get; set; } = "";

    [Required(ErrorMessage = "Введите фамилию"), StringLength(100)]
    public string RecipientLastName { get; set; } = "";

    [Required(ErrorMessage = "Введите имя"), StringLength(100)]
    public string RecipientFirstName { get; set; } = "";

    [StringLength(100)]
    public string? RecipientMiddleName { get; set; }

    [Required(ErrorMessage = "Введите телефон")]
    [StringLength(20)]
    [RegularExpression(@"^\+?[0-9\s\-()]{5,20}$", ErrorMessage = "Телефон: цифры, пробелы, +, -, скобки")]
    public string RecipientPhone { get; set; } = "";

    public List<OrderItemInput> Items { get; set; } =
        Enumerable.Range(0, 3).Select(_ => new OrderItemInput()).ToList();
}

public class OrderItemInput
{
    public string? Name { get; set; }
    public int? Quantity { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name) && Quantity is null &&
        WeightKg is null && LengthCm is null && WidthCm is null && HeightCm is null;
}