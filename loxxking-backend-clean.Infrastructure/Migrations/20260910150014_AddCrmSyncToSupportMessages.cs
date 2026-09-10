using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace loxxking_backend_clean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmSyncToSupportMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientMessageId",
                table: "SupportMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSyncedToCrm",
                table: "SupportMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SyncAttempts",
                table: "SupportMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SyncError",
                table: "SupportMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedToCrmAt",
                table: "SupportMessages",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientMessageId",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "IsSyncedToCrm",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "SyncAttempts",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "SyncError",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "SyncedToCrmAt",
                table: "SupportMessages");
        }
    }
}
