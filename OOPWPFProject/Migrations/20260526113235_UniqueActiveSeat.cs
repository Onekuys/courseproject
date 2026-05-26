using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OOPWPFProject.Migrations
{
    /// <inheritdoc />
    public partial class UniqueActiveSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_ShowtimeId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ShowtimeId_SeatNumber",
                table: "Reservations",
                columns: new[] { "ShowtimeId", "SeatNumber" },
                unique: true,
                filter: "\"IsCanceled\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_ShowtimeId_SeatNumber",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ShowtimeId",
                table: "Reservations",
                column: "ShowtimeId");
        }
    }
}
