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

    public virtual DbSet<MensajesContacto> MensajesContactos { get; set; }

    public virtual DbSet<Reporte> Reportes { get; set; }

    public virtual DbSet<Sesione> Sesiones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MensajesContacto>(entity =>
        {
            entity.HasKey(e => e.ContactoId).HasName("PK__Mensajes__8E0F85C84F21B9E0");

            entity.ToTable("MensajesContacto");

            entity.Property(e => e.ContactoId).HasColumnName("ContactoID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(255);
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.HasKey(e => e.ReporteId).HasName("PK__Reportes__0B29EA4E01DDEDB4");

            entity.Property(e => e.ReporteId).HasColumnName("ReporteID");
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");
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
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.TokenSesion).HasMaxLength(500);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Sesiones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sesiones_UsuarioID");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7981B407AB9");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D10534C1437617").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
