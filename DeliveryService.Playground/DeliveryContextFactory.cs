using Microsoft.EntityFrameworkCore.Design;

namespace DeliveryService.Infrastructure;

public class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("DELIVERY_DB")
            ?? throw new InvalidOperationException("Не задана переменная среды DELIVERY_DB");
        return new DeliveryDbContext(DeliveryDbContext.CreateOptions(cs));
    }
}