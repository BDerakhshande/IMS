using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.ProcurementManagementDb
{
    /// <inheritdoc />
    public partial class IsFullySuppliedAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFullySupplied",
                table: "PurchaseRequestItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFullySupplied",
                table: "PurchaseRequestItems");
        }
    }
}
