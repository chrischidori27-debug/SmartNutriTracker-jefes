using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public class HabitoServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;

        public HabitoServicio(IDbContextFactory<SmartNutriTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<HabitoDiario?> ObtenerAsync(int usuarioId, DateTime fecha)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var dia = fecha.Date;

            return await context.HabitosDiarios
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.UsuarioId == usuarioId && h.Fecha == dia);
        }

        public async Task<List<HabitoDiario>> ListarRecientesAsync(int usuarioId, DateTime fechaFin, int dias = 7)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var fin = fechaFin.Date;
            var inicio = fin.AddDays(-(dias - 1));

            return await context.HabitosDiarios
                .AsNoTracking()
                .Where(h => h.UsuarioId == usuarioId && h.Fecha >= inicio && h.Fecha <= fin)
                .OrderBy(h => h.Fecha)
                .ToListAsync();
        }

        public async Task<(bool Exito, string Mensaje)> GuardarAsync(
            int usuarioId,
            DateTime fecha,
            decimal horasSueno,
            decimal aguaLitros,
            int minutosActividad,
            string? tipoActividad)
        {
            if (fecha.Date > DateTime.Today)
            {
                return (false, "No se pueden registrar hábitos de fechas futuras.");
            }

            if (horasSueno < 0 || horasSueno > 24)
            {
                return (false, "Las horas de sueño deben estar entre 0 y 24.");
            }

            if (aguaLitros < 0 || aguaLitros > 10)
            {
                return (false, "El consumo de agua debe estar entre 0 y 10 litros.");
            }

            if (minutosActividad < 0 || minutosActividad > 600)
            {
                return (false, "La actividad física debe estar entre 0 y 600 minutos.");
            }

            var actividad = string.IsNullOrWhiteSpace(tipoActividad) || minutosActividad == 0
                ? null
                : tipoActividad.Trim();

            if (actividad != null && actividad.Length > 80)
            {
                return (false, "El tipo de actividad no puede superar los 80 caracteres.");
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            var dia = fecha.Date;

            var habito = await context.HabitosDiarios
                .FirstOrDefaultAsync(h => h.UsuarioId == usuarioId && h.Fecha == dia);

            bool nuevo = habito == null;

            if (habito == null)
            {
                habito = new HabitoDiario
                {
                    UsuarioId = usuarioId,
                    Fecha = dia,
                    FechaRegistro = DateTime.Now
                };

                context.HabitosDiarios.Add(habito);
            }

            habito.HorasSueno = horasSueno;
            habito.AguaLitros = aguaLitros;
            habito.MinutosActividad = minutosActividad;
            habito.TipoActividad = actividad;

            await context.SaveChangesAsync();

            return (true, nuevo
                ? "Hábitos del día registrados correctamente."
                : "Hábitos del día actualizados correctamente.");
        }
    }
}
