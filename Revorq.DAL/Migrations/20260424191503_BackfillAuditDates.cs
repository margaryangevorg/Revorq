using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BackfillAuditDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MaintenanceOrders""
                SET ""CreatedDate"" = NOW(), ""UpdatedDate"" = NOW()
                WHERE ""CreatedDate"" = TIMESTAMP '-infinity';

                UPDATE ""MaintenanceReports""
                SET ""CreatedDate"" = NOW(), ""UpdatedDate"" = NOW()
                WHERE ""CreatedDate"" = TIMESTAMP '-infinity';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
