using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StoreImageAsBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "MaintenanceReports");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "MaintenanceReports",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "MaintenanceReports");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "MaintenanceReports",
                type: "text",
                nullable: true);
        }
    }
}
