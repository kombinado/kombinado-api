using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kombinado.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTotalSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleTotalSeats",
                table: "Users",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleTotalSeats",
                table: "Users");
        }
    }
}
