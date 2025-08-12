using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS.Infrastructure.Migrations.ProcurementManagementDb
{
    /// <inheritdoc />
    public partial class RequestTypeAddMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // اضافه کردن ستون RequestTypeId به جدول PurchaseRequests
            migrationBuilder.AddColumn<int>(
      name: "RequestTypeId",
      table: "PurchaseRequests",
      type: "int",
      nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_RequestTypeId",
                table: "PurchaseRequests",
                column: "RequestTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_RequestTypes_RequestTypeId",
                table: "PurchaseRequests",
                column: "RequestTypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_RequestTypes_RequestTypeId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_RequestTypeId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                table: "PurchaseRequests");
        }

    }
}
