using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class StorageSectioncombitionMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageSections_SectionCode",
                table: "StorageSections");

            migrationBuilder.DropIndex(
                name: "IX_StorageSections_ZoneId",
                table: "StorageSections");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSections_ZoneId_SectionCode",
                table: "StorageSections",
                columns: new[] { "ZoneId", "SectionCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StorageSections_ZoneId_SectionCode",
                table: "StorageSections");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSections_SectionCode",
                table: "StorageSections",
                column: "SectionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageSections_ZoneId",
                table: "StorageSections",
                column: "ZoneId");
        }
    }
}
