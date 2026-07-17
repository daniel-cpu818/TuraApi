using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commune",
                table: "Locations",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Hood",
                table: "Locations",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Piso",
                table: "Locations",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commune",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Hood",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Piso",
                table: "Locations");
        }
    }
}
