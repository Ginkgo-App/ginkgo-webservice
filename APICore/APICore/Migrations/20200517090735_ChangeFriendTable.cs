using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class ChangeFriendTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friends",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "UserOtherId",
                table: "Friends");

            migrationBuilder.AddColumn<int>(
                name: "RequestedUserId",
                table: "Friends",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friends",
                table: "Friends",
                columns: new[] { "UserId", "RequestedUserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Friends",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "RequestedUserId",
                table: "Friends");

            migrationBuilder.AddColumn<int>(
                name: "UserOtherId",
                table: "Friends",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friends",
                table: "Friends",
                columns: new[] { "UserId", "UserOtherId" });
        }
    }
}
