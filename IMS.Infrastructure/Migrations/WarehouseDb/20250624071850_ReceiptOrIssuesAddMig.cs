using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class ReceiptOrIssuesAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptOrIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptOrIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptOrIssueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptOrIssueId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: true),
                    SourceZoneId = table.Column<int>(type: "int", nullable: true),
                    SourceSectionId = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DestinationZoneId = table.Column<int>(type: "int", nullable: true),
                    DestinationSectionId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptOrIssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_ReceiptOrIssues_ReceiptOrIssueId",
                        column: x => x.ReceiptOrIssueId,
                        principalTable: "ReceiptOrIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageSections_DestinationSectionId",
                        column: x => x.DestinationSectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageSections_SourceSectionId",
                        column: x => x.SourceSectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageZones_DestinationZoneId",
                        column: x => x.DestinationZoneId,
                        principalTable: "StorageZones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageZones_SourceZoneId",
                        column: x => x.SourceZoneId,
                        principalTable: "StorageZones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_CategoryId",
                table: "ReceiptOrIssueItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_DestinationSectionId",
                table: "ReceiptOrIssueItems",
                column: "DestinationSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_DestinationWarehouseId",
                table: "ReceiptOrIssueItems",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_DestinationZoneId",
                table: "ReceiptOrIssueItems",
                column: "DestinationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_GroupId",
                table: "ReceiptOrIssueItems",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_ProductId",
                table: "ReceiptOrIssueItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_ReceiptOrIssueId",
                table: "ReceiptOrIssueItems",
                column: "ReceiptOrIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_SourceSectionId",
                table: "ReceiptOrIssueItems",
                column: "SourceSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_SourceWarehouseId",
                table: "ReceiptOrIssueItems",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_SourceZoneId",
                table: "ReceiptOrIssueItems",
                column: "SourceZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItems_StatusId",
                table: "ReceiptOrIssueItems",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptOrIssueItems");

            migrationBuilder.DropTable(
                name: "ReceiptOrIssues");
        }
    }
}
