using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MoveImagePathToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "MaintenanceOrders");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "MaintenanceReports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "MaintenanceReports");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "MaintenanceOrders",
                type: "text",
                nullable: true);
        }
    }
}
