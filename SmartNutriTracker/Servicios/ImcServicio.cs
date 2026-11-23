namespace SmartNutriTracker.Servicios
{
    // HU-24: recomendación nutricional basada en el IMC

    public enum CategoriaImc
    {
        BajoPeso,
        PesoNormal,
        Sobrepeso,
        Obesidad
    }

    public record RecomendacionImc(
        decimal Imc,
        CategoriaImc Categoria,
        string NombreCategoria,
        string Resumen,
        int AjusteCaloricoPorcentaje,
        int ProteinasPorcentaje,
        int CarbohidratosPorcentaje,
        int GrasasPorcentaje,
        IReadOnlyList<string> Consejos);

    public record EvaluacionMenu(bool Adecuado, string Mensaje);

    public class ImcServicio
    {
        private readonly ILogger<ImcServicio> _logger;

        public ImcServicio(ILogger<ImcServicio> logger)
        {
            _logger = logger;
        }

        // IMC = peso (kg) / talla (m)^2
        public decimal CalcularImc(decimal pesoKg, decimal tallaCm)
        {
            if (pesoKg <= 0 || pesoKg > 500)
                throw new ArgumentOutOfRangeException(nameof(pesoKg), "El peso debe estar entre 1 y 500 kg.");

            if (tallaCm < 50 || tallaCm > 260)
                throw new ArgumentOutOfRangeException(nameof(tallaCm), "La talla debe estar entre 50 y 260 cm.");

            decimal tallaM = tallaCm / 100m;
            return Math.Round(pesoKg / (tallaM * tallaM), 1);
        }

        // Clasificación de la OMS para adultos
        public CategoriaImc Clasificar(decimal imc) => imc switch
        {
            < 18.5m => CategoriaImc.BajoPeso,
            < 25m => CategoriaImc.PesoNormal,
            < 30m => CategoriaImc.Sobrepeso,
            _ => CategoriaImc.Obesidad
        };

        public RecomendacionImc Recomendar(decimal pesoKg, decimal tallaCm)
        {
            decimal imc = CalcularImc(pesoKg, tallaCm);
            var categoria = Clasificar(imc);

            _logger.LogInformation("Recomendación por IMC: IMC {Imc}, categoría {Categoria}.", imc, categoria);

            return categoria switch
            {
                CategoriaImc.BajoPeso => new RecomendacionImc(
                    imc, categoria, "Bajo peso",
                    "Se recomienda un menú con superávit calórico moderado y alta densidad nutricional.",
                    15, 20, 55, 25,
                    new[]
                    {
                        "Aumenta las porciones de forma gradual.",
                        "Incluye frutos secos, legumbres y lácteos.",
                        "Haz 5 comidas al día, con colaciones saludables."
                    }),

                CategoriaImc.PesoNormal => new RecomendacionImc(
                    imc, categoria, "Peso normal",
                    "Se recomienda un menú equilibrado que mantenga las calorías actuales.",
                    0, 20, 55, 25,
                    new[]
                    {
                        "Mantén una dieta variada con frutas y verduras.",
                        "Bebe suficiente agua durante el día.",
                        "Limita los productos ultraprocesados."
                    }),

                CategoriaImc.Sobrepeso => new RecomendacionImc(
                    imc, categoria, "Sobrepeso",
                    "Se recomienda un menú con déficit calórico leve y más proteína y fibra.",
                    -10, 25, 50, 25,
                    new[]
                    {
                        "Prioriza verduras, legumbres y cereales integrales.",
                        "Reduce bebidas azucaradas y frituras.",
                        "Combina la alimentación con actividad física regular."
                    }),

                _ => new RecomendacionImc(
                    imc, categoria, "Obesidad",
                    "Se recomienda un menú con déficit calórico moderado, supervisado por un nutricionista.",
                    -15, 25, 45, 30,
                    new[]
                    {
                        "Consulta con un nutricionista para un plan personalizado.",
                        "Elige alimentos ricos en fibra y proteína magra.",
                        "Evita azúcares añadidos y grasas saturadas."
                    })
            };
        }

        // Compara la distribución de macros de un menú con la recomendada (tolerancia de ±10 puntos).
        public EvaluacionMenu EvaluarMenu(ResumenNutricional menu, RecomendacionImc recomendacion)
        {
            ArgumentNullException.ThrowIfNull(menu);
            ArgumentNullException.ThrowIfNull(recomendacion);

            const decimal tolerancia = 10m;

            if (menu.Calorias <= 0)
                return new EvaluacionMenu(false, "El menú no tiene información nutricional.");

            var desvios = new List<string>();

            if (Math.Abs(menu.PorcentajeProteinas - recomendacion.ProteinasPorcentaje) > tolerancia)
                desvios.Add("proteínas");
            if (Math.Abs(menu.PorcentajeCarbohidratos - recomendacion.CarbohidratosPorcentaje) > tolerancia)
                desvios.Add("carbohidratos");
            if (Math.Abs(menu.PorcentajeGrasas - recomendacion.GrasasPorcentaje) > tolerancia)
                desvios.Add("grasas");

            return desvios.Count == 0
                ? new EvaluacionMenu(true, $"El menú es adecuado para el perfil ({recomendacion.NombreCategoria}).")
                : new EvaluacionMenu(false, $"El menú se aleja de lo recomendado en: {string.Join(", ", desvios)}.");
        }
    }
}
