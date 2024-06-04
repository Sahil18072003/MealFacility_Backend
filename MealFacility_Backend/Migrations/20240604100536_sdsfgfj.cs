using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealFacility_Backend.Migrations
{
    public partial class sdsfgfj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "notification");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "notification",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "notification",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "notification",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
