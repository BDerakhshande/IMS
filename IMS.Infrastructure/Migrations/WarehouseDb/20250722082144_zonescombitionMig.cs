using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class zonescombitionMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageZones_WarehouseId",
                table: "StorageZones");

            migrationBuilder.DropIndex(
                name: "IX_StorageZones_ZoneCode",
                table: "StorageZones");

            migrationBuilder.CreateIndex(
                name: "IX_StorageZones_WarehouseId_ZoneCode",
                table: "StorageZones",
                columns: new[] { "WarehouseId", "ZoneCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageZones_WarehouseId_ZoneCode",
                table: "StorageZones");

            migrationBuilder.CreateIndex(
                name: "IX_StorageZones_WarehouseId",
                table: "StorageZones",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageZones_ZoneCode",
                table: "StorageZones",
                column: "ZoneCode",
                unique: true);
        }
    }
}
