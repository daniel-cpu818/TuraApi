using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowLegacyLocationColumnsToBeOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @sql = (
                    SELECT GROUP_CONCAT(CONCAT('DROP COLUMN `', COLUMN_NAME, '`'))
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'Locations'
                    AND COLUMN_NAME IN ('Country','State','City','Latitude','Longitude')
                );

                SET @sql = IF(@sql IS NOT NULL, CONCAT('ALTER TABLE `Locations` ', @sql), 'SELECT 1');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE `Locations`
                ADD COLUMN `Country` longtext NULL,
                ADD COLUMN `State` longtext NULL,
                ADD COLUMN `City` longtext NULL,
                ADD COLUMN `Latitude` double NULL,
                ADD COLUMN `Longitude` double NULL;
            """);
        }
    }
}
