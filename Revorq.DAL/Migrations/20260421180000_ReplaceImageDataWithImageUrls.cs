using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceImageDataWithImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "MaintenanceReports");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "MaintenanceReports",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "MaintenanceReports");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "MaintenanceReports",
                type: "bytea",
                nullable: true);
        }
    }
}
