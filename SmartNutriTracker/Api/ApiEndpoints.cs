using SmartNutriTracker.Servicios;

namespace SmartNutriTracker.Api
{
    // HU-32: endpoints documentados con Swagger (/swagger)
    public static class ApiEndpoints
    {
        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/api");

            // HU-24: recomendación según IMC
            api.MapGet("/imc", (decimal peso, decimal talla, ImcServicio imc) =>
                {
                    try
                    {
                        return Results.Ok(imc.Recomendar(peso, talla));
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message.Split('(')[0].Trim() });
                    }
                })
                .WithTags("Nutrición")
                .WithName("RecomendacionPorImc")
                .WithSummary("Calcula el IMC y devuelve una recomendación nutricional.")
                .WithDescription("Parámetros: peso en kilogramos y talla en centímetros.");

            // HU-26: cálculo nutricional de un menú
            api.MapGet("/menus/{id:int}/nutricion", async (int id, NutricionServicio nutricion) =>
                {
                    var resumen = await nutricion.CalcularMenuAsync(id);
                    return resumen is null ? Results.NotFound() : Results.Ok(resumen);
                })
                .WithTags("Nutrición")
                .WithName("NutricionDeMenu")
                .WithSummary("Calorías y macronutrientes de un menú diario.");

            return app;
        }
    }
}
