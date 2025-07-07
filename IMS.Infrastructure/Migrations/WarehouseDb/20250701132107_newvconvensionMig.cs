using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class newvconvensionMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversionDocuments_Warehouses_WarehouseId",
                table: "conversionDocuments");

            migrationBuilder.DropIndex(
                name: "IX_conversionDocuments_WarehouseId",
                table: "conversionDocuments");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "conversionDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "conversionDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_conversionDocuments_WarehouseId",
                table: "conversionDocuments",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_conversionDocuments_Warehouses_WarehouseId",
                table: "conversionDocuments",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
