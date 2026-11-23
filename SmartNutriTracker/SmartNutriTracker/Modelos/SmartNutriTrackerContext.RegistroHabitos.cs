using Microsoft.EntityFrameworkCore;

namespace SmartNutriTracker.Modelos;

public partial class SmartNutriTrackerContext
{
    public virtual DbSet<ConsumoAlimento> ConsumosAlimento { get; set; }

    public virtual DbSet<HabitoDiario> HabitosDiarios { get; set; }
}
