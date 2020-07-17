using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class Changepricetofloat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Price",
                table: "Tours",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Tours",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float));
        }
    }
}
