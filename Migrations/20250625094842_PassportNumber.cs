using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelFinalProject.Migrations
{
    /// <inheritdoc />
    public partial class PassportNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_passportNumbers_BookingTravellers_BookingTravellerId",
                table: "passportNumbers");

            migrationBuilder.AlterColumn<int>(
                name: "BookingTravellerId",
                table: "passportNumbers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "BookingTravellers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_passportNumbers_BookingTravellers_BookingTravellerId",
                table: "passportNumbers",
                column: "BookingTravellerId",
                principalTable: "BookingTravellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_passportNumbers_BookingTravellers_BookingTravellerId",
                table: "passportNumbers");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "BookingTravellers");

            migrationBuilder.AlterColumn<int>(
                name: "BookingTravellerId",
                table: "passportNumbers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_passportNumbers_BookingTravellers_BookingTravellerId",
                table: "passportNumbers",
                column: "BookingTravellerId",
                principalTable: "BookingTravellers",
                principalColumn: "Id");
        }
    }
}
