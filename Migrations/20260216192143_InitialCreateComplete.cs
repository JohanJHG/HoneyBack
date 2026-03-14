using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoneyBack.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    PreferenciasMoneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    AvatarURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__2B3DE7981B407AB9", x => x.UsuarioID);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasTransacciones",
                columns: table => new
                {
                    CategoriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true, defaultValue: "#FFD8A9"),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsSistema = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__F353C1C548B5BF7B", x => x.CategoriaID);
                    table.ForeignKey(
                        name: "FK_Categorias_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesUsuario",
                columns: table => new
                {
                    ConfiguracionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    NotificacionesEmail = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    NotificacionesPush = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Tema = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "dark"),
                    Idioma = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "es"),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "America/Bogota"),
                    FormatoFecha = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DD/MM/YYYY"),
                    PrimeraVez = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ConfiguracionPersonalizada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    MonedaPreferida = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true, defaultValue: "COP"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvatarURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsVeterano = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
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
                name: "MensajesContacto",
                columns: table => new
                {
                    ContactoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Asunto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Leido = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    FechaLeido = table.Column<DateTime>(type: "datetime", nullable: true),
                    Respuesta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime", nullable: true)
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
                    MetaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "otro"),
                    MontoObjetivo = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    MontoActual = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    FechaObjetivo = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCompletada = table.Column<DateTime>(type: "datetime", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true, defaultValue: "#FFD8A9"),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Prioridad = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Activa = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Completada = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
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
                    TokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nchar(6)", fixedLength: true, maxLength: 6, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
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
                name: "Reportes",
                columns: table => new
                {
                    ReporteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TipoReporte = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaReporte = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Parametros = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    ArchivoURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reportes__0B29EA4E01DDEDB4", x => x.ReporteID);
                    table.ForeignKey(
                        name: "FK_Reportes_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID");
                });

            migrationBuilder.CreateTable(
                name: "Sesiones",
                columns: table => new
                {
                    SesionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    TokenSesion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
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
                    TransaccionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
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

            migrationBuilder.CreateTable(
                name: "EstadisticasMensuales",
                columns: table => new
                {
                    EstadisticaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    TotalIngresos = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    TotalGastos = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    Balance = table.Column<decimal>(type: "decimal(16,2)", nullable: true, computedColumnSql: "([TotalIngresos]-[TotalGastos])", stored: true),
                    NumTransacciones = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CategoriaMayorGastoID = table.Column<int>(type: "int", nullable: true),
                    MontoMayorGasto = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    FechaCalculo = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Estadist__5E78B5EC289011CE", x => x.EstadisticaID);
                    table.ForeignKey(
                        name: "FK_Estadisticas_Categorias",
                        column: x => x.CategoriaMayorGastoID,
                        principalTable: "CategoriasTransacciones",
                        principalColumn: "CategoriaID");
                    table.ForeignKey(
                        name: "FK_Estadisticas_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoriaID = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrecuenciaUso = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    FechaUltimoUso = table.Column<DateTime>(type: "datetime", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Template__F87ADD07FF1A0B47", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK_Templates_Categorias",
                        column: x => x.CategoriaID,
                        principalTable: "CategoriasTransacciones",
                        principalColumn: "CategoriaID");
                    table.ForeignKey(
                        name: "FK_Templates_Usuarios",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Categorias_Tipo",
                table: "CategoriasTransacciones",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IDX_Categorias_Usuario",
                table: "CategoriasTransacciones",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "UQ_Categoria_Usuario",
                table: "CategoriasTransacciones",
                columns: new[] { "Nombre", "UsuarioID", "Tipo" },
                unique: true,
                filter: "[UsuarioID] IS NOT NULL");

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
                name: "IDX_Estadisticas_Usuario_Periodo",
                table: "EstadisticasMensuales",
                columns: new[] { "UsuarioID", "Anio", "Mes" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasMensuales_CategoriaMayorGastoID",
                table: "EstadisticasMensuales",
                column: "CategoriaMayorGastoID");

            migrationBuilder.CreateIndex(
                name: "UQ_Periodo_Usuario",
                table: "EstadisticasMensuales",
                columns: new[] { "UsuarioID", "Anio", "Mes" },
                unique: true);

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
                name: "IX_Reportes_UsuarioID",
                table: "Reportes",
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
                name: "IDX_Templates_Activo",
                table: "Templates",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IDX_Templates_Frecuencia",
                table: "Templates",
                column: "FrecuenciaUso",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IDX_Templates_Usuario",
                table: "Templates",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_CategoriaID",
                table: "Templates",
                column: "CategoriaID");

            migrationBuilder.CreateIndex(
                name: "UQ_Template_Nombre_Usuario",
                table: "Templates",
                columns: new[] { "UsuarioID", "Nombre" },
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
                name: "EstadisticasMensuales");

            migrationBuilder.DropTable(
                name: "MensajesContacto");

            migrationBuilder.DropTable(
                name: "MetasAhorro");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "Reportes");

            migrationBuilder.DropTable(
                name: "Sesiones");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "CategoriasTransacciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
