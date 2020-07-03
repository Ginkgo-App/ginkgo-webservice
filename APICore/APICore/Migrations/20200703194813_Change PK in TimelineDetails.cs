using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace APICore.Migrations
{
    public partial class ChangePKinTimelineDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TimelineDetails",
                table: "TimelineDetails");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TimelineDetails",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimelineDetails",
                table: "TimelineDetails",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TimelineDetails",
                table: "TimelineDetails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TimelineDetails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimelineDetails",
                table: "TimelineDetails",
                columns: new[] { "PlaceId", "TimelineId" });
        }
    }
}
