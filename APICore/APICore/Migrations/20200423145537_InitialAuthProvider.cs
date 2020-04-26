using Microsoft.EntityFrameworkCore.Migrations;

namespace APICore.Migrations
{
    public partial class InitialAuthProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthProviders",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    provider = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthProviders", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthProviders");
        }
    }
}
