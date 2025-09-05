using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.ProcurementManagementDb
{
    /// <inheritdoc />
    public partial class AddUnitIdMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "PurchaseRequestItems");


          
         

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "PurchaseRequestItems",
                type: "int",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "PurchaseRequestFlatItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

           

            migrationBuilder.CreateTable(
                name: "Unit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit", x => x.Id);
                });


           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Unit_UnitId",
                table: "Product");

            migrationBuilder.DropTable(
                name: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_Product_UnitId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "PurchaseRequestItems");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "PurchaseRequestFlatItems");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Product");

            migrationBuilder.AddColumn<decimal>(
                name: "Capacity",
                table: "StorageSection",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "PurchaseRequestItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Project",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "Project",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Objectives",
                table: "Project",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
