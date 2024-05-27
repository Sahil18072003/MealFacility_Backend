using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealFacility_Backend.Migrations
{
    public partial class secondds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedOTP",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedOTP",
                table: "users");
        }
    }
}
