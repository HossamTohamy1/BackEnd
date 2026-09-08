using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace loxxking_backend_clean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CountryId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ShipmentCode",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CountryId_Status_PaymentMethod_CreatedAt",
                table: "Orders",
                columns: new[] { "CountryId", "Status", "PaymentMethod", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShipmentCode",
                table: "Orders",
                column: "ShipmentCode",
                unique: true,
                filter: "[ShipmentCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CountryId_Status_PaymentMethod_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShipmentCode",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ShipmentCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CountryId",
                table: "Orders",
                column: "CountryId");
        }
    }
}
