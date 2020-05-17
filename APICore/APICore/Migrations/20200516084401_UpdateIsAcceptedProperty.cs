using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class UpdateIsAcceptedProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFriend",
                table: "Friends");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Friends",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Friends");

            migrationBuilder.AddColumn<bool>(
                name: "IsFriend",
                table: "Friends",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
