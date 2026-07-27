using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventReservationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Events",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartsAt",
                table: "Events",
                column: "StartsAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_Name",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StartsAt",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
