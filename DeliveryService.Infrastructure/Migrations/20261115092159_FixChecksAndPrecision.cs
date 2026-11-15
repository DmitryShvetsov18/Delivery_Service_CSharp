using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixChecksAndPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_deliveries",
                table: "users");

            migrationBuilder.AlterColumn<decimal>(
                name: "carrying_capacity_kg",
                table: "users",
                type: "numeric(8,3)",
                precision: 8,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_weight_kg",
                table: "order_items",
                type: "numeric(8,3)",
                precision: 8,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2);

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_deliveries",
                table: "users",
                sql: "deliveries_today >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_users_deliveries",
                table: "users");

            migrationBuilder.AlterColumn<decimal>(
                name: "carrying_capacity_kg",
                table: "users",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,3)",
                oldPrecision: 8,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_weight_kg",
                table: "order_items",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,3)",
                oldPrecision: 8,
                oldScale: 3);

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_deliveries",
                table: "users",
                sql: "deliveries_today > 0");
        }
    }
}
