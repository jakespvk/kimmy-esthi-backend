using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class FixAppointmentToScheduledApptRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ScheduledAppointment_ScheduledAppointmentAppointmentId",
                table: "Appointments"
            );

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ScheduledAppointmentAppointmentId",
                table: "Appointments"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledAppointment",
                table: "ScheduledAppointment"
            );

            migrationBuilder.DropColumn(
                name: "ScheduledAppointmentAppointmentId",
                table: "Appointments"
            );

            migrationBuilder.RenameTable(
                name: "ScheduledAppointment",
                newName: "ScheduledAppointments"
            );

            migrationBuilder
                .AlterColumn<Guid>(
                    name: "Id",
                    table: "Appointments",
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER"
                )
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledAppointments",
                table: "ScheduledAppointments",
                column: "AppointmentId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledAppointments_Appointments_AppointmentId",
                table: "ScheduledAppointments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledAppointments_Appointments_AppointmentId",
                table: "ScheduledAppointments"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScheduledAppointments",
                table: "ScheduledAppointments"
            );

            migrationBuilder.RenameTable(
                name: "ScheduledAppointments",
                newName: "ScheduledAppointment"
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "Appointments",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(Guid),
                    oldType: "TEXT"
                )
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduledAppointmentAppointmentId",
                table: "Appointments",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScheduledAppointment",
                table: "ScheduledAppointment",
                column: "AppointmentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ScheduledAppointmentAppointmentId",
                table: "Appointments",
                column: "ScheduledAppointmentAppointmentId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ScheduledAppointment_ScheduledAppointmentAppointmentId",
                table: "Appointments",
                column: "ScheduledAppointmentAppointmentId",
                principalTable: "ScheduledAppointment",
                principalColumn: "AppointmentId"
            );
        }
    }
}
