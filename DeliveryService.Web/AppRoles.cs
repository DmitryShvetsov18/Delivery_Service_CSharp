namespace DeliveryService.Web;

public static class AppRoles
{
    public const string Customer = "Customer";
    public const string Courier = "Courier";
    public const string Support = "Support";
    public const string Admin = "Admin";
    public const string Staff = Support + "," + Admin;  
}