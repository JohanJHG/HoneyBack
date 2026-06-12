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

    public virtual DbSet<ConfiguracionesUsuario> ConfiguracionesUsuarios { get; set; }

    public virtual DbSet<MensajesContacto> MensajesContactos { get; set; }

    public virtual DbSet<MetasAhorro> MetasAhorros { get; set; }

    public virtual DbSet<Sesione> Sesiones { get; set; }

    public virtual DbSet<Transaccione> Transacciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<EntornoPersonal> EntornosPersonales { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RegistroLogin> RegistrosLogin { get; set; }

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
        modelBuilder.Entity<ConfiguracionesUsuario>(entity =>
        {
            entity.HasKey(e => e.ConfiguracionId).HasName("PK__Configur__9B95E0564150AC51");

            entity.ToTable("ConfiguracionesUsuario");

            entity.HasIndex(e => e.UsuarioId, "IDX_Configuraciones_Usuario");

            entity.HasIndex(e => e.UsuarioId, "UQ__Configur__2B3DE7994E97C4E8").IsUnique();

            entity.Property(e => e.ConfiguracionId).HasColumnName("ConfiguracionID");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
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
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Usuario).WithOne(p => p.ConfiguracionesUsuario)
                .HasForeignKey<ConfiguracionesUsuario>(d => d.UsuarioId)
                .HasConstraintName("FK_Configuraciones_Usuarios");
        });

        modelBuilder.Entity<MensajesContacto>(entity =>
        {
            entity.HasKey(e => e.ContactoId).HasName("PK__Mensajes__8E0F85C84F21B9E0");

            entity.ToTable("MensajesContacto");

            entity.Property(e => e.ContactoId).HasColumnName("ContactoID");
            entity.Property(e => e.Asunto).HasMaxLength(300);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaLeido).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaRespuesta).HasColumnType("timestamp without time zone");
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
            entity.Property(e => e.FechaCompletada).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("CURRENT_DATE");
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

        modelBuilder.Entity<Sesione>(entity =>
        {
            entity.HasKey(e => e.SesionId).HasName("PK__Sesiones__52FD7C0689AF3185");

            entity.HasIndex(e => e.TokenSesion, "UQ__Sesiones__567B1115B8F65C64").IsUnique();

            entity.Property(e => e.SesionId).HasColumnName("SesionID");
            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaExpiracion).HasColumnType("timestamp without time zone");
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
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

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
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaUltimaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NombreCompleto).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PreferenciasMoneda)
                .HasMaxLength(3)
                .HasDefaultValue("COP");
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Rol)
                .HasDefaultValue(RolUsuario.Usuario)
                .HasConversion<int>();
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
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExpiresAtUtc)
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Used)
                .HasDefaultValue(false);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PasswordResetTokens_Usuarios");
        });

        modelBuilder.Entity<EntornoPersonal>(entity =>
        {
            entity.HasKey(e => e.EntornoId).HasName("PK_EntornosPersonales");

            entity.ToTable("EntornosPersonales");

            entity.HasIndex(e => new { e.UsuarioId, e.ModuloClave }, "IDX_Entornos_Usuario_Modulo");

            entity.Property(e => e.EntornoId).HasColumnName("EntornoID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.ModuloClave).HasMaxLength(50);
            entity.Property(e => e.Titulo).HasMaxLength(200);
            entity.Property(e => e.Subtitulo).HasMaxLength(300);
            entity.Property(e => e.ValorPrincipal).HasMaxLength(100);
            entity.Property(e => e.Etiqueta).HasMaxLength(50);
            entity.Property(e => e.DatosJson).HasColumnType("text");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Usuario).WithMany(p => p.EntornosPersonales)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EntornosPersonales_Usuarios");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("RefreshTokens");

            entity.HasIndex(e => e.Token, "IDX_RefreshTokens_Token").IsUnique();
            entity.HasIndex(e => new { e.UsuarioId, e.IsRevoked, e.ExpiresAt }, "IDX_RefreshTokens_Active");

            entity.Property(e => e.Token).HasMaxLength(512);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(512);
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshTokens_Usuarios");
        });

        modelBuilder.Entity<RegistroLogin>(entity =>
        {
            entity.HasKey(e => e.RegistroLoginId).HasName("PK_RegistrosLogin");

            entity.ToTable("RegistrosLogin");

            entity.HasIndex(e => e.UsuarioId, "IDX_RegistrosLogin_Usuario");
            entity.HasIndex(e => e.FechaLogin, "IDX_RegistrosLogin_Fecha");
            entity.HasIndex(e => new { e.UsuarioId, e.FechaLogin }, "IDX_RegistrosLogin_Usuario_Fecha");

            entity.Property(e => e.RegistroLoginId).HasColumnName("RegistroLoginID");
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.FechaLogin)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Ip)
                .HasMaxLength(45)
                .HasColumnName("IP");

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RegistrosLogin_Usuarios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimeKindsForPostgreSql();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateTimeKindsForPostgreSql();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizeDateTimeKindsForPostgreSql()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e =>
                     e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) &&
                    property.CurrentValue is DateTime dateTimeValue &&
                    dateTimeValue.Kind != DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Unspecified);
                    continue;
                }

                if (property.Metadata.ClrType == typeof(DateTime?) &&
                    property.CurrentValue is DateTime nullableDateTimeValue &&
                    nullableDateTimeValue.Kind != DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(nullableDateTimeValue, DateTimeKind.Unspecified);
                }
            }
        }
    }
}
