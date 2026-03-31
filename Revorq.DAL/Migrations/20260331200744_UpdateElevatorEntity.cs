using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateElevatorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Elevators");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Elevators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Elevators",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CustomerFullName",
                table: "Elevators",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhoneNumber",
                table: "Elevators",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Elevators",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumberInProject",
                table: "Elevators",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductionCountry",
                table: "Elevators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyDate",
                table: "Elevators",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyType",
                table: "Elevators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Elevators_SerialNumber",
                table: "Elevators",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Elevators_SerialNumber",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "CustomerFullName",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "CustomerPhoneNumber",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "NumberInProject",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "ProductionCountry",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "WarrantyDate",
                table: "Elevators");

            migrationBuilder.DropColumn(
                name: "WarrantyType",
                table: "Elevators");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Elevators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Elevators",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
