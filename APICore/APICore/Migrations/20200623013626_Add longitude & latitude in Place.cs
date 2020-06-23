using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class AddlongitudelatitudeinPlace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Places",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Places",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Places");
        }
    }
}
