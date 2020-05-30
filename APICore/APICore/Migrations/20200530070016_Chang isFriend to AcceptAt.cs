using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class ChangisFriendtoAcceptAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Friends");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Friends",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Friends");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Friends",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
