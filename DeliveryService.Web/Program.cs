using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            var cs = Environment.GetEnvironmentVariable("DELIVERY_DB")
    ?? throw new InvalidOperationException("Не задана DELIVERY_DB");
            var dataSource = DeliveryDbContext.CreateDataSource(cs);
            builder.Services.AddSingleton(dataSource);
            builder.Services.AddDbContext<DeliveryDbContext>(o =>
                DeliveryDbContext.Configure(new DbContextOptionsBuilder<DeliveryDbContext>(), dataSource));

            builder.Services.AddControllersWithViews();

            var app = builder.Build();


            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
