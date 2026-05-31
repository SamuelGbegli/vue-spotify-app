using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeScreenshotID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScreenshotID",
                table: "Playlists",
                newName: "SnapshotID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SnapshotID",
                table: "Playlists",
                newName: "ScreenshotID");
        }
    }
}
