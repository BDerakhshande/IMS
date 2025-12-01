using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class ishalakeinmighastok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Employer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNumber = table.Column<long>(type: "bigint", nullable: false),
                    LegalPersonType = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepresentativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepresentativePosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepresentativeMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepresentativeEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CooperationType = table.Column<int>(type: "int", nullable: true),
                    CooperationStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdditionalDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProjectManager = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgressPercent = table.Column<double>(type: "float", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Employer_EmployerId",
                        column: x => x.EmployerId,
                        principalTable: "Employer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_ProjectType_ProjectTypeId",
                        column: x => x.ProjectTypeId,
                        principalTable: "ProjectType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

         
         
         
            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_CategoryId",
                table: "conversionConsumedItems",
                column: "CategoryId");

           

            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_GroupId",
                table: "conversionConsumedItems",
                column: "GroupId");


           
            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_WarehouseId",
                table: "conversionConsumedItems",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_ZoneId",
                table: "conversionConsumedItems",
                column: "ZoneId");

         


          

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_CategoryId",
                table: "conversionProducedItems",
                column: "CategoryId");

          

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_GroupId",
                table: "conversionProducedItems",
                column: "GroupId");

          


            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_SectionId",
                table: "conversionProducedItems",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_StatusId",
                table: "conversionProducedItems",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_WarehouseId",
                table: "conversionProducedItems",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_ZoneId",
                table: "conversionProducedItems",
                column: "ZoneId");


          
          

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EmployerId",
                schema: "dbo",
                table: "Projects",
                column: "EmployerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectTypeId",
                schema: "dbo",
                table: "Projects",
                column: "ProjectTypeId");

        


       

           
         
         
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionConsumedItemUniqueCodes");

            migrationBuilder.DropTable(
                name: "ConversionProducedItemUniqueCodes");

            migrationBuilder.DropTable(
                name: "InventoryReceiptLogs");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "ProductItems");

            migrationBuilder.DropTable(
                name: "ReceiptOrIssueItemUniqueCode");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "conversionConsumedItems");

            migrationBuilder.DropTable(
                name: "conversionProducedItems");

            migrationBuilder.DropTable(
                name: "ReceiptOrIssueItems");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "conversionDocuments");

            migrationBuilder.DropTable(
                name: "ReceiptOrIssues");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "StorageSections");

            migrationBuilder.DropTable(
                name: "Employer");

            migrationBuilder.DropTable(
                name: "ProjectType");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "StorageZones");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
