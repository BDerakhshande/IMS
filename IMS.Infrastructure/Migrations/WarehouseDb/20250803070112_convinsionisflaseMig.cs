using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class convinsionisflaseMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // فقط وقتی FK واقعاً وجود دارد، حذفش کن
            // migrationBuilder.DropForeignKey(...); // این خط را حذف کن یا با شرط بررسی وجود جایگزین کن
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // اگر Down نیاز به افزودن FK دارد، باید مطمئن باشی FK صحیح تعریف می‌کنی
            // migrationBuilder.AddForeignKey(...);
        }

    }
}
