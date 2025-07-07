using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class StatausModMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StstusId",
                table: "conversionProducedItems",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "StstusId",
                table: "conversionConsumedItems",
                newName: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "conversionProducedItems",
                newName: "StstusId");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "conversionConsumedItems",
                newName: "StstusId");
        }
    }
}
