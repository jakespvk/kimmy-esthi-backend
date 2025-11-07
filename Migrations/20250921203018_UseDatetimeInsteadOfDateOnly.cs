using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class UseDatetimeInsteadOfDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Appointments",
                newName: "DateTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Appointments",
                newName: "Time");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}
