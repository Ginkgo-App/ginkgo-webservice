using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class Addplacetype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Places",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Places");
        }
    }
}
