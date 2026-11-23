using SmartNutriTracker_biblio.DTOs;
using SmartNutriTracker_biblio.Entities;

namespace SmartNutriTracker.Services;

public static class EstudianteMapper
{
    public static EstudianteDTO Mapear(PerfilEstudiante perfil)
    {
        var historial = perfil.Historiales
            .OrderBy(h => h.FechaRegistro)
            .Select(h => new ControlDTO
            {
                Id = h.Id,
                Fecha = DateOnly.FromDateTime(h.FechaRegistro),
                Peso = (double)h.Peso,
                Altura = (double)h.Altura,
                Imc = (double)h.Imc,
            })
            .ToList();

        return new EstudianteDTO
        {
            Id = perfil.Id,
            UsuarioId = perfil.UsuarioId,
            Nombre = perfil.Usuario?.Nombre ?? "",
            Correo = perfil.Usuario?.Correo ?? "",
            FechaNacimiento = perfil.FechaNacimiento,
            Objetivo = perfil.Objetivo,
            Altura = historial.LastOrDefault()?.Altura ?? 0,
            Historial = historial,
        };
    }
}