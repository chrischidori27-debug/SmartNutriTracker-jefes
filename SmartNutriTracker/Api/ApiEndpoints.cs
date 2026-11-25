using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;
using SmartNutriTracker.Servicios;

namespace SmartNutriTracker.Api
{
    public record AlimentoDto(
        int Id, string Nombre, string? Categoria, string Porcion,
        decimal Calorias, decimal Proteinas, decimal Carbohidratos, decimal Grasas, decimal Fibra);

    public static class ApiEndpoints
    {
        public static void MapApiEndpoints(this WebApplication app)
        {
            var api = app.MapGroup("/api");

            api.MapGet("/alimentos", async (IDbContextFactory<SmartNutriTrackerContext> factory) =>
                {
                    await using var context = await factory.CreateDbContextAsync();

                    var alimentos = await context.Alimentos
                        .AsNoTracking()
                        .Where(a => a.Estado)
                        .OrderBy(a => a.Nombre)
                        .Select(a => new AlimentoDto(
                            a.Id, a.Nombre, a.Categoria, a.Porcion,
                            a.Calorias, a.Proteinas, a.Carbohidratos, a.Grasas, a.Fibra))
                        .ToListAsync();

                    return Results.Ok(alimentos);
                })
                .WithName("ListarAlimentos")
                .WithSummary("Lista los alimentos activos con su información nutricional por porción.")
                .WithTags("Alimentos")
                .Produces<List<AlimentoDto>>();

            api.MapGet("/menus/{id:int}/nutricion", async (int id, NutricionServicio nutricion) =>
                {
                    var resumen = await nutricion.CalcularMenuAsync(id);

                    return resumen is null ? Results.NotFound() : Results.Ok(resumen);
                })
                .WithName("NutricionDeMenu")
                .WithSummary("Calcula calorías y macronutrientes de un menú diario (HU-26).")
                .WithTags("Nutrición")
                .Produces<ResumenNutricional>()
                .Produces(StatusCodes.Status404NotFound);

            api.MapGet("/imc", (decimal peso, decimal altura, ImcServicio imc) =>
                {
                    try
                    {
                        return Results.Ok(imc.Calcular(peso, altura));
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
                .WithName("CalcularImc")
                .WithSummary("Calcula el IMC (peso en kg, altura en metros) y devuelve la categoría y una recomendación de menú (HU-24).")
                .WithTags("Nutrición")
                .Produces<ResultadoImc>()
                .Produces(StatusCodes.Status400BadRequest);

            // HU-32: interfaz Swagger UI sobre el documento OpenAPI generado por la aplicación
            app.MapGet("/swagger", () => Results.Content(SwaggerHtml, "text/html; charset=utf-8"))
                .ExcludeFromDescription();
        }

        private const string SwaggerHtml = """
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <title>API | SmartNutriTracker</title>
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.14/swagger-ui.css" />
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.14/swagger-ui-bundle.js"></script>
  <script>
    window.onload = function () {
      SwaggerUIBundle({ url: '/openapi/v1.json', dom_id: '#swagger-ui' });
    };
  </script>
</body>
</html>
""";
    }
}
