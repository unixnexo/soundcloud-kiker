using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace soundcloud_kiker.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistTracks",
                table: "PlaylistTracks");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PlaylistTracks");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "PlaylistTracks");

            migrationBuilder.AlterColumn<string>(
                name: "SoundCloudId",
                table: "PlaylistTracks",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "PlaylistTracks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistTracks",
                table: "PlaylistTracks",
                column: "SoundCloudId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistTracks",
                table: "PlaylistTracks");

            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "PlaylistTracks");

            migrationBuilder.AlterColumn<string>(
                name: "SoundCloudId",
                table: "PlaylistTracks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PlaylistTracks",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "PlaylistTracks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistTracks",
                table: "PlaylistTracks",
                column: "Id");
        }
    }
}
