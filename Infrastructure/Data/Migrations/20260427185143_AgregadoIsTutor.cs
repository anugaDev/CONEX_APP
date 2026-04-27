using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CONEX_APP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoIsTutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTutor",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTutor",
                table: "Users");
        }
    }
}
