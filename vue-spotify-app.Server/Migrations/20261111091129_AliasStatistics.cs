using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class AliasStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackAliasPlaybackStatistics",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackAliasID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalRecordedPlays = table.Column<long>(type: "bigint", nullable: false),
                    LastRecordedPlayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackAliasPlaybackStatistics", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrackAliasPlaybackStatistics_TrackAliases_TrackAliasID",
                        column: x => x.TrackAliasID,
                        principalTable: "TrackAliases",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackAliasPlaybackStatistics_TrackAliasID",
                table: "TrackAliasPlaybackStatistics",
                column: "TrackAliasID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackAliasPlaybackStatistics");
        }
    }
}
