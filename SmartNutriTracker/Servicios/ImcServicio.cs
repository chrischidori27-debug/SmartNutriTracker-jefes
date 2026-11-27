namespace SmartNutriTracker.Servicios
{
    // HU-24: modelo nutricional basado en el IMC
    public record ResultadoImc(decimal Imc, string Categoria, string Recomendacion);

    public record RecomendacionImc(
        decimal Imc,
        string NombreCategoria,
        string Resumen,
        int AjusteCaloricoPorcentaje,
        int ProteinasPorcentaje,
        int CarbohidratosPorcentaje,
        int GrasasPorcentaje,
        List<string> Consejos);

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

        // Método requerido por Imc.razor (HU-24)
        public RecomendacionImc Recomendar(decimal pesoKg, decimal talla)
        {
            var baseImc = Calcular(pesoKg, talla);

            if (baseImc.Imc < 18.5m)
            {
                return new RecomendacionImc(
                    baseImc.Imc,
                    baseImc.Categoria,
                    baseImc.Recomendacion,
                    AjusteCaloricoPorcentaje: 15,
                    ProteinasPorcentaje: 25,
                    CarbohidratosPorcentaje: 50,
                    GrasasPorcentaje: 25,
                    Consejos: new List<string>
                    {
                        "Realizar de 5 a 6 comidas nutritivas al día sin saltar el desayuno.",
                        "Incluir fuentes de proteína magra, lácteos, huevos y legumbres en cada tiempo de comida.",
                        "Añadir grasas saludables como palta, frutos secos y aceite de oliva."
                    });
            }

            if (baseImc.Imc < 25m)
            {
                return new RecomendacionImc(
                    baseImc.Imc,
                    baseImc.Categoria,
                    baseImc.Recomendacion,
                    AjusteCaloricoPorcentaje: 0,
                    ProteinasPorcentaje: 20,
                    CarbohidratosPorcentaje: 50,
                    GrasasPorcentaje: 30,
                    Consejos: new List<string>
                    {
                        "Mantener horarios regulares de alimentación e hidratación constante (1.5 a 2 L de agua).",
                        "Combinar cereales integrales, frutas frescas y verduras variadas en el almuerzo escolar.",
                        "Complementar la alimentación con actividad física moderada diaria."
                    });
            }

            if (baseImc.Imc < 30m)
            {
                return new RecomendacionImc(
                    baseImc.Imc,
                    baseImc.Categoria,
                    baseImc.Recomendacion,
                    AjusteCaloricoPorcentaje: -15,
                    ProteinasPorcentaje: 30,
                    CarbohidratosPorcentaje: 40,
                    GrasasPorcentaje: 30,
                    Consejos: new List<string>
                    {
                        "Priorizar verduras frescas y alimentos ricos en fibra para aumentar la saciedad.",
                        "Reducir bebidas azucaradas, golosinas y frituras en las meriendas.",
                        "Controlar el tamaño de las porciones de harinas refinadas."
                    });
            }

            return new RecomendacionImc(
                baseImc.Imc,
                baseImc.Categoria,
                baseImc.Recomendacion,
                AjusteCaloricoPorcentaje: -20,
                ProteinasPorcentaje: 30,
                CarbohidratosPorcentaje: 40,
                GrasasPorcentaje: 30,
                Consejos: new List<string>
                {
                    "Seguir un plan alimenticio estructurado con acompañamiento del nutricionista escolar.",
                    "Evitar alimentos ultraprocesados, comida rápida y azúcares añadidos.",
                    "Incorporar actividad física progresiva y registrar diariamente las comidas."
                });
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