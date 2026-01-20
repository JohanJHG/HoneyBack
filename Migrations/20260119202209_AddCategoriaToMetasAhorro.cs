using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoneyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaToMetasAhorro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "MetasAhorro",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "otro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "MetasAhorro");
        }
    }
}
