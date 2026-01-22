using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class ConsentForm3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentForm_Appointments_AppointmentId",
                table: "ConsentForm");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                table: "ConsentForm",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentForm_Appointments_AppointmentId",
                table: "ConsentForm",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentForm_Appointments_AppointmentId",
                table: "ConsentForm");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentId",
                table: "ConsentForm",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentForm_Appointments_AppointmentId",
                table: "ConsentForm",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
