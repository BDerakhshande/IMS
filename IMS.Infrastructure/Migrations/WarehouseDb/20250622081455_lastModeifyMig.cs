using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class lastModeifyMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryAdjustments");

            migrationBuilder.DropTable(
                name: "InventoryConversionDetails");

            migrationBuilder.DropTable(
                name: "ReceiptOrIssueItems");

            migrationBuilder.DropTable(
                name: "WarehouseTransferItems");

            migrationBuilder.DropTable(
                name: "WarehouseInventories");

            migrationBuilder.DropTable(
                name: "InventoryConversions");

            migrationBuilder.DropTable(
                name: "ReceiptOrIssues");

            migrationBuilder.DropTable(
                name: "WarehouseTransfers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryConversions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProducedProductId = table.Column<int>(type: "int", nullable: false),
                    ConversionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryConversions_Products_ProducedProductId",
                        column: x => x.ProducedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptOrIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptOrIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssues_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseInventories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseInventories_StorageSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseInventories_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DestinationSectionId = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: false),
                    DestinationZoneId = table.Column<int>(type: "int", nullable: true),
                    SourceSectionId = table.Column<int>(type: "int", nullable: true),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: false),
                    SourceZoneId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseTransfers_StorageSections_DestinationSectionId",
                        column: x => x.DestinationSectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseTransfers_StorageSections_SourceSectionId",
                        column: x => x.SourceSectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseTransfers_StorageZones_DestinationZoneId",
                        column: x => x.DestinationZoneId,
                        principalTable: "StorageZones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseTransfers_StorageZones_SourceZoneId",
                        column: x => x.SourceZoneId,
                        principalTable: "StorageZones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WarehouseTransfers_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseTransfers_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryConversionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryConversionId = table.Column<int>(type: "int", nullable: false),
                    RawProductId = table.Column<int>(type: "int", nullable: false),
                    RawQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryConversionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryConversionDetails_InventoryConversions_InventoryConversionId",
                        column: x => x.InventoryConversionId,
                        principalTable: "InventoryConversions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryConversionDetails_Products_RawProductId",
                        column: x => x.RawProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptOrIssueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    DestinationSectionId = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: true),
                    DestinationZoneId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReceiptOrIssueId = table.Column<int>(type: "int", nullable: false),
                    SourceSectionId = table.Column<int>(type: "int", nullable: true),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: true),
                    SourceZoneId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptOrIssueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageSections_DestinationSectionId",
                        column: x => x.DestinationSectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageSections_SourceSectionId",
                        column: x => x.SourceSectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageZones_DestinationZoneId",
                        column: x => x.DestinationZoneId,
                        principalTable: "StorageZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_StorageZones_SourceZoneId",
                        column: x => x.SourceZoneId,
                        principalTable: "StorageZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItems_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    WarehouseInventoryId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityAdjustment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_StorageSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "StorageSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryAdjustments_WarehouseInventories_WarehouseInventoryId",
                        column: x => x.WarehouseInventoryId,
                        principalTable: "WarehouseInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTransferItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseTransferId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTransferItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseTransferItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseTransferItems_WarehouseTransfers_WarehouseTransferId",
                        column: x => x.WarehouseTransferId,
                        principalTable: "WarehouseTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_SectionId",
                table: "InventoryAdjustments",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAdjustments_WarehouseInventoryId",
                table: "InventoryAdjustments",
                column: "WarehouseInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryConversionDetails_InventoryConversionId",
                table: "InventoryConversionDetails",
                column: "InventoryConversionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryConversionDetails_RawProductId",
                table: "InventoryConversionDetails",
                column: "RawProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryConversions_ProducedProductId",
                table: "InventoryConversions",
                column: "ProducedProductId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssues_WarehouseId",
                table: "ReceiptOrIssues",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_ProductId",
                table: "WarehouseInventories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_SectionId",
                table: "WarehouseInventories",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseInventories_WarehouseId",
                table: "WarehouseInventories",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransferItems_ProductId",
                table: "WarehouseTransferItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransferItems_WarehouseTransferId",
                table: "WarehouseTransferItems",
                column: "WarehouseTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransfers_DestinationSectionId",
                table: "WarehouseTransfers",
                column: "DestinationSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransfers_DestinationWarehouseId",
                table: "WarehouseTransfers",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransfers_DestinationZoneId",
                table: "WarehouseTransfers",
                column: "DestinationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransfers_SourceSectionId",
                table: "WarehouseTransfers",
                column: "SourceSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransfers_SourceWarehouseId",
                table: "WarehouseTransfers",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransfers_SourceZoneId",
                table: "WarehouseTransfers",
                column: "SourceZoneId");
        }
    }
}
