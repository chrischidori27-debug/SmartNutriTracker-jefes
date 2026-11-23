using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmartNutriTracker_biblio.DTOs;

namespace SmartNutriTracker.Services;

// Convierte los errores automáticos de [ApiController] (p. ej. fecha "2010-13-45") en un ErrorDTO
// con un mensaje claro, en lugar de un "Error 400" genérico.
public static class TraductorErroresModelo
{
    private static readonly Dictionary<string, string> Mensajes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nombres"] = "Ingresa los nombres.",
        ["apellidos"] = "Ingresa los apellidos.",
        ["correo"] = "Ingresa un correo válido (ej. alumno@innovare.edu).",
        ["password"] = "Ingresa una contraseña.",
        ["fechaNacimiento"] = "La fecha de nacimiento no es válida. Usa el formato AAAA-MM-DD (ej. 2010-05-21).",
        ["objetivo"] = "Selecciona un objetivo válido.",
        ["peso"] = "El peso debe ser un número válido (ej. 68.5).",
        ["altura"] = "La altura debe ser un número válido (ej. 1.75).",
    };

    public static ErrorDTO Traducir(ModelStateDictionary estado)
    {
        foreach (var (clave, entrada) in estado)
        {
            if (entrada is null || entrada.Errors.Count == 0) continue;

            // Las claves llegan como "$.fechaNacimiento" (JSON) o "FechaNacimiento" (propiedad)
            var nombre = clave.Split('.').Last();
            var campo = Mensajes.Keys.FirstOrDefault(k => k.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (campo is not null) return new ErrorDTO(campo, Mensajes[campo]);
        }

        return new ErrorDTO(null, "Los datos enviados no son válidos. Revisa el formulario e inténtalo de nuevo.");
    }
}
