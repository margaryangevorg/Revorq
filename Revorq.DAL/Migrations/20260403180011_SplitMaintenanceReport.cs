using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SplitMaintenanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustmentDone",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "CleaningDone",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "IssueDetected",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "VisualCheckDone",
                table: "MaintenanceOrders");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MaintenanceOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MaintenanceReports",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    JobStartedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IssueDetected = table.Column<bool>(type: "boolean", nullable: false),
                    VisualCheckDone = table.Column<bool>(type: "boolean", nullable: false),
                    AdjustmentDone = table.Column<bool>(type: "boolean", nullable: false),
                    CleaningDone = table.Column<bool>(type: "boolean", nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceReports", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_MaintenanceReports_MaintenanceOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "MaintenanceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceReports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MaintenanceOrders");

            migrationBuilder.AddColumn<bool>(
                name: "AdjustmentDone",
                table: "MaintenanceOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CleaningDone",
                table: "MaintenanceOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "MaintenanceOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "MaintenanceOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IssueDetected",
                table: "MaintenanceOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VisualCheckDone",
                table: "MaintenanceOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
