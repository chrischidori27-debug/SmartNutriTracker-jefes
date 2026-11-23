using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

public partial class MenusDiario
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public DateOnly Fecha { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("Menu")]
    public virtual ICollection<MenuAlimento> MenuAlimentos { get; set; } = new List<MenuAlimento>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("MenusDiarios")]
    public virtual Usuario Usuario { get; set; } = null!;
}
