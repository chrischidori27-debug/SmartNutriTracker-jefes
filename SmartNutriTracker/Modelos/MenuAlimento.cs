using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

[Index("MenuId", "AlimentoId", Name = "UQ_MenuAlimentos", IsUnique = true)]
public partial class MenuAlimento
{
    [Key]
    public int Id { get; set; }

    public int MenuId { get; set; }

    public int AlimentoId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Cantidad { get; set; }

    [ForeignKey("AlimentoId")]
    [InverseProperty("MenuAlimentos")]
    public virtual Alimento Alimento { get; set; } = null!;

    [ForeignKey("MenuId")]
    [InverseProperty("MenuAlimentos")]
    public virtual MenusDiario Menu { get; set; } = null!;
}
