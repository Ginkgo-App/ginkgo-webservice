using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class RenameTourId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TourInfoId",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "TourId",
                table: "Groups",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TourId",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "TourInfoId",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
