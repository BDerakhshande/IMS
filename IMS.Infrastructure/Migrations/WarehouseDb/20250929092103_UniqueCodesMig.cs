using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class UniqueCodesMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptOrIssueItemUniqueCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptOrIssueItemId = table.Column<int>(type: "int", nullable: false),
                    UniqueCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptOrIssueItemUniqueCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptOrIssueItemUniqueCode_ReceiptOrIssueItems_ReceiptOrIssueItemId",
                        column: x => x.ReceiptOrIssueItemId,
                        principalTable: "ReceiptOrIssueItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptOrIssueItemUniqueCode_ReceiptOrIssueItemId",
                table: "ReceiptOrIssueItemUniqueCode",
                column: "ReceiptOrIssueItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptOrIssueItemUniqueCode");
        }
    }
}
