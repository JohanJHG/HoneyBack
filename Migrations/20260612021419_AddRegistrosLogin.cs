using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HoneyBack.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrosLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosLogin",
                columns: table => new
                {
                    RegistroLoginID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    FechaLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    IP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosLogin", x => x.RegistroLoginID);
                    table.ForeignKey(
                        name: "FK_RegistrosLogin_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_RegistrosLogin_Fecha",
                table: "RegistrosLogin",
                column: "FechaLogin");

            migrationBuilder.CreateIndex(
                name: "IDX_RegistrosLogin_Usuario",
                table: "RegistrosLogin",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IDX_RegistrosLogin_Usuario_Fecha",
                table: "RegistrosLogin",
                columns: new[] { "UsuarioID", "FechaLogin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosLogin");
        }
    }
}
