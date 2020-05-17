using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class UpdateIsFriendProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFriend",
                table: "Friends",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFriend",
                table: "Friends");
        }
    }
}
