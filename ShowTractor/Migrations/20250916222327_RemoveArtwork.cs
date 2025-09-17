using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShowTractor.Migrations
{
    /// <inheritdoc />
    public partial class RemoveArtwork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Artwork",
                table: "TvSeasons");

            migrationBuilder.DropColumn(
                name: "Artwork",
                table: "TvEpisodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Artwork",
                table: "TvSeasons",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Artwork",
                table: "TvEpisodes",
                type: "BLOB",
                nullable: true);
        }
    }
}
