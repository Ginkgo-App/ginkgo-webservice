using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class AddratingforPost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Posts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TourId",
                table: "Posts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TourId",
                table: "Posts");
        }
    }
}
