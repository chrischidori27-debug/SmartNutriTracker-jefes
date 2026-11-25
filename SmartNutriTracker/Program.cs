using SmartNutriTracker.Components;
using SmartNutriTracker.Modelos;
using SmartNutriTracker.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<UsuarioSesion>();
builder.Services.AddScoped<AlimentoServicio>();
builder.Services.AddScoped<MenuServicio>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
