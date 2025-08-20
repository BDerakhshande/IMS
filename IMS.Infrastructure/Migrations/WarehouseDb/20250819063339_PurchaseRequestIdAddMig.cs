using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class PurchaseRequestIdAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseRequestId",
                table: "ReceiptOrIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseRequestTitle",
                table: "ReceiptOrIssueItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseRequestId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropColumn(
                name: "PurchaseRequestTitle",
                table: "ReceiptOrIssueItems");
        }
    }
}
