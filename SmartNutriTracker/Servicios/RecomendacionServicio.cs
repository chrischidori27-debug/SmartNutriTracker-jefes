using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public enum ObjetivoNutricional
    {
        BajarPeso,
        MantenerPeso,
        GanarPeso
    }

    public record AlimentoRecomendadoDto(
        int Id,
        string Nombre,
        string? Categoria,
        string Porcion,
        decimal CantidadPorciones,
        decimal Calorias,
        decimal Proteinas,
        decimal Carbohidratos,
        decimal Grasas);

    public record PlanRecomendacionDto(
        decimal Imc,
        string CategoriaImc,
        string MensajeImc,
        ObjetivoNutricional ObjetivoAplicado,
        decimal CaloriasObjetivoDiario,
        decimal ProteinasObjetivoGramos,
        decimal CarbohidratosObjetivoGramos,
        decimal GrasasObjetivoGramos,
        List<AlimentoRecomendadoDto> MenuSugerido,
        ResumenNutricional ResumenMenuSugerido);

    // HU-23, HU-24, HU-25 y HU-26: Motor de recomendaciones inteligentes
    public class RecomendacionServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;
        private readonly ImcServicio _imcServicio;
        private readonly NutricionServicio _nutricionServicio;
        private readonly ILogger<RecomendacionServicio> _logger;

        public RecomendacionServicio(
            IDbContextFactory<SmartNutriTrackerContext> contextFactory,
            ImcServicio imcServicio,
            NutricionServicio nutricionServicio,
            ILogger<RecomendacionServicio> logger)
        {
            _contextFactory = contextFactory;
            _imcServicio = imcServicio;
            _nutricionServicio = nutricionServicio;
            _logger = logger;
        }

        public async Task<PlanRecomendacionDto> GenerarRecomendacionAsync(
            decimal pesoKg,
            decimal altura,
            int edad,
            string sexo,
            ObjetivoNutricional? objetivoUsuario = null)
        {
            // 1. HU-24: Cálculo y clasificación de IMC reutilizando ImcServicio
            var resultadoImc = _imcServicio.Calcular(pesoKg, altura);
            decimal alturaMetros = altura > 3 ? altura / 100m : altura;

            // 2. HU-25: Determinar objetivo (si no lo elige el estudiante, se infiere del IMC)
            ObjetivoNutricional objetivo = objetivoUsuario ?? resultadoImc.Imc switch
            {
                < 18.5m => ObjetivoNutricional.GanarPeso,
                >= 25.0m => ObjetivoNutricional.BajarPeso,
                _ => ObjetivoNutricional.MantenerPeso
            };

            // 3. HU-26: Cálculo automático de requerimientos calóricos (Mifflin-St Jeor + factor escolar)
            decimal tmb = (10m * pesoKg) + (6.25m * (alturaMetros * 100m)) - (5m * edad);
            tmb += sexo.Equals("F", StringComparison.OrdinalIgnoreCase) ? -161m : 5m;

            decimal gastoDiario = tmb * 1.45m; // Factor actividad física escolar promedio

            decimal caloriasObjetivo = objetivo switch
            {
                ObjetivoNutricional.BajarPeso => Math.Max(1400m, Math.Round(gastoDiario - 400m, 0)),
                ObjetivoNutricional.GanarPeso => Math.Round(gastoDiario + 400m, 0),
                _ => Math.Round(gastoDiario, 0)
            };

            // Distribución de macros según objetivo (HU-25 + HU-26)
            (decimal pctProt, decimal pctCarb, decimal pctGrasa) = objetivo switch
            {
                ObjetivoNutricional.BajarPeso => (0.30m, 0.40m, 0.30m),
                ObjetivoNutricional.GanarPeso => (0.25m, 0.50m, 0.25m),
                _ => (0.20m, 0.50m, 0.30m)
            };

            decimal protObjetivo = Math.Round((caloriasObjetivo * pctProt) / 4m, 1);
            decimal carbObjetivo = Math.Round((caloriasObjetivo * pctCarb) / 4m, 1);
            decimal grasaObjetivo = Math.Round((caloriasObjetivo * pctGrasa) / 9m, 1);

            // 4. HU-23: Selección inteligente de alimentos activos desde SQL Server
            await using var context = await _contextFactory.CreateDbContextAsync();
            var alimentosActivos = await context.Alimentos
                .AsNoTracking()
                .Where(a => a.Estado && a.Calorias > 0)
                .ToListAsync();

            // Ordenar priorizando alimentos acordes al objetivo
            var candidatos = objetivo switch
            {
                ObjetivoNutricional.BajarPeso => alimentosActivos
                    .OrderByDescending(a => a.Proteinas + a.Fibra)
                    .ThenBy(a => a.Calorias)
                    .ToList(),
                ObjetivoNutricional.GanarPeso => alimentosActivos
                    .OrderByDescending(a => a.Calorias)
                    .ThenByDescending(a => a.Proteinas)
                    .ToList(),
                _ => alimentosActivos
                    .OrderBy(a => Guid.NewGuid()) // Variedad equilibrada
                    .ToList()
            };

            var menuSugerido = new List<AlimentoRecomendadoDto>();
            var resumenAcumulado = ResumenNutricional.Vacio;

            // Ajustar factor de porción según objetivo (0.75 porción para bajar, 1.5 para ganar, 1.0 mantener)
            decimal factorPorcion = objetivo switch
            {
                ObjetivoNutricional.BajarPeso => 0.75m,
                ObjetivoNutricional.GanarPeso => 1.50m,
                _ => 1.00m
            };

            foreach (var alimento in candidatos)
            {
                var aporte = _nutricionServicio.CalcularAlimento(alimento, factorPorcion);

                if (resumenAcumulado.Calorias + aporte.Calorias <= caloriasObjetivo * 1.05m)
                {
                    menuSugerido.Add(new AlimentoRecomendadoDto(
                        alimento.Id,
                        alimento.Nombre,
                        alimento.Categoria,
                        alimento.Porcion,
                        factorPorcion,
                        aporte.Calorias,
                        aporte.Proteinas,
                        aporte.Carbohidratos,
                        aporte.Grasas));

                    resumenAcumulado += aporte;
                }

                if (resumenAcumulado.Calorias >= caloriasObjetivo * 0.90m || menuSugerido.Count >= 6)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "Recomendación generada (IMC: {Imc}, Objetivo: {Objetivo}, Meta: {Meta} kcal, Sugerido: {Sugerido} kcal).",
                resultadoImc.Imc, objetivo, caloriasObjetivo, resumenAcumulado.Calorias);

            return new PlanRecomendacionDto(
                resultadoImc.Imc,
                resultadoImc.Categoria,
                resultadoImc.Recomendacion,
                objetivo,
                caloriasObjetivo,
                protObjetivo,
                carbObjetivo,
                grasaObjetivo,
                menuSugerido,
                resumenAcumulado);
        }
    }
}