using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPartChangeToMaintenanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPartChange",
                table: "MaintenanceReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPartChange",
                table: "MaintenanceReports");
        }
    }
}
