namespace SmartNutriTracker_biblio.Helpers;

/// <summary>
/// Límites de validación en un solo lugar. La API y el cliente Blazor leen estos valores,
/// así que cambiar una regla aquí la cambia en los dos lados.
/// </summary>
public static class ReglasValidacion
{
    // Nombres y apellidos (la columna Usuarios.Nombre guarda "nombres apellidos", máximo 50)
    public const int NombreCompletoMax = 50;
    public const int NombreMinLetras = 2;

    // Correo
    public const int CorreoMax = 150;
    public const int CorreoParteLocalMax = 64;

    // Contraseña: coordinar con el Equipo 1 (dueño de las reglas de contraseña) antes de cambiarlas.
    public const int PasswordMin = 8;
    public const int PasswordMax = 100;

    // Fecha de nacimiento: rango de edad de un colegio
    public const int EdadMin = 5;
    public const int EdadMax = 25;

    // Peso (kg). La BD guarda DECIMAL(5,2), por eso máximo 2 decimales.
    public const double PesoMin = 15;
    public const double PesoMax = 300;
    public const int PesoDecimales = 2;

    // Altura (m). Se acepta en metros o en centímetros.
    public const double AlturaMin = 0.5;
    public const double AlturaMax = 2.5;
    public const int AlturaDecimales = 2;

    // Coherencia peso / altura
    public const double ImcMin = 10;
    public const double ImcMax = 60;

    // HU-10: cambio de peso que pide confirmación (kg)
    public const double CambioPesoSinConfirmar = 10;
}
