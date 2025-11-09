using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class Move_ServiceName_ToScheduledAppointment2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentRequest",
                columns: table => new
                {
                    AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduledAppointmentAppointmentId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentRequest", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_AppointmentRequest_ScheduledAppointments_ScheduledAppointmentAppointmentId",
                        column: x => x.ScheduledAppointmentAppointmentId,
                        principalTable: "ScheduledAppointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequest_ScheduledAppointmentAppointmentId",
                table: "AppointmentRequest",
                column: "ScheduledAppointmentAppointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentRequest");
        }
    }
}
