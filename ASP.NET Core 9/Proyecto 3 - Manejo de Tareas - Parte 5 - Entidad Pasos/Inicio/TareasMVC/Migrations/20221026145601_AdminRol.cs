using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS(Select Id from AspNetRoles where Id = '89eead86-8314-48ce-908b-742d68e27356')
BEGIN
	INSERT AspNetRoles (Id, [Name], [NormalizedName])
	VALUES ('89eead86-8314-48ce-908b-742d68e27356', 'admin', 'ADMIN')
END

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE AspNetRoles Where Id = '89eead86-8314-48ce-908b-742d68e27356'");
        }
    }
}
