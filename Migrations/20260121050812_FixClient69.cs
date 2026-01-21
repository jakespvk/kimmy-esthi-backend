using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class FixClient69 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledAppointment_Clients_AppointmentId",
                table: "ScheduledAppointment");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "ScheduledAppointment",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledAppointment_ClientId",
                table: "ScheduledAppointment",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledAppointment_Clients_ClientId",
                table: "ScheduledAppointment",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledAppointment_Clients_ClientId",
                table: "ScheduledAppointment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledAppointment_ClientId",
                table: "ScheduledAppointment");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ScheduledAppointment");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledAppointment_Clients_AppointmentId",
                table: "ScheduledAppointment",
                column: "AppointmentId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
