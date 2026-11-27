using SmartNutriTracker.Components;
using SmartNutriTracker.Api;
using SmartNutriTracker.Logging;
using SmartNutriTracker.Modelos;
using SmartNutriTracker.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// HU-29: logger global a archivo (carpeta logs/)
builder.Logging.AddProvider(
    new ArchivoLoggerProvider(Path.Combine(builder.Environment.ContentRootPath, "logs")));

// Servicios de Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Conexion con SQL Server
builder.Services.AddDbContextFactory<SmartNutriTrackerContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexionSQL")
    )
);

// Servicio para proteger y verificar contrasenas
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Servicio de usuarios
builder.Services.AddScoped<UsuarioServicio>();

// HU-29: auditoría de acciones
builder.Services.AddScoped<AuditoriaServicio>();

// HU-26: cálculo nutricional automático
builder.Services.AddScoped<NutricionServicio>();

// HU-24: IMC y recomendación base
builder.Services.AddScoped<ImcServicio>();

// Equipo 5 (HU-23 a HU-26): motor de recomendaciones inteligentes
builder.Services.AddScoped<RecomendacionServicio>();

// HU-32: documento OpenAPI (Swagger)
builder.Services.AddOpenApi();

var app = builder.Build();

// Crea la base de datos/tablas si no existen y asegura los roles base.
// Sin estos roles el registro de usuarios no funciona.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<SmartNutriTrackerContext>>();

        await using var context = await factory.CreateDbContextAsync();

        await context.Database.EnsureCreatedAsync();

        var roles = new (string Nombre, string Descripcion)[]
        {
            ("Administrador", "Control general del sistema."),
            ("Docente", "Seguimiento de estudiantes."),
            ("Estudiante", "Consulta y registro de información."),
            ("Nutricionista", "Gestión nutricional y alimentos.")
        };

        foreach (var (nombre, descripcion) in roles)
        {
            if (!await context.Roles.AnyAsync(r => r.Nombre == nombre))
            {
                context.Roles.Add(new Role { Nombre = nombre, Descripcion = descripcion });
            }
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo inicializar la base de datos. Revisa la cadena de conexión.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();

// HU-32: /openapi/v1.json y /swagger
app.MapOpenApi();
app.MapApiEndpoints();

// Equipo 5 (HU-23 a HU-26): endpoints de recomendaciones inteligentes
app.MapRecomendacionesEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();