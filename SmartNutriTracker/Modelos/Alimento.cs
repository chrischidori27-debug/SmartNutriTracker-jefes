using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

public partial class Alimento
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [StringLength(80)]
    public string? Categoria { get; set; }

    [StringLength(50)]
    public string Porcion { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Calorias { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Proteinas { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Carbohidratos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Grasas { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Fibra { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("Alimento")]
    public virtual ICollection<MenuAlimento> MenuAlimentos { get; set; } = new List<MenuAlimento>();
}
