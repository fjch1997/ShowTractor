using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowTractor.Migrations
{
    public partial class DateFollowed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFollowed",
                table: "TvSeasons",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFollowed",
                table: "TvSeasons");
        }
    }
}
