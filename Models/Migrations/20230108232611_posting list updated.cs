using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    public partial class postinglistupdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndexId",
                table: "postingList");

            migrationBuilder.DropColumn(
                name: "LinkId",
                table: "postingList");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndexId",
                table: "postingList",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LinkId",
                table: "postingList",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
