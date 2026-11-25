namespace SmartNutriTracker.Servicios
{
    // HU-24: modelo nutricional basado en el IMC
    public record ResultadoImc(decimal Imc, string Categoria, string Recomendacion);

    public class ImcServicio
    {
        private readonly ILogger<ImcServicio> _logger;

        public ImcServicio(ILogger<ImcServicio> logger)
        {
            _logger = logger;
        }

        // pesoKg en kilogramos; altura en metros (si es mayor a 3 se interpreta como centímetros).
        public ResultadoImc Calcular(decimal pesoKg, decimal altura)
        {
            if (altura > 3)
            {
                altura /= 100;
            }

            if (pesoKg < 10 || pesoKg > 300)
            {
                throw new ArgumentOutOfRangeException(nameof(pesoKg), "El peso debe estar entre 10 y 300 kg.");
            }

            if (altura < 0.5m || altura > 2.5m)
            {
                throw new ArgumentOutOfRangeException(nameof(altura), "La altura debe estar entre 0.5 y 2.5 metros.");
            }

            decimal imc = Math.Round(pesoKg / (altura * altura), 1);

            var (categoria, recomendacion) = Clasificar(imc);

            _logger.LogInformation("IMC calculado: {Imc} ({Categoria}).", imc, categoria);

            return new ResultadoImc(imc, categoria, recomendacion);
        }

        // Categorías de la OMS y orientación general para armar el menú
        public static (string Categoria, string Recomendacion) Clasificar(decimal imc)
        {
            if (imc < 18.5m)
            {
                return ("Bajo peso",
                    "Menú con mayor aporte calórico y proteico: priorizar legumbres, lácteos, frutos secos y carbohidratos complejos.");
            }

            if (imc < 25m)
            {
                return ("Peso normal",
                    "Menú equilibrado para mantener el peso: variedad de frutas, verduras, proteínas magras y cereales integrales.");
            }

            if (imc < 30m)
            {
                return ("Sobrepeso",
                    "Menú con menor densidad calórica: más verduras y fibra, reducir azúcares y grasas saturadas.");
            }

            return ("Obesidad",
                "Menú hipocalórico supervisado por un nutricionista: controlar porciones y evitar ultraprocesados.");
        }
    }
}
