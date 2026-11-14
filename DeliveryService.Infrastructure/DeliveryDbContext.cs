using DeliveryService.Domain.Models;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Models.Statuses;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DeliveryService.Infrastructure;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

    public DbSet<SystemUser> Users => Set<SystemUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<SupportEmployee> SupportStaff => Set<SupportEmployee>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderEvent> OrderEvents => Set<OrderEvent>();

    // PG-enum нужно зарегистрировать и в источнике данных, и в модели
    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var b = new NpgsqlDataSourceBuilder(connectionString);
        b.MapEnum<UserRole>("user_role");
        b.MapEnum<CourierStatus>("courier_state");
        b.MapEnum<OrderStatus>("order_status");
        b.MapEnum<OrderEventType>("event_type");
        b.MapEnum<EventChannel>("event_channel");
        return b.Build();
    }
    public static DbContextOptions<DeliveryDbContext> CreateOptions(string connectionString) =>
        new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseNpgsql(CreateDataSource(connectionString), o =>
            {
                o.MapEnum<UserRole>("user_role");
                o.MapEnum<CourierStatus>("courier_state");
                o.MapEnum<OrderStatus>("order_status");
                o.MapEnum<OrderEventType>("event_type");
                o.MapEnum<EventChannel>("event_channel");
            })
            .UseSnakeCaseNamingConvention()
            .Options;

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresEnum<UserRole>("user_role");
        b.HasPostgresEnum<CourierStatus>("courier_state");
        b.HasPostgresEnum<OrderStatus>("order_status");
        b.HasPostgresEnum<OrderEventType>("event_type");
        b.HasPostgresEnum<EventChannel>("event_channel");

        // --- пользователи: одна таблица, дискриминатор = role
        b.Entity<SystemUser>(e =>
        {
            e.ToTable("users");
            e.HasDiscriminator(u => u.Role)
                .HasValue<Customer>(UserRole.Customer)
                .HasValue<Courier>(UserRole.Courier)
                .HasValue<SupportEmployee>(UserRole.Support)
                .HasValue<Admin>(UserRole.Admin);
        });

        // одноимённые свойства соседних подтипов нужно привязать к одной колонке явно
        b.Entity<SupportEmployee>(e =>
        {
            e.Property(x => x.FirstName).HasColumnName("first_name");
            e.Property(x => x.LastName).HasColumnName("last_name");
            e.Property(x => x.MiddleName).HasColumnName("middle_name");
        });
        b.Entity<Courier>(e =>
        {
            e.Property(x => x.FirstName).HasColumnName("first_name");
            e.Property(x => x.LastName).HasColumnName("last_name");
            e.Property(x => x.MiddleName).HasColumnName("middle_name");
            e.Property(x => x.ContactPhone).HasColumnName("contact_phone");
        });
        b.Entity<Customer>().Property(x => x.ContactPhone).HasColumnName("contact_phone");

        // --- заказы
        b.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasOne<Customer>().WithMany().HasForeignKey(o => o.CustomerId);
            e.HasOne<Courier>().WithMany().HasForeignKey(o => o.CourierId);
            e.HasMany(o => o.OrderItems).WithOne().HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Navigation(o => o.OrderItems).HasField("_orderItems")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<OrderItem>().ToTable("order_items");

        // --- журнал
        b.Entity<OrderEvent>(e =>
        {
            e.ToTable("order_events");
            e.Property(x => x.Type).HasColumnName("event_type");
            e.Property(x => x.OrderStatusBeforeEvent).HasColumnName("status_before");
            e.Property(x => x.OrderStatusAfterEvent).HasColumnName("status_after");
            e.Property(x => x.EventDateTime).HasColumnName("occurred_at");
            e.Property(x => x.UserInitiatorId).HasColumnName("initiator_id");
            e.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId);
            e.HasOne(x => x.UserInitiator).WithMany().HasForeignKey(x => x.UserInitiatorId);
        });
    }
}