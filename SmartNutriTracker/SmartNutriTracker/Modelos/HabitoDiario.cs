using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

[Index("UsuarioId", "Fecha", Name = "UQ_HabitosDiarios_Usuario_Fecha", IsUnique = true)]
public partial class HabitoDiario
{
    [Key]
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    [Column(TypeName = "decimal(4, 2)")]
    public decimal HorasSueno { get; set; }

    [Column(TypeName = "decimal(4, 2)")]
    public decimal AguaLitros { get; set; }

    public int MinutosActividad { get; set; }

    [StringLength(80)]
    public string? TipoActividad { get; set; }

    public DateTime FechaRegistro { get; set; }

    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
}
