using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class InventoryReceiptLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_StorageSections_StorageSectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_StorageZones_StorageZoneId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_StorageSectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_StorageZoneId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropColumn(
                name: "StorageSectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropColumn(
                name: "StorageZoneId",
                table: "InventoryReceiptLogs");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptLogs_SectionId",
                table: "InventoryReceiptLogs",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptLogs_ZoneId",
                table: "InventoryReceiptLogs",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptLogs_StorageSections_SectionId",
                table: "InventoryReceiptLogs",
                column: "SectionId",
                principalTable: "StorageSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptLogs_StorageZones_ZoneId",
                table: "InventoryReceiptLogs",
                column: "ZoneId",
                principalTable: "StorageZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_StorageSections_SectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryReceiptLogs_StorageZones_ZoneId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_SectionId",
                table: "InventoryReceiptLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryReceiptLogs_ZoneId",
                table: "InventoryReceiptLogs");

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

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptLogs_StorageSections_StorageSectionId",
                table: "InventoryReceiptLogs",
                column: "StorageSectionId",
                principalTable: "StorageSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryReceiptLogs_StorageZones_StorageZoneId",
                table: "InventoryReceiptLogs",
                column: "StorageZoneId",
                principalTable: "StorageZones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
