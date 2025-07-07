using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class convensionproductMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_ProductId",
                table: "conversionProducedItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_ProductId",
                table: "conversionConsumedItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_conversionConsumedItems_Products_ProductId",
                table: "conversionConsumedItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_conversionProducedItems_Products_ProductId",
                table: "conversionProducedItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversionConsumedItems_Products_ProductId",
                table: "conversionConsumedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_conversionProducedItems_Products_ProductId",
                table: "conversionProducedItems");

            migrationBuilder.DropIndex(
                name: "IX_conversionProducedItems_ProductId",
                table: "conversionProducedItems");

            migrationBuilder.DropIndex(
                name: "IX_conversionConsumedItems_ProductId",
                table: "conversionConsumedItems");
        }
    }
}
