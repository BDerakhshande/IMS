using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class lastmigrationinwarhouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductItem_Products_ProductId",
                table: "ProductItem");


            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductItem",
                table: "ProductItem");

            migrationBuilder.RenameTable(
                name: "ProductItem",
                newName: "ProductItems");

            migrationBuilder.RenameIndex(
                name: "IX_ProductItem_ProjectId",
                table: "ProductItems",
                newName: "IX_ProductItems_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductItem_ProductId",
                table: "ProductItems",
                newName: "IX_ProductItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductItems",
                table: "ProductItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductItems_Products_ProductId",
                table: "ProductItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductItems_Products_ProductId",
                table: "ProductItems");

          

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductItems",
                table: "ProductItems");

            migrationBuilder.RenameTable(
                name: "ProductItems",
                newName: "ProductItem");

            migrationBuilder.RenameIndex(
                name: "IX_ProductItems_ProjectId",
                table: "ProductItem",
                newName: "IX_ProductItem_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductItems_ProductId",
                table: "ProductItem",
                newName: "IX_ProductItem_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductItem",
                table: "ProductItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductItem_Products_ProductId",
                table: "ProductItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

          
        }
    }
}
