using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumArtist_Albums_AlbumsID",
                table: "AlbumArtist");

            migrationBuilder.DropForeignKey(
                name: "FK_AlbumArtist_Artists_ArtistsID",
                table: "AlbumArtist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlbumArtist",
                table: "AlbumArtist");

            migrationBuilder.RenameColumn(
                name: "ArtistsID",
                table: "AlbumArtist",
                newName: "ArtistID");

            migrationBuilder.RenameColumn(
                name: "AlbumsID",
                table: "AlbumArtist",
                newName: "AlbumID");

            migrationBuilder.RenameIndex(
                name: "IX_AlbumArtist_ArtistsID",
                table: "AlbumArtist",
                newName: "IX_AlbumArtist_ArtistID");


            migrationBuilder.AddPrimaryKey(
                name: "PK_AlbumArtist",
                table: "AlbumArtist",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_AlbumID_ArtistID",
                table: "AlbumArtist",
                columns: new[] { "AlbumID", "ArtistID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumArtist_Albums_AlbumID",
                table: "AlbumArtist",
                column: "AlbumID",
                principalTable: "Albums",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumArtist_Artists_ArtistID",
                table: "AlbumArtist",
                column: "ArtistID",
                principalTable: "Artists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlbumArtist_Albums_AlbumID",
                table: "AlbumArtist");

            migrationBuilder.DropForeignKey(
                name: "FK_AlbumArtist_Artists_ArtistID",
                table: "AlbumArtist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlbumArtist",
                table: "AlbumArtist");

            migrationBuilder.DropIndex(
                name: "IX_AlbumArtist_AlbumID_ArtistID",
                table: "AlbumArtist");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "AlbumArtist");

            migrationBuilder.RenameColumn(
                name: "ArtistID",
                table: "AlbumArtist",
                newName: "ArtistsID");

            migrationBuilder.RenameColumn(
                name: "AlbumID",
                table: "AlbumArtist",
                newName: "AlbumsID");

            migrationBuilder.RenameIndex(
                name: "IX_AlbumArtist_ArtistID",
                table: "AlbumArtist",
                newName: "IX_AlbumArtist_ArtistsID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlbumArtist",
                table: "AlbumArtist",
                columns: new[] { "AlbumsID", "ArtistsID" });

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumArtist_Albums_AlbumsID",
                table: "AlbumArtist",
                column: "AlbumsID",
                principalTable: "Albums",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlbumArtist_Artists_ArtistsID",
                table: "AlbumArtist",
                column: "ArtistsID",
                principalTable: "Artists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
