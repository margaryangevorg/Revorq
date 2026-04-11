using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revorq.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleIdsToMatchEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""AspNetUserRoles"" DROP CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId"";
                ALTER TABLE ""AspNetRoleClaims"" DROP CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId"";

                UPDATE ""AspNetUserRoles"" SET ""RoleId"" = ""RoleId"" - 1;
                UPDATE ""AspNetRoleClaims"" SET ""RoleId"" = ""RoleId"" - 1;
                UPDATE ""AspNetRoles"" SET ""Id"" = ""Id"" - 1;

                ALTER TABLE ""AspNetUserRoles"" ADD CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId""
                    FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles""(""Id"") ON DELETE CASCADE;
                ALTER TABLE ""AspNetRoleClaims"" ADD CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId""
                    FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles""(""Id"") ON DELETE CASCADE;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""AspNetUserRoles"" DROP CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId"";
                ALTER TABLE ""AspNetRoleClaims"" DROP CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId"";

                UPDATE ""AspNetUserRoles"" SET ""RoleId"" = ""RoleId"" + 1;
                UPDATE ""AspNetRoleClaims"" SET ""RoleId"" = ""RoleId"" + 1;
                UPDATE ""AspNetRoles"" SET ""Id"" = ""Id"" + 1;

                ALTER TABLE ""AspNetUserRoles"" ADD CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId""
                    FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles""(""Id"") ON DELETE CASCADE;
                ALTER TABLE ""AspNetRoleClaims"" ADD CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId""
                    FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles""(""Id"") ON DELETE CASCADE;
            ");
        }
    }
}
