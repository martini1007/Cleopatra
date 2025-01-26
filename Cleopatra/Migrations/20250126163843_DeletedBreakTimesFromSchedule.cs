using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cleopatra.Migrations
{
    /// <inheritdoc />
    public partial class DeletedBreakTimesFromSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Services");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Services",
                type: "INTEGER",
                nullable: true);
        }
    }
}
