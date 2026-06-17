using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCare.Infreastructure.Migrations
{
    /// <inheritdoc />
    public partial class editMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lat",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "lng",
                table: "Measurements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "lat",
                table: "Measurements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "lng",
                table: "Measurements",
                type: "float",
                nullable: true);
        }
    }
}
