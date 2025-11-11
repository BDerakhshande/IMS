using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class complateMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StorageSectionId",
                table: "InventoryReceiptLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StorageZoneId",
                table: "InventoryReceiptLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptLogs_StorageSectionId",
                table: "InventoryReceiptLogs",
                column: "StorageSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptLogs_StorageZoneId",
                table: "InventoryReceiptLogs",
                column: "StorageZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptLogs_WarehouseId",
                table: "InventoryReceiptLogs",
                column: "WarehouseId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_StorageSections_StorageSectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_StorageZones_StorageZoneId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_Warehouses_WarehouseId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_StorageSectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_StorageZoneId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_WarehouseId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropColumn(
                name: "StorageSectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropColumn(
                name: "StorageZoneId",
                table: "InventoryReceiptLogs");
        }
    }
}
