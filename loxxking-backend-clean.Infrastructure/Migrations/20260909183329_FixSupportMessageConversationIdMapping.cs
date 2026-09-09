using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace loxxking_backend_clean.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSupportMessageConversationIdMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data preservation: copy old shadow property values to ConversationId
            migrationBuilder.Sql(@"
                UPDATE SupportMessages 
                SET ConversationId = SupportConversationId 
                WHERE (ConversationId IS NULL OR ConversationId = '00000000-0000-0000-0000-000000000000') 
                AND SupportConversationId IS NOT NULL;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportMessages_SupportConversations_SupportConversationId",
                table: "SupportMessages");

            migrationBuilder.DropIndex(
                name: "IX_SupportMessages_SupportConversationId",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "SupportConversationId",
                table: "SupportMessages");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportMessages_SupportConversations_ConversationId",
                table: "SupportMessages",
                column: "ConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportMessages_SupportConversations_ConversationId",
                table: "SupportMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "SupportConversationId",
                table: "SupportMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessages_SupportConversationId",
                table: "SupportMessages",
                column: "SupportConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportMessages_SupportConversations_SupportConversationId",
                table: "SupportMessages",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Revert data preservation: copy ConversationId back to SupportConversationId
            migrationBuilder.Sql(@"
                UPDATE SupportMessages 
                SET SupportConversationId = ConversationId 
                WHERE SupportConversationId IS NULL 
                AND ConversationId IS NOT NULL 
                AND ConversationId != '00000000-0000-0000-0000-000000000000';
            ");
        }
    }
}
