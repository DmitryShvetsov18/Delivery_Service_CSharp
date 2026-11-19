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
    public static DbContextOptions<DeliveryDbContext> CreateOptions(string connectionString)
    {
        var ob = new DbContextOptionsBuilder<DeliveryDbContext>();
        Configure(ob, CreateDataSource(connectionString));
        return ob.Options;
    }

    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder ob, NpgsqlDataSource ds) =>
        ob.UseNpgsql(ds, o =>
        {
            o.MapEnum<UserRole>("user_role");
            o.MapEnum<CourierStatus>("courier_state");
            o.MapEnum<OrderStatus>("order_status");
            o.MapEnum<OrderEventType>("event_type");
            o.MapEnum<EventChannel>("event_channel");
        }).UseSnakeCaseNamingConvention();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("pgcrypto");

        // ================== ПОЛЬЗОВАТЕЛИ (одна таблица, дискриминатор role) ==================
        b.Entity<SystemUser>(e =>
        {
            e.ToTable("users", t =>
            {
                t.HasCheckConstraint("ck_users_customer",
                    "role <> 'customer' OR (name IS NOT NULL AND contact_phone IS NOT NULL AND pickup_address IS NOT NULL)");
                t.HasCheckConstraint("ck_users_staff",
                    "role NOT IN ('support', 'admin') OR (first_name IS NOT NULL AND last_name IS NOT NULL)");
                t.HasCheckConstraint("ck_users_courier",
                    "role <> 'courier' OR (first_name IS NOT NULL AND last_name IS NOT NULL " +
                    "AND contact_phone IS NOT NULL AND status IS NOT NULL " +
                    "AND body_length_cm IS NOT NULL AND body_width_cm IS NOT NULL AND body_height_cm IS NOT NULL " +
                    "AND carrying_capacity_kg IS NOT NULL AND deliveries_today IS NOT NULL)");
                t.HasCheckConstraint("ck_users_body_length", "body_length_cm > 0");
                t.HasCheckConstraint("ck_users_body_width", "body_width_cm > 0");
                t.HasCheckConstraint("ck_users_body_height", "body_height_cm > 0");
                t.HasCheckConstraint("ck_users_capacity", "carrying_capacity_kg > 0");
                t.HasCheckConstraint("ck_users_deliveries", "deliveries_today >= 0");
            });

            e.Property(u => u.Login).HasMaxLength(64);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

            e.HasDiscriminator(u => u.Role)
                .HasValue<Customer>(UserRole.Customer)
                .HasValue<Courier>(UserRole.Courier)
                .HasValue<SupportEmployee>(UserRole.Support)
                .HasValue<Admin>(UserRole.Admin);
        });

        b.Entity<Customer>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            e.Property(x => x.PickupAddress);
            e.HasIndex(x => x.ApiKeyHash).IsUnique();
        });

        b.Entity<Courier>(e =>
        {
            e.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
            e.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
            e.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(100);
            e.Property(x => x.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
            e.Property(x => x.BodyLengthCm).HasPrecision(6, 2);
            e.Property(x => x.BodyWidthCm).HasPrecision(6, 2);
            e.Property(x => x.BodyHeightCm).HasPrecision(6, 2);
            e.Property(x => x.CarryingCapacityKg).HasPrecision(8, 3);
        });

        b.Entity<SupportEmployee>(e =>
        {
            e.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
            e.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
            e.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(100);
        });

        // ================== ЗАКАЗ ==================
        b.Entity<Order>(e =>
        {
            e.ToTable("orders", t =>
            {
                t.HasCheckConstraint("ck_orders_status_fields",
                    "(status = 'created' AND courier_id IS NULL AND assigned_at IS NULL AND completed_at IS NULL) " +
                    "OR (status = 'in_delivery' AND courier_id IS NOT NULL AND assigned_at IS NOT NULL AND completed_at IS NULL) " +
                    "OR (status = 'delivered' AND courier_id IS NOT NULL AND assigned_at IS NOT NULL AND completed_at IS NOT NULL) " +
                    "OR (status = 'cancelled' AND completed_at IS NOT NULL)");
                t.HasCheckConstraint("ck_orders_dates",
                    "(assigned_at IS NULL OR assigned_at >= created_at) AND (completed_at IS NULL OR completed_at >= created_at)");
            });

            e.Property(o => o.PublicCode).HasMaxLength(16);
            e.Property(o => o.ExternalNumber).HasMaxLength(64);
            e.Property(o => o.RecipientsFirstName).HasMaxLength(100);
            e.Property(o => o.RecipientsLastName).HasMaxLength(100);
            e.Property(o => o.RecipientsMiddleName).HasMaxLength(100);
            e.Property(o => o.RecipientsPhoneNumber).HasMaxLength(20);
            e.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
            e.Property(o => o.Version).IsRowVersion();

            e.HasOne<Customer>().WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Courier>().WithMany().HasForeignKey(o => o.CourierId).OnDelete(DeleteBehavior.Restrict);

            e.HasMany(o => o.OrderItems).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.Navigation(o => o.OrderItems).HasField("_orderItems").UsePropertyAccessMode(PropertyAccessMode.Field);

            e.HasIndex(o => new { o.CustomerId, o.ExternalNumber }).IsUnique().HasDatabaseName("ux_orders_customer_external");
            e.HasIndex(o => o.PublicCode).IsUnique();
            e.HasIndex(o => o.CourierId).IsUnique().HasFilter("status = 'in_delivery'")
                .HasDatabaseName("ux_orders_one_active_per_courier");
            e.HasIndex(o => o.CreatedAt).HasFilter("status = 'created'").HasDatabaseName("ix_orders_queue");
            e.HasIndex(o => new { o.CustomerId, o.CreatedAt }).IsDescending(false, true).HasDatabaseName("ix_orders_customer");
            e.HasIndex(o => new { o.CourierId, o.CompletedAt }).HasDatabaseName("ix_orders_courier");
            e.HasIndex(o => o.RecipientsPhoneNumber).HasDatabaseName("ix_orders_recipient_phone");
        });

        // ================== ПОЗИЦИЯ ЗАКАЗА ==================
        b.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items", t =>
            {
                t.HasCheckConstraint("ck_order_items_quantity", "quantity > 0");
                t.HasCheckConstraint("ck_order_items_weight", "unit_weight_kg > 0");
                t.HasCheckConstraint("ck_order_items_length", "unit_length_cm > 0");
                t.HasCheckConstraint("ck_order_items_width", "unit_width_cm > 0");
                t.HasCheckConstraint("ck_order_items_height", "unit_height_cm > 0");
            });
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.UnitWeightKg).HasPrecision(8, 3);
            e.Property(x => x.UnitLengthCm).HasPrecision(6, 2);
            e.Property(x => x.UnitWidthCm).HasPrecision(6, 2);
            e.Property(x => x.UnitHeightCm).HasPrecision(6, 2);
        });

        // ================== ЖУРНАЛ СОБЫТИЙ ==================
        b.Entity<OrderEvent>(e =>
        {
            e.ToTable("order_events");
            e.Property(x => x.Type).HasColumnName("event_type");
            e.Property(x => x.OrderStatusBeforeEvent).HasColumnName("status_before");
            e.Property(x => x.OrderStatusAfterEvent).HasColumnName("status_after");
            e.Property(x => x.EventDateTime).HasColumnName("occurred_at").HasDefaultValueSql("now()");
            e.Property(x => x.UserInitiatorId).HasColumnName("initiator_id");

            e.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.UserInitiator).WithMany().HasForeignKey(x => x.UserInitiatorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.OrderId, x.EventDateTime }).HasDatabaseName("ix_order_events_order");
        });
    }
}