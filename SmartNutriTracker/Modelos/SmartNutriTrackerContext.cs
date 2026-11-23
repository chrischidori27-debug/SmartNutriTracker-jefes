using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

public partial class SmartNutriTrackerContext : DbContext
{
    public SmartNutriTrackerContext(DbContextOptions<SmartNutriTrackerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alimento> Alimentos { get; set; }

    public virtual DbSet<LogsAuditorium> LogsAuditoria { get; set; }

    public virtual DbSet<MenuAlimento> MenuAlimentos { get; set; }

    public virtual DbSet<MenusDiario> MenusDiarios { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alimento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alimento__3214EC072A167132");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<LogsAuditorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LogsAudi__3214EC075EE6C29A");

            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.LogsAuditoria).HasConstraintName("FK_LogsAuditoria_Usuarios");
        });

        modelBuilder.Entity<MenuAlimento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MenuAlim__3214EC07BA8D8E2E");

            entity.HasOne(d => d.Alimento).WithMany(p => p.MenuAlimentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuAlimentos_Alimentos");

            entity.HasOne(d => d.Menu).WithMany(p => p.MenuAlimentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuAlimentos_Menus");
        });

        modelBuilder.Entity<MenusDiario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MenusDia__3214EC07F12BAD70");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MenusDiarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenusDiarios_Usuarios");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07111462A6");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07EAD751B1");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
