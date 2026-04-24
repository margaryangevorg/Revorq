using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddReporterToMaintenanceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReporterId",
                table: "MaintenanceOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""MaintenanceOrders"" o
                SET ""ReporterId"" = (
                    SELECT u.""Id""
                    FROM ""AspNetUsers"" u
                    JOIN ""AspNetUserRoles"" ur ON ur.""UserId"" = u.""Id""
                    JOIN ""AspNetRoles"" r ON r.""Id"" = ur.""RoleId""
                    WHERE r.""Name"" = 'Admin'
                      AND u.""CompanyId"" = (
                          SELECT b.""CompanyId""
                          FROM ""Elevators"" e
                          JOIN ""Buildings"" b ON b.""Id"" = e.""BuildingId""
                          WHERE e.""Id"" = o.""ElevatorId""
                      )
                    LIMIT 1
                )
                WHERE o.""ReporterId"" = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_ReporterId",
                table: "MaintenanceOrders",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceOrders_AspNetUsers_ReporterId",
                table: "MaintenanceOrders",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceOrders_AspNetUsers_ReporterId",
                table: "MaintenanceOrders");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceOrders_ReporterId",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "MaintenanceOrders");
        }
    }
}
