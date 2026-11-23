using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

[Index("Correo", Name = "UQ__Usuarios__60695A199444227F", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string Correo { get; set; } = null!;

    [StringLength(500)]
    public string PasswordHash { get; set; } = null!;

    public int RolId { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<LogsAuditorium> LogsAuditoria { get; set; } = new List<LogsAuditorium>();

    [InverseProperty("Usuario")]
    public virtual ICollection<MenusDiario> MenusDiarios { get; set; } = new List<MenusDiario>();

    [ForeignKey("RolId")]
    [InverseProperty("Usuarios")]
    public virtual Role Rol { get; set; } = null!;
}
