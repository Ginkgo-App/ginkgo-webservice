using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace APICore.Migrations
{
    public partial class UpdateNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<int>(nullable: false),
                    ReceiverId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    SendAt = table.Column<DateTime>(nullable: true),
                    SeenAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
