using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class ConsentForm2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ConsentForm_ConsentFormId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ConsentFormId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConsentFormId",
                table: "Appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                table: "ConsentForm",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ConsentForm_AppointmentId",
                table: "ConsentForm",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentForm_Appointments_AppointmentId",
                table: "ConsentForm",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentForm_Appointments_AppointmentId",
                table: "ConsentForm");

            migrationBuilder.DropIndex(
                name: "IX_ConsentForm_AppointmentId",
                table: "ConsentForm");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "ConsentForm");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsentFormId",
                table: "Appointments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ConsentFormId",
                table: "Appointments",
                column: "ConsentFormId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ConsentForm_ConsentFormId",
                table: "Appointments",
                column: "ConsentFormId",
                principalTable: "ConsentForm",
                principalColumn: "Id");
        }
    }
}
