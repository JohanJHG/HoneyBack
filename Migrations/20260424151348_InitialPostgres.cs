using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HoneyBack.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCompleto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    Activo = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    PreferenciasMoneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    AvatarURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__2B3DE7981B407AB9", x => x.UsuarioID);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesUsuario",
                columns: table => new
                {
                    ConfiguracionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    NotificacionesEmail = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    NotificacionesPush = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Tema = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "dark"),
                    Idioma = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "es"),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "America/Bogota"),
                    FormatoFecha = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "DD/MM/YYYY"),
                    PrimeraVez = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    ConfiguracionPersonalizada = table.Column<string>(type: "text", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    MonedaPreferida = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    NombreUsuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EsVeterano = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Configur__9B95E0564150AC51", x => x.ConfiguracionID);
                    table.ForeignKey(
                        name: "FK_Configuraciones_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntornosPersonales",
                columns: table => new
                {
                    EntornoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    ModuloClave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Subtitulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ValorPrincipal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Etiqueta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DatosJson = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntornosPersonales", x => x.EntornoID);
                    table.ForeignKey(
                        name: "FK_EntornosPersonales_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MensajesContacto",
                columns: table => new
                {
                    ContactoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    Asunto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    UsuarioID = table.Column<int>(type: "integer", nullable: true),
                    Leido = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    FechaLeido = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Respuesta = table.Column<string>(type: "text", nullable: true),
                    FechaRespuesta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Mensajes__8E0F85C84F21B9E0", x => x.ContactoID);
                    table.ForeignKey(
                        name: "FK_MensajesContacto_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MetasAhorro",
                columns: table => new
                {
                    MetaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "otro"),
                    MontoObjetivo = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    MontoActual = table.Column<decimal>(type: "numeric(15,2)", nullable: true, defaultValue: 0m),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    FechaObjetivo = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCompletada = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true, defaultValue: "#FFD8A9"),
                    Icono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Prioridad = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    Activa = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Completada = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MetasAho__60EE57F83C03D1E5", x => x.MetaID);
                    table.ForeignKey(
                        name: "FK_Metas_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    TokenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character(6)", fixedLength: true, maxLength: 6, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.TokenID);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sesiones",
                columns: table => new
                {
                    SesionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    TokenSesion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()"),
                    IPAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sesiones__52FD7C0689AF3185", x => x.SesionID);
                    table.ForeignKey(
                        name: "FK_Sesiones_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "Transacciones",
                columns: table => new
                {
                    TransaccionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacciones", x => x.TransaccionID);
                    table.ForeignKey(
                        name: "FK_Transacciones_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Configuraciones_Usuario",
                table: "ConfiguracionesUsuario",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "UQ__Configur__2B3DE7994E97C4E8",
                table: "ConfiguracionesUsuario",
                column: "UsuarioID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_Entornos_Usuario_Modulo",
                table: "EntornosPersonales",
                columns: new[] { "UsuarioID", "ModuloClave" });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesContacto_UsuarioID",
                table: "MensajesContacto",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IDX_Metas_Activa",
                table: "MetasAhorro",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IDX_Metas_Prioridad",
                table: "MetasAhorro",
                column: "Prioridad",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IDX_Metas_Usuario",
                table: "MetasAhorro",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IDX_PasswordResetTokens_Active",
                table: "PasswordResetTokens",
                columns: new[] { "UsuarioID", "Used", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IDX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IDX_PasswordResetTokens_Usuario",
                table: "PasswordResetTokens",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Sesiones_UsuarioID",
                table: "Sesiones",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "UQ__Sesiones__567B1115B8F65C64",
                table: "Sesiones",
                column: "TokenSesion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_Transacciones_Fecha",
                table: "Transacciones",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IDX_Transacciones_Tipo",
                table: "Transacciones",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IDX_Transacciones_Usuario",
                table: "Transacciones",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__A9D10534C1437617",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesUsuario");

            migrationBuilder.DropTable(
                name: "EntornosPersonales");

            migrationBuilder.DropTable(
                name: "MensajesContacto");

            migrationBuilder.DropTable(
                name: "MetasAhorro");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "Sesiones");

            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
