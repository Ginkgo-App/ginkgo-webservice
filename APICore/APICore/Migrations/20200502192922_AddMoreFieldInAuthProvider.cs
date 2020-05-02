using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class AddMoreFieldInAuthProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "AuthProviders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AuthProviders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AuthProviders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "AuthProviders");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AuthProviders");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AuthProviders");
        }
    }
}
