using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.ProcurementManagementDb
{
    /// <inheritdoc />
    public partial class projectdeleteMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "GoodsRequests");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "GoodsRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "GoodsRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "GoodsRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
