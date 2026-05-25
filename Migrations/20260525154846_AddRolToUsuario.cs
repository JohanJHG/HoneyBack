using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoneyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddRolToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rol",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Asignar SuperAdmin (Rol = 1) al correo de soporte
            migrationBuilder.Sql(@"
                UPDATE ""Usuarios""
                SET ""Rol"" = 1
                WHERE ""Email"" = 'honeybalance.soporte@gmail.com';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Usuarios");
        }
    }
}
