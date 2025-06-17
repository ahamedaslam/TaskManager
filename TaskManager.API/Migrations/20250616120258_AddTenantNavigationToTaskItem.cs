using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantNavigationToTaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "TaskItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_TenantId",
                table: "TaskItems",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Tenants_TenantId",
                table: "TaskItems",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Tenants_TenantId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_TenantId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaskItems");
        }
    }
}
