using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

public partial class LogsAuditorium
{
    [Key]
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    [StringLength(100)]
    public string Accion { get; set; } = null!;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("LogsAuditoria")]
    public virtual Usuario? Usuario { get; set; }
}
