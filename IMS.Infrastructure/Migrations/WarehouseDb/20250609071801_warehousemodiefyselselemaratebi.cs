using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class warehousemodiefyselselemaratebi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationWarehouseId",
                table: "ReceiptOrIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationZoneId",
                table: "ReceiptOrIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceWarehouseId",
                table: "ReceiptOrIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceZoneId",
                table: "ReceiptOrIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_DestinationWarehouseId",
                table: "ReceiptOrIssueItems",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_DestinationZoneId",
                table: "ReceiptOrIssueItems",
                column: "DestinationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_SourceWarehouseId",
                table: "ReceiptOrIssueItems",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_SourceZoneId",
                table: "ReceiptOrIssueItems",
                column: "SourceZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptOrIssueItems_StorageZones_DestinationZoneId",
                table: "ReceiptOrIssueItems",
                column: "DestinationZoneId",
                principalTable: "StorageZones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptOrIssueItems_StorageZones_SourceZoneId",
                table: "ReceiptOrIssueItems",
                column: "SourceZoneId",
                principalTable: "StorageZones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptOrIssueItems_Warehouses_DestinationWarehouseId",
                table: "ReceiptOrIssueItems",
                column: "DestinationWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptOrIssueItems_Warehouses_SourceWarehouseId",
                table: "ReceiptOrIssueItems",
                column: "SourceWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptOrIssueItems_StorageZones_DestinationZoneId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptOrIssueItems_StorageZones_SourceZoneId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptOrIssueItems_Warehouses_DestinationWarehouseId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptOrIssueItems_Warehouses_SourceWarehouseId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptOrIssueItems_DestinationWarehouseId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptOrIssueItems_DestinationZoneId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptOrIssueItems_SourceWarehouseId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptOrIssueItems_SourceZoneId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropColumn(
                name: "DestinationZoneId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropColumn(
                name: "SourceWarehouseId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropColumn(
                name: "SourceZoneId",
                table: "ReceiptOrIssueItems");
        }
    }
}
