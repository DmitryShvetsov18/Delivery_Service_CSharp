using System.ComponentModel.DataAnnotations;

namespace DeliveryService.Web.Models;

public class ApiOrderRequest
{
    [Required, StringLength(64)]
    public string ExternalNumber { get; set; } = "";

    [Required]
    public string DeliveryAddress { get; set; } = "";

    [Required]
    public ApiRecipient Recipient { get; set; } = null!;

    [Required, MinLength(1, ErrorMessage = "Нужна хотя бы одна позиция")]
    public List<ApiOrderItem> Items { get; set; } = new();
}

public class ApiRecipient
{
    [Required, StringLength(100)] public string LastName { get; set; } = "";
    [Required, StringLength(100)] public string FirstName { get; set; } = "";
    [StringLength(100)] public string? MiddleName { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^\+?[0-9\s\-()]{5,20}$", ErrorMessage = "Телефон: цифры, пробелы, +, -, скобки")]
    public string Phone { get; set; } = "";
}

public class ApiOrderItem
{
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Range(1, 100000)] public int Quantity { get; set; }
    [Range(0.01, 99999)] public decimal WeightKg { get; set; }
    [Range(0.01, 9999)] public decimal LengthCm { get; set; }
    [Range(0.01, 9999)] public decimal WidthCm { get; set; }
    [Range(0.01, 9999)] public decimal HeightCm { get; set; }
}