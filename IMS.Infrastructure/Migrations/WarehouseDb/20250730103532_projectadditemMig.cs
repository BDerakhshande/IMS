using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class projectadditemMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // اگر کلید خارجی وجود ندارد، این دو خط حذف یا کامنت شود
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ReceiptOrIssues_Project_ProjectId",
            //     table: "ReceiptOrIssues");

            // migrationBuilder.DropIndex(
            //     name: "IX_ReceiptOrIssues_ProjectId",
            //     table: "ReceiptOrIssues");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ReceiptOrIssues");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ReceiptOrIssueItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectTitle",
                table: "ReceiptOrIssueItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ReceiptOrIssueItems");

            migrationBuilder.DropColumn(
                name: "ProjectTitle",
                table: "ReceiptOrIssueItems");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ReceiptOrIssues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssues_ProjectId",
                table: "ReceiptOrIssues",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptOrIssues_Project_ProjectId",
                table: "ReceiptOrIssues",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
