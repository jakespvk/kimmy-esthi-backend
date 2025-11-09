using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveScheduledSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledAppointments_Appointments_AppointmentId",
                table: "ScheduledAppointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledAppointments",
                table: "ScheduledAppointments");

            migrationBuilder.RenameTable(
                name: "ScheduledAppointments",
                newName: "ScheduledAppointment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledAppointment",
                table: "ScheduledAppointment",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledAppointment_Appointments_AppointmentId",
                table: "ScheduledAppointment",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledAppointment_Appointments_AppointmentId",
                table: "ScheduledAppointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledAppointment",
                table: "ScheduledAppointment");

            migrationBuilder.RenameTable(
                name: "ScheduledAppointment",
                newName: "ScheduledAppointments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledAppointments",
                table: "ScheduledAppointments",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledAppointments_Appointments_AppointmentId",
                table: "ScheduledAppointments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
