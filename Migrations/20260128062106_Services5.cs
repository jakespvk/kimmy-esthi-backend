using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class Services5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceCardType",
                table: "Services",
                newName: "ServiceType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceType",
                table: "Services",
                newName: "ServiceCardType");
        }
    }
}
