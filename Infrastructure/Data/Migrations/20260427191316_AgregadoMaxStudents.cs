using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CONEX_APP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoMaxStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxStudents",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStudents",
                table: "Activities");
        }
    }
}
