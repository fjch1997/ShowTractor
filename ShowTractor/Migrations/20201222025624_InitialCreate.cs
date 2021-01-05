using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShowTractor.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TvSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShowName = table.Column<string>(type: "TEXT", nullable: false),
                    Season = table.Column<int>(type: "INTEGER", nullable: false),
                    GenresCsv = table.Column<string>(type: "TEXT", nullable: false),
                    RatingsCsv = table.Column<string>(type: "TEXT", nullable: false),
                    ShowDescription = table.Column<string>(type: "TEXT", nullable: false),
                    SeasonDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Artwork = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Following = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowEnded = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowFinale = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalAttributes",
                columns: table => new
                {
                    TvSeasonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssemblyName = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalAttributes", x => new { x.TvSeasonId, x.AssemblyName, x.Name });
                    table.ForeignKey(
                        name: "FK_AdditionalAttributes_TvSeasons_TvSeasonId",
                        column: x => x.TvSeasonId,
                        principalTable: "TvSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TvEpisodes",
                columns: table => new
                {
                    TvSeasonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Artwork = table.Column<byte[]>(type: "BLOB", nullable: true),
                    FirstAirDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Runtime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WatchProgress = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvEpisodes", x => new { x.TvSeasonId, x.EpisodeNumber });
                    table.ForeignKey(
                        name: "FK_TvEpisodes_TvSeasons_TvSeasonId",
                        column: x => x.TvSeasonId,
                        principalTable: "TvSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalAttributes");

            migrationBuilder.DropTable(
                name: "TvEpisodes");

            migrationBuilder.DropTable(
                name: "TvSeasons");
        }
    }
}
