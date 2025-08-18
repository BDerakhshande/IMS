using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.ProcurementManagementDb
{
    /// <inheritdoc />
    public partial class StatusIdAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "PurchaseRequestFlatItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "PurchaseRequestFlatItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "PurchaseRequestFlatItems");

            migrationBuilder.DropColumn(
                name: "StatusName",
                table: "PurchaseRequestFlatItems");
        }
    }
}
