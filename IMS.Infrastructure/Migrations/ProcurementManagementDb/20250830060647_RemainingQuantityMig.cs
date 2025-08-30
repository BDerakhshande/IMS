using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.ProcurementManagementDb
{
    /// <inheritdoc />
    public partial class RemainingQuantityMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "PurchaseRequestItems",
                newName: "RemainingQuantity");

            migrationBuilder.AddColumn<decimal>(
                name: "InitialQuantity",
                table: "PurchaseRequestItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialQuantity",
                table: "PurchaseRequestItems");

            migrationBuilder.RenameColumn(
                name: "RemainingQuantity",
                table: "PurchaseRequestItems",
                newName: "Quantity");
        }
    }
}
