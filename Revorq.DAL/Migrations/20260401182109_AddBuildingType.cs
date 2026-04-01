using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuildingType",
                table: "Buildings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingType",
                table: "Buildings");
        }
    }
}
