using SmartNutriTracker.Api;
using SmartNutriTracker.Components;
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

// HU-24: recomendación nutricional según IMC
builder.Services.AddScoped<ImcServicio>();

// HU-32: documentación de la API con Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SmartNutriTracker API", Version = "v1" });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartNutriTracker API v1");
    c.RoutePrefix = "swagger";
});

app.UseAntiforgery();

app.MapStaticAssets();

app.MapApiEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
