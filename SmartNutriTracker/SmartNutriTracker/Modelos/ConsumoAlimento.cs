using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

[Index("UsuarioId", "Fecha", Name = "IX_ConsumosAlimento_Usuario_Fecha")]
public partial class ConsumoAlimento
{
    [Key]
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int AlimentoId { get; set; }

    [StringLength(20)]
    public string TipoComida { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Porciones { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Calorias { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Proteinas { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Carbohidratos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Grasas { get; set; }

    public DateTime FechaRegistro { get; set; }

    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey("AlimentoId")]
    public virtual Alimento Alimento { get; set; } = null!;
}
