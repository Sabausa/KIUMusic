using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Back.Migrations
{
    /// <inheritdoc />
    public partial class newdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstrumentName",
                table: "Reservations");

            migrationBuilder.AddColumn<bool>(
                name: "IsBassTaken",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDrumsTaken",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuitarTaken",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMicrophoneTaken",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPianoTaken",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBassTaken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsDrumsTaken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsGuitarTaken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsMicrophoneTaken",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsPianoTaken",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "InstrumentName",
                table: "Reservations",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
