using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceOrderHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceOrderHistories",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Assignments = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceOrderHistories", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_MaintenanceOrderHistories_MaintenanceOrders_OrderId",
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
                name: "MaintenanceOrderHistories");
        }
    }
}
