using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddISRCDropPopularity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateSaved",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Tracks");

            migrationBuilder.AddColumn<string>(
                name: "ISRC",
                table: "Tracks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISRC",
                table: "Tracks");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSaved",
                table: "Tracks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Popularity",
                table: "Tracks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
