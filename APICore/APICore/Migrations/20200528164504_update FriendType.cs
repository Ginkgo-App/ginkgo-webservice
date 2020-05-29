using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class updateFriendType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHost",
                table: "TourMembers");

            migrationBuilder.AddColumn<int>(
                name: "CreateBy",
                table: "Tours",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Tours",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Tours");

            migrationBuilder.AddColumn<bool>(
                name: "IsHost",
                table: "TourMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
