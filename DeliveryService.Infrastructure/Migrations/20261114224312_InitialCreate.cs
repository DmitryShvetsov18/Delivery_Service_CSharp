using System;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Domain.Models.Statuses;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeliveryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:courier_state", "busy,free,off_shift")
                .Annotation("Npgsql:Enum:event_channel", "api,system,web")
                .Annotation("Npgsql:Enum:event_type", "assigned,cancelled,created,data_changed,delivered,returned_to_queue")
                .Annotation("Npgsql:Enum:order_status", "cancelled,created,delivered,in_delivery")
                .Annotation("Npgsql:Enum:user_role", "admin,courier,customer,support")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<UserRole>(type: "user_role", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<CourierStatus>(type: "courier_state", nullable: true),
                    body_length_cm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    body_height_cm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    body_width_cm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    carrying_capacity_kg = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    deliveries_today = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pickup_address = table.Column<string>(type: "text", nullable: true),
                    api_key_hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_body_height", "body_height_cm > 0");
                    table.CheckConstraint("ck_users_body_length", "body_length_cm > 0");
                    table.CheckConstraint("ck_users_body_width", "body_width_cm > 0");
                    table.CheckConstraint("ck_users_capacity", "carrying_capacity_kg > 0");
                    table.CheckConstraint("ck_users_courier", "role <> 'courier' OR (first_name IS NOT NULL AND last_name IS NOT NULL AND contact_phone IS NOT NULL AND status IS NOT NULL AND body_length_cm IS NOT NULL AND body_width_cm IS NOT NULL AND body_height_cm IS NOT NULL AND carrying_capacity_kg IS NOT NULL AND deliveries_today IS NOT NULL)");
                    table.CheckConstraint("ck_users_customer", "role <> 'customer' OR (name IS NOT NULL AND contact_phone IS NOT NULL AND pickup_address IS NOT NULL)");
                    table.CheckConstraint("ck_users_deliveries", "deliveries_today > 0");
                    table.CheckConstraint("ck_users_staff", "role NOT IN ('support', 'admin') OR (first_name IS NOT NULL AND last_name IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    courier_id = table.Column<long>(type: "bigint", nullable: true),
                    public_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    external_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    pickup_address = table.Column<string>(type: "text", nullable: false),
                    delivery_address = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<OrderStatus>(type: "order_status", nullable: false),
                    recipients_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recipients_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recipients_middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    recipients_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.CheckConstraint("ck_orders_dates", "(assigned_at IS NULL OR assigned_at >= created_at) AND (completed_at IS NULL OR completed_at >= created_at)");
                    table.CheckConstraint("ck_orders_status_fields", "(status = 'created' AND courier_id IS NULL AND assigned_at IS NULL AND completed_at IS NULL) OR (status = 'in_delivery' AND courier_id IS NOT NULL AND assigned_at IS NOT NULL AND completed_at IS NULL) OR (status = 'delivered' AND courier_id IS NOT NULL AND assigned_at IS NOT NULL AND completed_at IS NOT NULL) OR (status = 'cancelled' AND completed_at IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_orders_couriers_courier_id",
                        column: x => x.courier_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    initiator_id = table.Column<long>(type: "bigint", nullable: true),
                    event_type = table.Column<OrderEventType>(type: "event_type", nullable: false),
                    status_before = table.Column<OrderStatus>(type: "order_status", nullable: true),
                    status_after = table.Column<OrderStatus>(type: "order_status", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    comment = table.Column<string>(type: "text", nullable: true),
                    channel = table.Column<EventChannel>(type: "event_channel", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_events_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_events_users_user_initiator_id",
                        column: x => x.initiator_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_weight_kg = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    unit_length_cm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    unit_width_cm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    unit_height_cm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.CheckConstraint("ck_order_items_height", "unit_height_cm > 0");
                    table.CheckConstraint("ck_order_items_length", "unit_length_cm > 0");
                    table.CheckConstraint("ck_order_items_quantity", "quantity > 0");
                    table.CheckConstraint("ck_order_items_weight", "unit_weight_kg > 0");
                    table.CheckConstraint("ck_order_items_width", "unit_width_cm > 0");
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_events_order",
                table: "order_events",
                columns: new[] { "order_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_order_events_user_initiator_id",
                table: "order_events",
                column: "initiator_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_courier",
                table: "orders",
                columns: new[] { "courier_id", "completed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer",
                table: "orders",
                columns: new[] { "customer_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_orders_public_code",
                table: "orders",
                column: "public_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_queue",
                table: "orders",
                column: "created_at",
                filter: "status = 'created'");

            migrationBuilder.CreateIndex(
                name: "ix_orders_recipient_phone",
                table: "orders",
                column: "recipients_phone_number");

            migrationBuilder.CreateIndex(
                name: "ux_orders_customer_external",
                table: "orders",
                columns: new[] { "customer_id", "external_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_orders_one_active_per_courier",
                table: "orders",
                column: "courier_id",
                unique: true,
                filter: "status = 'in_delivery'");

            migrationBuilder.CreateIndex(
                name: "ix_users_api_key_hash",
                table: "users",
                column: "api_key_hash",
                unique: true);
            // 1. Уникальный логин без учёта регистра
            migrationBuilder.Sql("CREATE UNIQUE INDEX ux_users_login ON users (lower(login));");

            // 2. Роль пользователя не меняется
            migrationBuilder.Sql(@"
CREATE FUNCTION trg_users_role_immutable() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.role <> OLD.role THEN
        RAISE EXCEPTION 'User role cannot be changed';
    END IF;
    RETURN NEW;
END;
$$;");
            migrationBuilder.Sql(@"
CREATE TRIGGER users_role_immutable
    BEFORE UPDATE OF role ON users
    FOR EACH ROW EXECUTE FUNCTION trg_users_role_immutable();");

            // 3. customer_id - заказчик, courier_id - курьер
            migrationBuilder.Sql(@"
CREATE FUNCTION trg_orders_check_roles() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users WHERE id = NEW.customer_id AND role = 'customer') THEN
        RAISE EXCEPTION 'customer_id % is not a customer', NEW.customer_id;
    END IF;
    IF NEW.courier_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM users WHERE id = NEW.courier_id AND role = 'courier') THEN
        RAISE EXCEPTION 'courier_id % is not a courier', NEW.courier_id;
    END IF;
    RETURN NEW;
END;
$$;");
            migrationBuilder.Sql(@"
CREATE TRIGGER orders_check_roles
    BEFORE INSERT OR UPDATE OF customer_id, courier_id ON orders
    FOR EACH ROW EXECUTE FUNCTION trg_orders_check_roles();");

            // 4. У заказа должна быть хотя бы одна позиция (проверка при COMMIT)
            migrationBuilder.Sql(@"
CREATE FUNCTION trg_orders_require_items() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM order_items WHERE order_id = NEW.id) THEN
        RAISE EXCEPTION 'Order % must contain at least one item', NEW.id;
    END IF;
    RETURN NULL;
END;
$$;");
            migrationBuilder.Sql(@"
CREATE CONSTRAINT TRIGGER orders_require_items
    AFTER INSERT ON orders
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION trg_orders_require_items();");

            // 5. Журнал событий неизменяем
            migrationBuilder.Sql(@"
CREATE FUNCTION trg_order_events_immutable() RETURNS trigger
LANGUAGE plpgsql AS $$
BEGIN
    RAISE EXCEPTION 'order_events is append-only';
END;
$$;");
            migrationBuilder.Sql(@"
CREATE TRIGGER order_events_no_update_delete
    BEFORE UPDATE OR DELETE ON order_events
    FOR EACH ROW EXECUTE FUNCTION trg_order_events_immutable();");

            // 6. Первый администратор (admin / admin, пароль сменить после первого входа)
            migrationBuilder.Sql(@"
INSERT INTO users (login, password_hash, role, is_active, first_name, last_name)
VALUES ('admin', crypt('admin', gen_salt('bf', 12)), 'admin', true, 'Системный', 'Администратор');");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS order_events_no_update_delete ON order_events;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS trg_order_events_immutable();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS orders_require_items ON orders;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS trg_orders_require_items();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS orders_check_roles ON orders;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS trg_orders_check_roles();");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS users_role_immutable ON users;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS trg_users_role_immutable();");
            migrationBuilder.DropTable(
                name: "order_events");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
