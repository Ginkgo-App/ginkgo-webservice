using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class AddTotalRatingfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalRating",
                table: "TourInfos",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRating",
                table: "TourInfos");
        }
    }
}
