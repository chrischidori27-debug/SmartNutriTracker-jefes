using SmartNutriTracker.Servicios;

namespace SmartNutriTracker.Api
{
    public static class RecomendacionesEndpoints
    {
        public static void MapRecomendacionesEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/api/recomendaciones")
                .WithTags("Recomendaciones Inteligentes (Equipo 5)");

            grupo.MapGet("/generar", async (
                decimal pesoKg,
                decimal altura,
                int edad,
                string sexo,
                ObjetivoNutricional? objetivo,
                RecomendacionServicio servicio) =>
            {
                try
                {
                    var plan = await servicio.GenerarRecomendacionAsync(
                        pesoKg, altura, edad, sexo, objetivo);
                    return Results.Ok(plan);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("GenerarRecomendacionMenu")
            .WithSummary("Genera un menú saludable personalizado según IMC, edad, sexo y objetivo (HU-23, HU-24, HU-25, HU-26).")
            .Produces<PlanRecomendacionDto>()
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}