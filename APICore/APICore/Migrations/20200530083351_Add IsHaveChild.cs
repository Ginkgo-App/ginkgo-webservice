using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class AddIsHaveChild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHaveChild",
                table: "PlaceTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChildPlaces",
                columns: table => new
                {
                    ParentId = table.Column<int>(nullable: false),
                    ChildId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildPlaces", x => new { x.ParentId, x.ChildId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildPlaces");

            migrationBuilder.DropColumn(
                name: "IsHaveChild",
                table: "PlaceTypes");
        }
    }
}
