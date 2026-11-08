using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedRecordIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExtendedRecords_Timestamp_ID",
                table: "ExtendedRecords",
                columns: new[] { "Timestamp", "ID" },
                descending: new[] { true, false })
                .Annotation("SqlServer:Include", new[] { "TrackURI", "AmountPlayed", "ReasonStart", "ReasonEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExtendedRecords_Timestamp_ID",
                table: "ExtendedRecords");
        }
    }
}
