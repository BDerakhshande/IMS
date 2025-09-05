using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    public partial class FixUnitRelationClean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // حذف ستون اضافی اگر وجود دارد
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = N'UnitId1' AND Object_ID = Object_ID(N'Products')
                )
                BEGIN
                    ALTER TABLE Products DROP COLUMN UnitId1;
                END
            ");

            // حذف FK اضافی با نام قدیمی اگر وجود دارد
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = N'FK_Products_Units_UnitId1'
                )
                BEGIN
                    ALTER TABLE Products DROP CONSTRAINT FK_Products_Units_UnitId1;
                END
            ");

            // حذف FK صحیح قبلی اگر از قبل وجود دارد
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = N'FK_Products_Units_UnitId'
                )
                BEGIN
                    ALTER TABLE Products DROP CONSTRAINT FK_Products_Units_UnitId;
                END
            ");

            // حذف Index قبلی روی UnitId اگر وجود دارد
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Products_UnitId' AND object_id = OBJECT_ID('Products')
                )
                BEGIN
                    DROP INDEX IX_Products_UnitId ON Products;
                END
            ");

            // ایجاد Index صحیح روی UnitId
            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId",
                table: "Products",
                column: "UnitId"
            );

            // ایجاد Foreign Key صحیح
            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products"
            );

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitId",
                table: "Products"
            );
        }
    }
}
