using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kimmy_esthi_backend.Migrations
{
    /// <inheritdoc />
    public partial class Services : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceName = table.Column<string>(type: "TEXT", nullable: false),
                    PromotionName = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceCardType = table.Column<int>(type: "INTEGER", nullable: false),
                    CardTitle = table.Column<string>(type: "TEXT", nullable: false),
                    CardContent = table.Column<string>(type: "TEXT", nullable: false),
                    CardImgSrc = table.Column<string>(type: "TEXT", nullable: false),
                    CardOverlayContent = table.Column<string>(type: "TEXT", nullable: true),
                    PackageItems = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    NotBookable = table.Column<bool>(type: "INTEGER", nullable: true),
                    Price = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
