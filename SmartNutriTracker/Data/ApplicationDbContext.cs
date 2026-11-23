using Microsoft.EntityFrameworkCore;
using SmartNutriTracker_biblio.Entities;

namespace SmartNutriTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<PerfilEstudiante> PerfilesEstudiante => Set<PerfilEstudiante>();
    public DbSet<HistorialFisico> HistorialesFisicos => Set<HistorialFisico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.Property(x => x.Nombre).HasMaxLength(50);
            e.Property(x => x.Descripcion).HasMaxLength(150);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.Property(x => x.Nombre).HasMaxLength(50);
            e.Property(x => x.Correo).HasMaxLength(150);
            e.Property(x => x.PasswordHash).HasMaxLength(500);
            e.Property(x => x.FechaRegistro).HasColumnType("datetime2");
            e.HasOne(x => x.Role).WithMany(r => r.Usuarios).HasForeignKey(x => x.RolId);
        });

        modelBuilder.Entity<LogAuditoria>(e =>
        {
            e.ToTable("LogsAuditoria");
            e.Property(x => x.Accion).HasMaxLength(100);
            e.Property(x => x.Descripcion).HasMaxLength(500);
            e.Property(x => x.Fecha).HasColumnType("datetime2");
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<PerfilEstudiante>(e =>
        {
            e.ToTable("PerfilesEstudiante");
            e.Property(x => x.FechaNacimiento).HasColumnType("date");
            e.Property(x => x.Objetivo).HasMaxLength(50);
            e.HasOne(x => x.Usuario).WithOne(u => u.Perfil).HasForeignKey<PerfilEstudiante>(x => x.UsuarioId);
        });

        modelBuilder.Entity<HistorialFisico>(e =>
        {
            e.ToTable("HistorialFisico");
            e.Property(x => x.Peso).HasColumnType("decimal(5,2)");
            e.Property(x => x.Altura).HasColumnType("decimal(4,2)");
            e.Property(x => x.Imc).HasColumnName("IMC").HasColumnType("decimal(5,2)");
            e.Property(x => x.FechaRegistro).HasColumnType("datetime2");
            e.HasOne(x => x.Perfil).WithMany(p => p.Historiales).HasForeignKey(x => x.PerfilId);
        });
    }
}