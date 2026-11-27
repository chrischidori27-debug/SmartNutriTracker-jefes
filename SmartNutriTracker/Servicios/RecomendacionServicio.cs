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

    public record AlertaNutricionalDto(
        string Nivel, // "peligro", "advertencia", "info"
        string Titulo,
        string Mensaje);

    public record DistribucionComidaDto(
        string TiempoComida,
        int PorcentajeCalorias,
        decimal Calorias,
        decimal ProteinasGramos,
        decimal CarbohidratosGramos,
        decimal GrasasGramos,
        string GuiaPractica);

    public record AlimentoRecomendadoDto(
        int Id,
        string Nombre,
        string? Categoria,
        string TiempoComidaSugerido,
        string Porcion,
        decimal CantidadPorciones,
        decimal Calorias,
        decimal Proteinas,
        decimal Carbohidratos,
        decimal Grasas,
        decimal PorcentajeMetaCalorica);

    public record PlanRecomendacionDto(
        decimal Imc,
        string CategoriaImc,
        string MensajeImc,
        ObjetivoNutricional ObjetivoAplicado,
        decimal CaloriasObjetivoDiario,
        int PorcentajeProteinas,
        int PorcentajeCarbohidratos,
        int PorcentajeGrasas,
        decimal ProteinasObjetivoGramos,
        decimal CarbohidratosObjetivoGramos,
        decimal GrasasObjetivoGramos,
        decimal FibraObjetivoGramos,
        decimal AguaRecomendadaLitros,
        List<AlertaNutricionalDto> Alertas,
        List<DistribucionComidaDto> DistribucionComidas,
        List<string> RecomendacionesEspecificas,
        List<AlimentoRecomendadoDto> MenuSugerido,
        ResumenNutricional ResumenMenuSugerido);

    // HU-23, HU-24, HU-25 y HU-26: Motor avanzado de recomendaciones inteligentes
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
            // 1. HU-24: Cálculo y clasificación de IMC
            var resultadoImc = _imcServicio.Calcular(pesoKg, altura);
            decimal alturaMetros = altura > 3 ? altura / 100m : altura;

            // 2. HU-25: Determinar objetivo (si no lo elige el estudiante, se infiere del IMC)
            ObjetivoNutricional objetivo = objetivoUsuario ?? resultadoImc.Imc switch
            {
                < 18.5m => ObjetivoNutricional.GanarPeso,
                >= 25.0m => ObjetivoNutricional.BajarPeso,
                _ => ObjetivoNutricional.MantenerPeso
            };

            // 3. Generar Alertas de Riesgo y Coherencia
            var alertas = GenerarAlertas(resultadoImc.Imc, objetivo, objetivoUsuario.HasValue);

            // 4. HU-26: Cálculo automático de TMB y gasto energético (Mifflin-St Jeor)
            decimal tmb = (10m * pesoKg) + (6.25m * (alturaMetros * 100m)) - (5m * edad);
            tmb += sexo.Equals("F", StringComparison.OrdinalIgnoreCase) ? -161m : 5m;

            decimal gastoDiario = tmb * 1.45m;

            decimal ajusteCalorico = objetivo switch
            {
                ObjetivoNutricional.BajarPeso => resultadoImc.Imc >= 30m ? -500m : -350m,
                ObjetivoNutricional.GanarPeso => resultadoImc.Imc < 18.5m ? 450m : 350m,
                _ => 0m
            };

            decimal caloriasObjetivo = Math.Max(1350m, Math.Round(gastoDiario + ajusteCalorico, 0));

            // Porcentajes exactos de macronutrientes según IMC y objetivo
            (int pctProt, int pctCarb, int pctGrasa) = (objetivo, resultadoImc.Imc) switch
            {
                (ObjetivoNutricional.BajarPeso, >= 30m) => (35, 35, 30), // Mayor proteína para saciedad y proteger músculo
                (ObjetivoNutricional.BajarPeso, _) => (30, 40, 30),
                (ObjetivoNutricional.GanarPeso, < 18.5m) => (25, 50, 25),
                (ObjetivoNutricional.GanarPeso, _) => (25, 48, 27),
                _ => (20, 50, 30)
            };

            decimal protObjetivo = Math.Round((caloriasObjetivo * (pctProt / 100m)) / 4m, 1);
            decimal carbObjetivo = Math.Round((caloriasObjetivo * (pctCarb / 100m)) / 4m, 1);
            decimal grasaObjetivo = Math.Round((caloriasObjetivo * (pctGrasa / 100m)) / 9m, 1);
            decimal fibraObjetivo = objetivo == ObjetivoNutricional.BajarPeso ? 32m : 26m;
            decimal aguaLitros = Math.Round((pesoKg * 35m) / 1000m, 1);

            // 5. Distribución exacta por tiempos de comida (25% / 35% / 15% / 25%)
            var distribucion = CrearDistribucionComidas(
                caloriasObjetivo, protObjetivo, carbObjetivo, grasaObjetivo, objetivo);

            // 6. Recomendaciones prácticas específicas
            var recomendacionesEspecificas = GenerarRecomendacionesEspecificas(
                resultadoImc.Imc, objetivo, aguaLitros, protObjetivo);

            // 7. HU-23: Selección inteligente de alimentos activos desde SQL Server
            await using var context = await _contextFactory.CreateDbContextAsync();
            var alimentosActivos = await context.Alimentos
                .AsNoTracking()
                .Where(a => a.Estado && a.Calorias > 0)
                .ToListAsync();

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
                    .OrderByDescending(a => a.Proteinas)
                    .ThenBy(a => a.Calorias)
                    .ToList()
            };

            var menuSugerido = new List<AlimentoRecomendadoDto>();
            var resumenAcumulado = ResumenNutricional.Vacio;
            string[] tiempos = { "Desayuno (25%)", "Almuerzo (35%)", "Merienda (15%)", "Cena (25%)", "Colación Extra", "Complemento" };

            decimal factorPorcion = objetivo switch
            {
                ObjetivoNutricional.BajarPeso => 0.75m,
                ObjetivoNutricional.GanarPeso => 1.50m,
                _ => 1.00m
            };

            int idx = 0;
            foreach (var alimento in candidatos)
            {
                var aporte = _nutricionServicio.CalcularAlimento(alimento, factorPorcion);

                if (resumenAcumulado.Calorias + aporte.Calorias <= caloriasObjetivo * 1.05m)
                {
                    decimal pctMeta = caloriasObjetivo > 0
                        ? Math.Round((aporte.Calorias / caloriasObjetivo) * 100m, 1)
                        : 0m;

                    menuSugerido.Add(new AlimentoRecomendadoDto(
                        alimento.Id,
                        alimento.Nombre,
                        alimento.Categoria,
                        tiempos[Math.Min(idx, tiempos.Length - 1)],
                        alimento.Porcion,
                        factorPorcion,
                        aporte.Calorias,
                        aporte.Proteinas,
                        aporte.Carbohidratos,
                        aporte.Grasas,
                        pctMeta));

                    resumenAcumulado += aporte;
                    idx++;
                }

                if (resumenAcumulado.Calorias >= caloriasObjetivo * 0.90m || menuSugerido.Count >= 6)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "Recomendación avanzada generada (IMC: {Imc}, Alertas: {Alertas}, Objetivo: {Objetivo}).",
                resultadoImc.Imc, alertas.Count, objetivo);

            return new PlanRecomendacionDto(
                resultadoImc.Imc,
                resultadoImc.Categoria,
                resultadoImc.Recomendacion,
                objetivo,
                caloriasObjetivo,
                pctProt,
                pctCarb,
                pctGrasa,
                protObjetivo,
                carbObjetivo,
                grasaObjetivo,
                fibraObjetivo,
                aguaLitros,
                alertas,
                distribucion,
                recomendacionesEspecificas,
                menuSugerido,
                resumenAcumulado);
        }

        private static List<AlertaNutricionalDto> GenerarAlertas(
            decimal imc, ObjetivoNutricional objetivo, bool elegidoPorUsuario)
        {
            var alertas = new List<AlertaNutricionalDto>();

            // Alertas clínicas por rango de IMC
            if (imc >= 30.0m)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "peligro",
                    "ALERTA CRÍTICA: Riesgo Metabólico por Obesidad (IMC ≥ 30)",
                    "Tu IMC indica obesidad. Es urgente reducir el exceso calórico, eliminar bebidas azucaradas/ultraprocesados y acudir a valoración con el nutricionista escolar."));
            }
            else if (imc >= 25.0m)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "advertencia",
                    "ADVERTENCIA: Sobrepeso Detectado — Se recomienda bajar de peso",
                    "Tu IMC supera el rango saludable (24.9). Aplicar un déficit calórico moderado ahora previene resistencia a la insulina y fatiga escolar."));
            }
            else if (imc < 16.5m)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "peligro",
                    "ALERTA CRÍTICA: Delgadez Severa / Riesgo Nutricional",
                    "Tu IMC está muy por debajo del mínimo saludable. Necesitas aumentar tu ingesta calórica y proteica de inmediato bajo supervisión médica."));
            }
            else if (imc < 18.5m)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "advertencia",
                    "ADVERTENCIA: Bajo Peso — Se recomienda ganar masa saludable",
                    "Tu peso actual es inferior al recomendado para tu estatura. Prioriza comidas densas en nutrientes para fortalecer tu desarrollo físico y concentración."));
            }

            // Alertas de contradicción cuando el estudiante fuerza un objetivo riesgoso
            if (elegidoPorUsuario && imc >= 25.0m && objetivo == ObjetivoNutricional.GanarPeso)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "peligro",
                    "OBJETIVO PELIGROSO PARA TU IMC ACTUAL",
                    "Seleccionaste 'Ganar peso' teniendo sobrepeso u obesidad. Un superávit calórico agravará tu estado de salud; se sugiere cambiar a 'Bajar de peso'."));
            }
            else if (elegidoPorUsuario && imc < 18.5m && objetivo == ObjetivoNutricional.BajarPeso)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "peligro",
                    "OBJETIVO PELIGROSO: Déficit calórico con Bajo Peso",
                    "Seleccionaste 'Bajar de peso' estando por debajo de tu peso saludable. Esto puede provocar pérdida muscular, anemia y bajo rendimiento académico."));
            }

            if (alertas.Count == 0)
            {
                alertas.Add(new AlertaNutricionalDto(
                    "info",
                    "Estado Nutricional Óptimo",
                    "Tu IMC se encuentra en rango saludable y tu objetivo es seguro. Mantén la constancia en tus horarios y porciones."));
            }

            return alertas;
        }

        private static List<DistribucionComidaDto> CrearDistribucionComidas(
            decimal kcal, decimal prot, decimal carb, decimal grasa, ObjetivoNutricional objetivo)
        {
            var tiempos = new (string Nombre, int Pct, string Guia)[]
            {
                ("Desayuno", 25, objetivo == ObjetivoNutricional.BajarPeso
                    ? "Proteína magra (huevo/yogur griego) + fruta entera + avena moderada sin azúcar."
                    : "Avena o pan integral + huevos/lácteos + fruta energética para iniciar clases."),
                ("Almuerzo", 35, objetivo == ObjetivoNutricional.BajarPeso
                    ? "50% del plato vegetales verdes, 30% proteína a la plancha, 20% carbohidrato complejo."
                    : "40% carbohidratos (arroz integral, papa, quinua), 35% proteína, 25% ensalada y palta."),
                ("Merienda Escolar", 15, objetivo == ObjetivoNutricional.BajarPeso
                    ? "Fruta rica en fibra (manzana/pera) o un puñado pequeño de almendras; cero frituras."
                    : "Sándwich integral con proteína, yogur natural o frutos secos con plátano."),
                ("Cena", 25, objetivo == ObjetivoNutricional.BajarPeso
                    ? "Cena ligera y temprana: proteína magra + verduras cocidas o salteadas; evitar harinas."
                    : "Proteína de fácil digestión + porción moderada de carbohidrato y vegetales.")
            };

            return tiempos.Select(t =>
            {
                decimal factor = t.Pct / 100m;
                return new DistribucionComidaDto(
                    t.Nombre,
                    t.Pct,
                    Math.Round(kcal * factor, 0),
                    Math.Round(prot * factor, 1),
                    Math.Round(carb * factor, 1),
                    Math.Round(grasa * factor, 1),
                    t.Guia);
            }).ToList();
        }

        private static List<string> GenerarRecomendacionesEspecificas(
            decimal imc, ObjetivoNutricional objetivo, decimal aguaLitros, decimal protObjetivo)
        {
            var lista = new List<string>
            {
                $"Hidratación obligatoria: bebe al menos {aguaLitros:0.0} litros de agua pura al día (1 vaso antes de cada comida principal).",
                $"Meta proteica diaria: asegura {protObjetivo:0.0} g de proteína repartidos en tus 4 comidas para mantener saciedad y masa muscular."
            };

            if (objetivo == ObjetivoNutricional.BajarPeso || imc >= 25m)
            {
                lista.Add("Regla del plato (50-30-20): llena la mitad de tu plato con verduras antes de servir arroz, pasta o pan.");
                lista.Add("Elimina calorías líquidas: sustituye gaseosas y jugos procesados por agua o infusiones sin azúcar (ahorra 250-400 kcal/día).");
                lista.Add("Evita ayunos prolongados en la mañana para no llegar con ansiedad calórica a la cena.");
            }
            else if (objetivo == ObjetivoNutricional.GanarPeso || imc < 18.5m)
            {
                lista.Add("Densidad nutricional: agrega grasas saludables (palta, aceite de oliva, semillas o maní) a tus comidas principales.");
                lista.Add("No saltes la merienda de media tarde; es clave para alcanzar tu superávit calórico sin sentir pesadez.");
                lista.Add("Combina cereales con legumbres (ej. arroz con lentejas) para obtener aminoácidos completos.");
            }
            else
            {
                lista.Add("Mantén horarios fijos de alimentación con intervalos de 3.5 a 4 horas entre comidas.");
                lista.Add("Prioriza alimentos naturales de temporada y limita ultraprocesados a máximo 1 ocasión por semana.");
            }

            return lista;
        }
    }
}