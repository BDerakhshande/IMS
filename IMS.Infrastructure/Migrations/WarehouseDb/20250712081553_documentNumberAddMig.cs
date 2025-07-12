using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class documentNumberAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "conversionDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "conversionDocuments");
        }
    }
}
