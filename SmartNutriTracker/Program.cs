using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Data;
using SmartNutriTracker.Services;
using Microsoft.AspNetCore.Mvc;
using SmartNutriTracker_biblio.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS: permite que el cliente Blazor (otro puerto) llame a esta API
builder.Services.AddCors(options =>
{
    options.AddPolicy("Blazor", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Errores de formato (fecha inválida, número mal escrito...) con mensaje claro y el campo afectado
        opciones.InvalidModelStateResponseFactory = contexto =>
            new BadRequestObjectResult(TraductorErroresModelo.Traducir(contexto.ModelState));
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Blazor");
app.UseAuthorization();
app.MapControllers();

app.Run();