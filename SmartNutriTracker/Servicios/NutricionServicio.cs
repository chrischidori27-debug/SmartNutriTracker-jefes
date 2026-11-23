using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    // HU-26: resumen nutricional calculado automáticamente
    public record ResumenNutricional(
        decimal Calorias,
        decimal Proteinas,
        decimal Carbohidratos,
        decimal Grasas,
        decimal Fibra)
    {
        public static ResumenNutricional Vacio => new(0, 0, 0, 0, 0);

        public static ResumenNutricional operator +(ResumenNutricional a, ResumenNutricional b) =>
            new(a.Calorias + b.Calorias,
                a.Proteinas + b.Proteinas,
                a.Carbohidratos + b.Carbohidratos,
                a.Grasas + b.Grasas,
                a.Fibra + b.Fibra);

        // Calorías aportadas por macros (4 kcal/g proteína y carbohidrato, 9 kcal/g grasa)
        private decimal CaloriasMacros => Proteinas * 4 + Carbohidratos * 4 + Grasas * 9;

        public decimal PorcentajeProteinas => Porcentaje(Proteinas * 4);
        public decimal PorcentajeCarbohidratos => Porcentaje(Carbohidratos * 4);
        public decimal PorcentajeGrasas => Porcentaje(Grasas * 9);

        private decimal Porcentaje(decimal kcal) =>
            CaloriasMacros == 0 ? 0 : Math.Round(kcal * 100 / CaloriasMacros, 1);
    }

    public class NutricionServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;
        private readonly ILogger<NutricionServicio> _logger;

        public NutricionServicio(
            IDbContextFactory<SmartNutriTrackerContext> contextFactory,
            ILogger<NutricionServicio> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        // Los valores del alimento son por porción; "cantidad" es el número de porciones.
        public ResumenNutricional CalcularAlimento(Alimento alimento, decimal cantidad)
        {
            ArgumentNullException.ThrowIfNull(alimento);

            if (cantidad < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad), "La cantidad no puede ser negativa.");
            }

            return new ResumenNutricional(
                Math.Round(alimento.Calorias * cantidad, 2),
                Math.Round(alimento.Proteinas * cantidad, 2),
                Math.Round(alimento.Carbohidratos * cantidad, 2),
                Math.Round(alimento.Grasas * cantidad, 2),
                Math.Round(alimento.Fibra * cantidad, 2));
        }

        public ResumenNutricional CalcularItems(IEnumerable<MenuAlimento> items)
        {
            var total = ResumenNutricional.Vacio;

            foreach (var item in items)
            {
                total += CalcularAlimento(item.Alimento, item.Cantidad);
            }

            return total;
        }

        // Calcula calorías y macros de un menú. Devuelve null si el menú no existe.
        public async Task<ResumenNutricional?> CalcularMenuAsync(int menuId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.MenuAlimentos
                .AsNoTracking()
                .Include(m => m.Alimento)
                .Where(m => m.MenuId == menuId)
                .ToListAsync();

            if (items.Count == 0)
            {
                bool existe = await context.MenusDiarios.AnyAsync(m => m.Id == menuId);

                if (!existe)
                {
                    _logger.LogWarning("Cálculo nutricional: el menú {MenuId} no existe.", menuId);
                    return null;
                }
            }

            var resumen = CalcularItems(items);

            _logger.LogInformation(
                "Cálculo nutricional del menú {MenuId}: {Calorias} kcal.", menuId, resumen.Calorias);

            return resumen;
        }

        // Suma todos los menús de un usuario en una fecha.
        public async Task<ResumenNutricional> CalcularDiaAsync(int usuarioId, DateOnly fecha)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.MenuAlimentos
                .AsNoTracking()
                .Include(m => m.Alimento)
                .Where(m => m.Menu.UsuarioId == usuarioId && m.Menu.Fecha == fecha)
                .ToListAsync();

            return CalcularItems(items);
        }
    }
}
