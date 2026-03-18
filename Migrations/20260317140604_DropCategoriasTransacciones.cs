using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoneyBack.Migrations
{
    /// <inheritdoc />
    public partial class DropCategoriasTransacciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstadisticasMensuales");

            migrationBuilder.DropTable(
                name: "Reportes");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "CategoriasTransacciones");

            migrationBuilder.CreateTable(
                name: "EntornosPersonales",
                columns: table => new
                {
                    EntornoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    ModuloClave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ValorPrincipal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Etiqueta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DatosJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
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

            migrationBuilder.CreateIndex(
                name: "IDX_Entornos_Usuario_Modulo",
                table: "EntornosPersonales",
                columns: new[] { "UsuarioID", "ModuloClave" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntornosPersonales");

            migrationBuilder.CreateTable(
                name: "CategoriasTransacciones",
                columns: table => new
                {
                    CategoriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true, defaultValue: "#FFD8A9"),
                    EsSistema = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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
                name: "Reportes",
                columns: table => new
                {
                    ReporteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    ArchivoURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime", nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaReporte = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Parametros = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoReporte = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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
                name: "EstadisticasMensuales",
                columns: table => new
                {
                    EstadisticaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaMayorGastoID = table.Column<int>(type: "int", nullable: true),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(16,2)", nullable: true, computedColumnSql: "([TotalIngresos]-[TotalGastos])", stored: true),
                    FechaCalculo = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    MontoMayorGasto = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    NumTransacciones = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    TotalGastos = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m),
                    TotalIngresos = table.Column<decimal>(type: "decimal(15,2)", nullable: true, defaultValue: 0m)
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
                    CategoriaID = table.Column<int>(type: "int", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    FechaUltimoUso = table.Column<DateTime>(type: "datetime", nullable: true),
                    FrecuenciaUso = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Monto = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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
                name: "IX_Reportes_UsuarioID",
                table: "Reportes",
                column: "UsuarioID");

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
        }
    }
}
