using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class InventoryTransactionsAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

           



            migrationBuilder.EnsureSchema(
                name: "dbo");

           

         

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    QuantityChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                });

        

         
          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversionConsumedItems_Projects_ProjectId",
                table: "conversionConsumedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_conversionProducedItems_Projects_ProjectId",
                table: "conversionProducedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductItems_Projects_ProjectId",
                table: "ProductItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Employer_EmployerId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectType_ProjectTypeId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "Projects",
                schema: "dbo",
                newName: "Project");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ProjectTypeId",
                table: "Project",
                newName: "IX_Project_ProjectTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_EmployerId",
                table: "Project",
                newName: "IX_Project_EmployerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_conversionConsumedItems_Project_ProjectId",
                table: "conversionConsumedItems",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_conversionProducedItems_Project_ProjectId",
                table: "conversionProducedItems",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductItems_Project_ProjectId",
                table: "ProductItems",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Employer_EmployerId",
                table: "Project",
                column: "EmployerId",
                principalTable: "Employer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_ProjectType_ProjectTypeId",
                table: "Project",
                column: "ProjectTypeId",
                principalTable: "ProjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
