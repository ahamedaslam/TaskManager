using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToChatHIstoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "ChatHistories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistories_TenantId",
                table: "ChatHistories",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatHistories_Tenants_TenantId",
                table: "ChatHistories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatHistories_Tenants_TenantId",
                table: "ChatHistories");

            migrationBuilder.DropIndex(
                name: "IX_ChatHistories_TenantId",
                table: "ChatHistories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ChatHistories");
        }
    }
}
