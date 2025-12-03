using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class CascadeInventoryMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversionConsumedItemUniqueCodes_InventoryItems_InventoryItemId",
                table: "ConversionConsumedItemUniqueCodes");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversionConsumedItemUniqueCodes_InventoryItems_InventoryItemId",
                table: "ConversionConsumedItemUniqueCodes",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversionConsumedItemUniqueCodes_InventoryItems_InventoryItemId",
                table: "ConversionConsumedItemUniqueCodes");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversionConsumedItemUniqueCodes_InventoryItems_InventoryItemId",
                table: "ConversionConsumedItemUniqueCodes",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
