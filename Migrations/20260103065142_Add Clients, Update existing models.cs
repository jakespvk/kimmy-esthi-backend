using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddClientsUpdateexistingmodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "ScheduledAppointment");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "ScheduledAppointment");

            migrationBuilder.RenameColumn(
                name: "PreferredName",
                table: "ScheduledAppointment",
                newName: "ClientId");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsentFormId",
                table: "Appointments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreferredName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    SkinConcerns = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "ConsentForm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrintedName = table.Column<string>(type: "TEXT", nullable: false),
                    InitialedStatements = table.Column<string>(type: "TEXT", nullable: false),
                    Initials = table.Column<string>(type: "TEXT", nullable: false),
                    Signature = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentForm_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledAppointment_ClientId",
                table: "ScheduledAppointment",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ConsentFormId",
                table: "Appointments",
                column: "ConsentFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DateTime",
                table: "Appointments",
                column: "DateTime",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsentForm_ClientId",
                table: "ConsentForm",
                column: "ClientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ConsentForm_ConsentFormId",
                table: "Appointments",
                column: "ConsentFormId",
                principalTable: "ConsentForm",
                principalColumn: "Id");

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
                name: "FK_Appointments_ConsentForm_ConsentFormId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledAppointment_Clients_ClientId",
                table: "ScheduledAppointment");

            migrationBuilder.DropTable(
                name: "ConsentForm");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledAppointment_ClientId",
                table: "ScheduledAppointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ConsentFormId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DateTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConsentFormId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ScheduledAppointment",
                newName: "PreferredName");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ScheduledAppointment",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "ScheduledAppointment",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
