using DeliveryService.Infrastructure;
using DeliveryService.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var cs = Environment.GetEnvironmentVariable("DELIVERY_DB")
    ?? throw new InvalidOperationException("Не задана DELIVERY_DB");
var dataSource = DeliveryDbContext.CreateDataSource(cs);
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<DeliveryDbContext>(o => DeliveryDbContext.Configure(o, dataSource));

builder.Services.AddScoped<AuthService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/Denied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<CourierService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<DeliveryDbContext>());

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // строго до UseAuthorization
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();