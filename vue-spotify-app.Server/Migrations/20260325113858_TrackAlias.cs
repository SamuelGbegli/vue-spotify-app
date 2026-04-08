using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class TrackAliasOld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AliasID",
                table: "Tracks",
                type: "uniqueidentifier",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SortName",
                table: "Playlists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TrackAliases",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackAliases", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AliasID",
                table: "Tracks",
                column: "AliasID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tracks_TrackAliases_AliasID",
                table: "Tracks",
                column: "AliasID",
                principalTable: "TrackAliases",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tracks_TrackAliases_AliasID",
                table: "Tracks");

            migrationBuilder.DropTable(
                name: "TrackAliases");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_AliasID",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "AliasID",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "SortName",
                table: "Playlists");
        }
    }
}
