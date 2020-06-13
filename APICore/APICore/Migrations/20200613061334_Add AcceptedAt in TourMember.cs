using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class AddAcceptedAtinTourMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteAt",
                table: "TourInfos");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "TourMembers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TotalComment",
                table: "Posts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalLike",
                table: "Posts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "TourMembers");

            migrationBuilder.DropColumn(
                name: "TotalComment",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TotalLike",
                table: "Posts");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteAt",
                table: "TourInfos",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
