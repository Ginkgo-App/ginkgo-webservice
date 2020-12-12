using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class UpdateChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "UserGroup");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserGroup",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Groups",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Groups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "Groups",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserGroup");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "UserGroup",
                type: "text",
                nullable: true);
        }
    }
}
