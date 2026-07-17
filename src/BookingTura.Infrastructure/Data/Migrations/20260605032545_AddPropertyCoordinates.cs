using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Properties",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Commune",
                table: "Properties",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Hood",
                table: "Properties",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Properties",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Properties",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Commune",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Hood",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Properties");
        }
    }
}
