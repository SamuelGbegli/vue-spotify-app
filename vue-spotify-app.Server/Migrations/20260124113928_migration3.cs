using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class migration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistTrack_Artists_ArtistsID",
                table: "ArtistTrack");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistTrack_Tracks_TracksID",
                table: "ArtistTrack");

            migrationBuilder.RenameColumn(
                name: "TracksID",
                table: "ArtistTrack",
                newName: "TrackID");

            migrationBuilder.RenameColumn(
                name: "ArtistsID",
                table: "ArtistTrack",
                newName: "ArtistID");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistTrack_TracksID",
                table: "ArtistTrack",
                newName: "IX_ArtistTrack_TrackID");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistTrack_Artists_ArtistID",
                table: "ArtistTrack",
                column: "ArtistID",
                principalTable: "Artists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistTrack_Tracks_TrackID",
                table: "ArtistTrack",
                column: "TrackID",
                principalTable: "Tracks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistTrack_Artists_ArtistID",
                table: "ArtistTrack");

            migrationBuilder.DropForeignKey(
                name: "FK_ArtistTrack_Tracks_TrackID",
                table: "ArtistTrack");

            migrationBuilder.RenameColumn(
                name: "TrackID",
                table: "ArtistTrack",
                newName: "TracksID");

            migrationBuilder.RenameColumn(
                name: "ArtistID",
                table: "ArtistTrack",
                newName: "ArtistsID");

            migrationBuilder.RenameIndex(
                name: "IX_ArtistTrack_TrackID",
                table: "ArtistTrack",
                newName: "IX_ArtistTrack_TracksID");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistTrack_Artists_ArtistsID",
                table: "ArtistTrack",
                column: "ArtistsID",
                principalTable: "Artists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistTrack_Tracks_TracksID",
                table: "ArtistTrack",
                column: "TracksID",
                principalTable: "Tracks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
