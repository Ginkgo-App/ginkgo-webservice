using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class Updatetourtotaldaynight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimelinePlaces");

            migrationBuilder.AddColumn<string[]>(
                name: "Services",
                table: "Tours",
                nullable: false,
                defaultValue: new string[0] {  });

            migrationBuilder.AddColumn<int>(
                name: "TotalDay",
                table: "Tours",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalNight",
                table: "Tours",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TimelineDetails",
                columns: table => new
                {
                    PlaceId = table.Column<int>(nullable: false),
                    TimelineId = table.Column<int>(nullable: false),
                    Time = table.Column<string>(nullable: true),
                    Detail = table.Column<string>(nullable: true),
                    DeletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineDetails", x => new { x.PlaceId, x.TimelineId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimelineDetails");

            migrationBuilder.DropColumn(
                name: "Services",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "TotalDay",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "TotalNight",
                table: "Tours");

            migrationBuilder.CreateTable(
                name: "TimelinePlaces",
                columns: table => new
                {
                    PlaceId = table.Column<int>(type: "integer", nullable: false),
                    TimelineId = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelinePlaces", x => new { x.PlaceId, x.TimelineId });
                });
        }
    }
}
