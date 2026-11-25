using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public record ResumenDia(DateTime Fecha, decimal Calorias);

    public record ResumenNutricionalDia(
        DateTime Fecha,
        decimal Calorias,
        decimal Proteinas,
        decimal Carbohidratos,
        decimal Grasas,
        int Registros);

    public class ConsumoServicio
    {
        public static readonly string[] TiposComida =
        {
            "Desayuno", "Media mañana", "Almuerzo", "Merienda", "Cena"
        };

        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;

        public ConsumoServicio(IDbContextFactory<SmartNutriTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ConsumoAlimento>> ListarPorDiaAsync(int usuarioId, DateTime fecha)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var dia = fecha.Date;

            return await context.ConsumosAlimento
                .Include(c => c.Alimento)
                .Where(c => c.UsuarioId == usuarioId && c.Fecha == dia)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarAsync(
            int usuarioId,
            int alimentoId,
            string tipoComida,
            decimal porciones,
            DateTime fecha)
        {
            if (!TiposComida.Contains(tipoComida))
            {
                return (false, "Selecciona una comida del día.");
            }

            if (alimentoId <= 0)
            {
                return (false, "Selecciona un alimento.");
            }

            if (porciones <= 0 || porciones > 20)
            {
                return (false, "Las porciones deben ser mayores a 0 y no superar 20.");
            }

            if (fecha.Date > DateTime.Today)
            {
                return (false, "No se pueden registrar consumos de fechas futuras.");
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            var alimento = await context.Alimentos
                .FirstOrDefaultAsync(a => a.Id == alimentoId && a.Estado);

            if (alimento == null)
            {
                return (false, "El alimento seleccionado no está disponible.");
            }

            var consumo = new ConsumoAlimento
            {
                UsuarioId = usuarioId,
                AlimentoId = alimento.Id,
                TipoComida = tipoComida,
                Porciones = porciones,
                Fecha = fecha.Date,
                Calorias = Math.Round(alimento.Calorias * porciones, 2),
                Proteinas = Math.Round(alimento.Proteinas * porciones, 2),
                Carbohidratos = Math.Round(alimento.Carbohidratos * porciones, 2),
                Grasas = Math.Round(alimento.Grasas * porciones, 2),
                FechaRegistro = DateTime.Now
            };

            context.ConsumosAlimento.Add(consumo);

            await context.SaveChangesAsync();

            return (true, "Consumo registrado correctamente.");
        }

        public async Task<(bool Exito, string Mensaje)> EliminarAsync(int usuarioId, int consumoId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var consumo = await context.ConsumosAlimento
                .FirstOrDefaultAsync(c => c.Id == consumoId && c.UsuarioId == usuarioId);

            if (consumo == null)
            {
                return (false, "No se encontró el registro.");
            }

            context.ConsumosAlimento.Remove(consumo);

            await context.SaveChangesAsync();

            return (true, "Registro eliminado correctamente.");
        }

        public async Task<(bool Exito, string Mensaje)> ActualizarAsync(
            int usuarioId,
            int consumoId,
            string tipoComida,
            decimal porciones)
        {
            if (!TiposComida.Contains(tipoComida))
            {
                return (false, "Selecciona una comida del día.");
            }

            if (porciones <= 0 || porciones > 20)
            {
                return (false, "Las porciones deben ser mayores a 0 y no superar 20.");
            }

            await using var context = await _contextFactory.CreateDbContextAsync();

            var consumo = await context.ConsumosAlimento
                .Include(c => c.Alimento)
                .FirstOrDefaultAsync(c => c.Id == consumoId && c.UsuarioId == usuarioId);

            if (consumo == null)
            {
                return (false, "No se encontró el registro.");
            }

            consumo.TipoComida = tipoComida;
            consumo.Porciones = porciones;
            consumo.Calorias = Math.Round(consumo.Alimento.Calorias * porciones, 2);
            consumo.Proteinas = Math.Round(consumo.Alimento.Proteinas * porciones, 2);
            consumo.Carbohidratos = Math.Round(consumo.Alimento.Carbohidratos * porciones, 2);
            consumo.Grasas = Math.Round(consumo.Alimento.Grasas * porciones, 2);

            await context.SaveChangesAsync();

            return (true, "Registro actualizado correctamente.");
        }

        public async Task<List<ResumenNutricionalDia>> ResumenDiarioAsync(int usuarioId, DateTime fechaFin, int dias)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var fin = fechaFin.Date;
            var inicio = fin.AddDays(-(dias - 1));

            var totales = await context.ConsumosAlimento
                .Where(c => c.UsuarioId == usuarioId && c.Fecha >= inicio && c.Fecha <= fin)
                .GroupBy(c => c.Fecha)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Calorias = g.Sum(x => x.Calorias),
                    Proteinas = g.Sum(x => x.Proteinas),
                    Carbohidratos = g.Sum(x => x.Carbohidratos),
                    Grasas = g.Sum(x => x.Grasas),
                    Registros = g.Count()
                })
                .ToListAsync();

            var resumen = new List<ResumenNutricionalDia>();

            for (int i = 0; i < dias; i++)
            {
                var dia = inicio.AddDays(i);
                var total = totales.FirstOrDefault(x => x.Fecha == dia);

                resumen.Add(total == null
                    ? new ResumenNutricionalDia(dia, 0, 0, 0, 0, 0)
                    : new ResumenNutricionalDia(dia, total.Calorias, total.Proteinas, total.Carbohidratos, total.Grasas, total.Registros));
            }

            return resumen;
        }

        public async Task<List<ResumenDia>> ResumenSemanalAsync(int usuarioId, DateTime fechaFin)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var fin = fechaFin.Date;
            var inicio = fin.AddDays(-6);

            var totales = await context.ConsumosAlimento
                .Where(c => c.UsuarioId == usuarioId && c.Fecha >= inicio && c.Fecha <= fin)
                .GroupBy(c => c.Fecha)
                .Select(g => new { Fecha = g.Key, Calorias = g.Sum(x => x.Calorias) })
                .ToListAsync();

            var resumen = new List<ResumenDia>();

            for (int i = 0; i < 7; i++)
            {
                var dia = inicio.AddDays(i);
                var total = totales.FirstOrDefault(t => t.Fecha == dia);
                resumen.Add(new ResumenDia(dia, total?.Calorias ?? 0));
            }

            return resumen;
        }
    }
}
