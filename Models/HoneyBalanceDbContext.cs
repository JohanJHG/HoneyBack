using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Models;

public partial class HoneyBalanceDbContext : DbContext
{
    public HoneyBalanceDbContext()
    {
    }

    public HoneyBalanceDbContext(DbContextOptions<HoneyBalanceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoriasTransaccione> CategoriasTransacciones { get; set; }

    public virtual DbSet<ConfiguracionesUsuario> ConfiguracionesUsuarios { get; set; }

    public virtual DbSet<EstadisticasMensuale> EstadisticasMensuales { get; set; }

    public virtual DbSet<MensajesContacto> MensajesContactos { get; set; }

    public virtual DbSet<MetasAhorro> MetasAhorros { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Sesione> Sesiones { get; set; }

    public virtual DbSet<Template> Templates { get; set; }

    public virtual DbSet<Transaccione> Transacciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // NOTA: En runtime, la conexión se configura en Program.cs vía AddDbContext
        // Este método solo se usa para design-time (migraciones EF)
        // NO incluir conexión hardcodeada aquí para evitar conflictos en Docker
        
        // Para migraciones en desarrollo, usar:
        // dotnet ef migrations add <NombreMigracion> --project HoneyBack.csproj
        // La conexión se toma de appsettings.json o variables de entorno
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriasTransaccione>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__F353C1C548B5BF7B");

            entity.HasIndex(e => e.Tipo, "IDX_Categorias_Tipo");

            entity.HasIndex(e => e.UsuarioId, "IDX_Categorias_Usuario");

            entity.HasIndex(e => new { e.Nombre, e.UsuarioId, e.Tipo }, "UQ_Categoria_Usuario").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#FFD8A9");
            entity.Property(e => e.EsSistema).HasDefaultValue(false);
            entity.Property(e => e.Icono).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Tipo).HasMaxLength(20);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CategoriasTransacciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Categorias_Usuarios");
        });

        modelBuilder.Entity<ConfiguracionesUsuario>(entity =>
        {
            entity.HasKey(e => e.ConfiguracionId).HasName("PK__Configur__9B95E0564150AC51");

            entity.ToTable("ConfiguracionesUsuario");

            entity.HasIndex(e => e.UsuarioId, "IDX_Configuraciones_Usuario");

            entity.HasIndex(e => e.UsuarioId, "UQ__Configur__2B3DE7994E97C4E8").IsUnique();

            entity.Property(e => e.ConfiguracionId).HasColumnName("ConfiguracionID");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FormatoFecha)
                .HasMaxLength(20)
                .HasDefaultValue("DD/MM/YYYY");
            entity.Property(e => e.Idioma)
                .HasMaxLength(10)
                .HasDefaultValue("es");
            entity.Property(e => e.NotificacionesEmail).HasDefaultValue(true);
            entity.Property(e => e.NotificacionesPush).HasDefaultValue(true);
            entity.Property(e => e.PrimeraVez).HasDefaultValue(true);
            entity.Property(e => e.Tema)
                .HasMaxLength(20)
                .HasDefaultValue("dark");
            entity.Property(e => e.Timezone)
                .HasMaxLength(50)
                .HasDefaultValue("America/Bogota");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.MonedaPreferida)
                .HasMaxLength(3)
                .HasDefaultValue("COP");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100);
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("AvatarURL");
            entity.Property(e => e.EsVeterano)
                .HasDefaultValue(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithOne(p => p.ConfiguracionesUsuario)
                .HasForeignKey<ConfiguracionesUsuario>(d => d.UsuarioId)
                .HasConstraintName("FK_Configuraciones_Usuarios");
        });

        modelBuilder.Entity<EstadisticasMensuale>(entity =>
        {
            entity.HasKey(e => e.EstadisticaId).HasName("PK__Estadist__5E78B5EC289011CE");

            entity.HasIndex(e => new { e.UsuarioId, e.Anio, e.Mes }, "IDX_Estadisticas_Usuario_Periodo").IsDescending(false, true, true);

            entity.HasIndex(e => new { e.UsuarioId, e.Anio, e.Mes }, "UQ_Periodo_Usuario").IsUnique();

            entity.Property(e => e.EstadisticaId).HasColumnName("EstadisticaID");
            entity.Property(e => e.Balance)
                .HasComputedColumnSql("([TotalIngresos]-[TotalGastos])", true)
                .HasColumnType("decimal(16, 2)");
            entity.Property(e => e.CategoriaMayorGastoId).HasColumnName("CategoriaMayorGastoID");
            entity.Property(e => e.FechaCalculo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MontoMayorGasto).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.NumTransacciones).HasDefaultValue(0);
            entity.Property(e => e.TotalGastos)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalIngresos)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.CategoriaMayorGasto).WithMany(p => p.EstadisticasMensuales)
                .HasForeignKey(d => d.CategoriaMayorGastoId)
                .HasConstraintName("FK_Estadisticas_Categorias");

            entity.HasOne(d => d.Usuario).WithMany(p => p.EstadisticasMensuales)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Estadisticas_Usuarios");
        });

        modelBuilder.Entity<MensajesContacto>(entity =>
        {
            entity.HasKey(e => e.ContactoId).HasName("PK__Mensajes__8E0F85C84F21B9E0");

            entity.ToTable("MensajesContacto");

            entity.Property(e => e.ContactoId).HasColumnName("ContactoID");
            entity.Property(e => e.Asunto).HasMaxLength(300);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaLeido).HasColumnType("datetime");
            entity.Property(e => e.FechaRespuesta).HasColumnType("datetime");
            entity.Property(e => e.Leido).HasDefaultValue(false);
            entity.Property(e => e.Nombre).HasMaxLength(255);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MensajesContactos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_MensajesContacto_Usuarios");
        });

        modelBuilder.Entity<MetasAhorro>(entity =>
        {
            entity.HasKey(e => e.MetaId).HasName("PK__MetasAho__60EE57F83C03D1E5");

            entity.ToTable("MetasAhorro");

            entity.HasIndex(e => e.Activa, "IDX_Metas_Activa");

            entity.HasIndex(e => e.Prioridad, "IDX_Metas_Prioridad").IsDescending();

            entity.HasIndex(e => e.UsuarioId, "IDX_Metas_Usuario");

            entity.Property(e => e.MetaId).HasColumnName("MetaID");
            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .HasDefaultValue("otro");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#FFD8A9");
            entity.Property(e => e.Completada).HasDefaultValue(false);
            entity.Property(e => e.FechaCompletada).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Icono).HasMaxLength(50);
            entity.Property(e => e.MontoActual)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.MontoObjetivo).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Prioridad).HasDefaultValue(0);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MetasAhorros)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Metas_Usuarios");
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.ReporteId).HasName("PK__Reportes__0B29EA4E01DDEDB4");

            entity.Property(e => e.ReporteId).HasColumnName("ReporteID");
            entity.Property(e => e.ArchivoUrl)
                .HasMaxLength(500)
                .HasColumnName("ArchivoURL");
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaGeneracion).HasColumnType("datetime");
            entity.Property(e => e.FechaReporte)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(255);
            entity.Property(e => e.TipoReporte).HasMaxLength(50);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reportes)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Reportes_Usuarios");
        });

        modelBuilder.Entity<Sesione>(entity =>
        {
            entity.HasKey(e => e.SesionId).HasName("PK__Sesiones__52FD7C0689AF3185");

            entity.HasIndex(e => e.TokenSesion, "UQ__Sesiones__567B1115B8F65C64").IsUnique();

            entity.Property(e => e.SesionId).HasColumnName("SesionID");
            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(45)
                .HasColumnName("IPAddress");
            entity.Property(e => e.TokenSesion).HasMaxLength(500);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Sesiones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sesiones_UsuarioID");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Template__F87ADD07FF1A0B47");

            entity.HasIndex(e => e.Activo, "IDX_Templates_Activo");

            entity.HasIndex(e => e.FrecuenciaUso, "IDX_Templates_Frecuencia").IsDescending();

            entity.HasIndex(e => e.UsuarioId, "IDX_Templates_Usuario");

            entity.HasIndex(e => new { e.UsuarioId, e.Nombre }, "UQ_Template_Nombre_Usuario").IsUnique();

            entity.Property(e => e.TemplateId).HasColumnName("TemplateID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.CategoriaId).HasColumnName("CategoriaID");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaUltimoUso).HasColumnType("datetime");
            entity.Property(e => e.FrecuenciaUso).HasDefaultValue(0);
            entity.Property(e => e.Monto).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Tipo).HasMaxLength(20);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Templates)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Templates_Categorias");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Templates)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Templates_Usuarios");
        });

        modelBuilder.Entity<Transaccione>(entity =>
        {
            entity.HasKey(e => e.TransaccionId).HasName("PK_Transacciones");

            entity.ToTable("Transacciones");

            entity.HasIndex(e => e.UsuarioId, "IDX_Transacciones_Usuario");

            entity.HasIndex(e => e.Fecha, "IDX_Transacciones_Fecha");

            entity.HasIndex(e => e.Tipo, "IDX_Transacciones_Tipo");

            entity.Property(e => e.TransaccionId).HasColumnName("TransaccionID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Monto).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Tipo).HasMaxLength(20);
            entity.Property(e => e.Categoria).HasMaxLength(50);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Transacciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Transacciones_Usuarios");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7981B407AB9");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D10534C1437617").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("AvatarURL");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaUltimaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PreferenciasMoneda)
                .HasMaxLength(3)
                .HasDefaultValue("COP");
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK_PasswordResetTokens");

            entity.ToTable("PasswordResetTokens");

            entity.HasIndex(e => e.Token, "IDX_PasswordResetTokens_Token");
            entity.HasIndex(e => e.UsuarioId, "IDX_PasswordResetTokens_Usuario");
            entity.HasIndex(e => new { e.UsuarioId, e.Used, e.ExpiresAtUtc }, "IDX_PasswordResetTokens_Active");

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Token)
                .HasMaxLength(6)
                .IsFixedLength();
            entity.Property(e => e.CreatedAtUtc)
                .HasColumnType("datetime2");
            entity.Property(e => e.ExpiresAtUtc)
                .HasColumnType("datetime2");
            entity.Property(e => e.Used)
                .HasDefaultValue(false);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PasswordResetTokens_Usuarios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
