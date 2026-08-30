using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameMaintenanceReportShortDescriptionToNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "MaintenanceReports",
                newName: "Notes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "MaintenanceReports",
                newName: "ShortDescription");
        }
    }
}
