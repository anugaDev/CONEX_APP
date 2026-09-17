using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CONEX_APP.Migrations
{
    /// <inheritdoc />
    public partial class AddRenewalsAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RenewalPeriodMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    RenewalCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    ClassCost = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Renewals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Renewals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Renewals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "ClassCost", "RenewalCost", "RenewalPeriodMonths" },
                values: new object[] { 1, 0m, 0m, 12 });

            migrationBuilder.CreateIndex(
                name: "IX_Renewals_UserId",
                table: "Renewals",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Renewals");
            migrationBuilder.DropTable(name: "AppSettings");
        }
    }
}
