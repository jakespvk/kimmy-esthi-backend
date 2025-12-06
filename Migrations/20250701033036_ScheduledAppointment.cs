using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class ScheduledAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledAppointment",
                columns: table => new
                {
                    AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreferredName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    SkinConcerns = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledAppointment", x => x.AppointmentId);
                }
            );

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledAppointmentAppointmentId = table.Column<Guid>(
                        type: "TEXT",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_ScheduledAppointment_ScheduledAppointmentAppointmentId",
                        column: x => x.ScheduledAppointmentAppointmentId,
                        principalTable: "ScheduledAppointment",
                        principalColumn: "AppointmentId"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ScheduledAppointmentAppointmentId",
                table: "Appointments",
                column: "ScheduledAppointmentAppointmentId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Appointments");

            migrationBuilder.DropTable(name: "ScheduledAppointment");
        }
    }
}
