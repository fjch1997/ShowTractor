using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShowTractor.Migrations
{
    /// <inheritdoc />
    public partial class Vacuum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("VACUUM;", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
