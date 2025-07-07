using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class convensiondocumentMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversionDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversionDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversionDocuments_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_ConversionDocumentId",
                table: "conversionProducedItems",
                column: "ConversionDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_ConversionDocumentId",
                table: "conversionConsumedItems",
                column: "ConversionDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionDocuments_WarehouseId",
                table: "conversionDocuments",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_conversionConsumedItems_conversionDocuments_ConversionDocumentId",
                table: "conversionConsumedItems",
                column: "ConversionDocumentId",
                principalTable: "conversionDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversionProducedItems_conversionDocuments_ConversionDocumentId",
                table: "conversionProducedItems",
                column: "ConversionDocumentId",
                principalTable: "conversionDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversionConsumedItems_conversionDocuments_ConversionDocumentId",
                table: "conversionConsumedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_conversionProducedItems_conversionDocuments_ConversionDocumentId",
                table: "conversionProducedItems");

            migrationBuilder.DropTable(
                name: "conversionDocuments");

            migrationBuilder.DropIndex(
                name: "IX_conversionProducedItems_ConversionDocumentId",
                table: "conversionProducedItems");

            migrationBuilder.DropIndex(
                name: "IX_conversionConsumedItems_ConversionDocumentId",
                table: "conversionConsumedItems");
        }
    }
}
