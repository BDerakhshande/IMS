using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class convisionitemMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "conversionProducedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "conversionConsumedItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversionProducedItems_ProjectId",
                table: "conversionProducedItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_conversionConsumedItems_ProjectId",
                table: "conversionConsumedItems",
                column: "ProjectId");

           
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversionConsumedItems_Project_ProjectId",
                table: "conversionConsumedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_conversionProducedItems_Project_ProjectId",
                table: "conversionProducedItems");

            migrationBuilder.DropIndex(
                name: "IX_conversionProducedItems_ProjectId",
                table: "conversionProducedItems");

            migrationBuilder.DropIndex(
                name: "IX_conversionConsumedItems_ProjectId",
                table: "conversionConsumedItems");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "conversionProducedItems");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "conversionConsumedItems");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "conversionDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversionDocuments_ProjectId",
                table: "conversionDocuments",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_conversionDocuments_Project_ProjectId",
                table: "conversionDocuments",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
