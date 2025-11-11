using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class ProductAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptLogs_ProductId",
                table: "InventoryReceiptLogs",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptLogs_Products_ProductId",
                table: "InventoryReceiptLogs",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_Products_ProductId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_ProductId",
                table: "InventoryReceiptLogs");
        }
    }
}
