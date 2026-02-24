using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KimmyEsthi.Migrations
{
    /// <inheritdoc />
    public partial class AddSkincareHistoryQuestionnaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkincareHistoryQuestionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EverReceivedFacial = table.Column<string>(type: "TEXT", nullable: false),
                    LastFacialDate = table.Column<string>(type: "TEXT", nullable: true),
                    Retinol = table.Column<string>(type: "TEXT", nullable: false),
                    ChemPeel = table.Column<string>(type: "TEXT", nullable: false),
                    LastChemPeelDate = table.Column<string>(type: "TEXT", nullable: true),
                    HairRemoval = table.Column<string>(type: "TEXT", nullable: false),
                    MedicalConditions = table.Column<string>(type: "TEXT", nullable: false),
                    Allergies = table.Column<string>(type: "TEXT", nullable: false),
                    Botox = table.Column<string>(type: "TEXT", nullable: false),
                    NegativeReaction = table.Column<string>(type: "TEXT", nullable: false),
                    SkinType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkincareHistoryQuestionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkincareHistoryQuestionnaires_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkincareHistoryQuestionnaires_ClientId",
                table: "SkincareHistoryQuestionnaires",
                column: "ClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkincareHistoryQuestionnaires");
        }
    }
}
