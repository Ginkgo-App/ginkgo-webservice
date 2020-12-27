using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class UpdateLastSeenMessageinUsergroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastSeenMessageId",
                table: "UserGroup",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenMessageId",
                table: "UserGroup");
        }
    }
}
