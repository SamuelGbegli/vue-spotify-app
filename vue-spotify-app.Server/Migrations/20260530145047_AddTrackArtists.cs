using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackArtists",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ArtistID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackArtists", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrackArtists_Artists_ArtistID",
                        column: x => x.ArtistID,
                        principalTable: "Artists",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TrackArtists_Tracks_TrackID",
                        column: x => x.TrackID,
                        principalTable: "Tracks",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtists_ArtistID",
                table: "TrackArtists",
                column: "ArtistID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtists_TrackID_ArtistID",
                table: "TrackArtists",
                columns: new[] { "TrackID", "ArtistID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackArtists");
        }
    }
}
