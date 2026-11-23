namespace SmartNutriTracker_biblio.DTOs;

public class ControlDTO
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public double Peso { get; set; }
    public double Altura { get; set; }
    public double Imc { get; set; }
}

public class EstudianteDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public DateOnly FechaNacimiento { get; set; }
    public string Objetivo { get; set; } = "";
    public double Altura { get; set; }
    public List<ControlDTO> Historial { get; set; } = new();
}

public class RegistroEstudianteDTO
{
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Password { get; set; } = "";
    public DateOnly FechaNacimiento { get; set; }
    public string Objetivo { get; set; } = "";
    public double Peso { get; set; }
    public double Altura { get; set; }
}

public class ActualizarControlDTO
{
    public double Peso { get; set; }
    public string Objetivo { get; set; } = "";
    // HU-10: true cuando la persona ya confirmó un cambio de peso mayor a 10 kg
    public bool ConfirmarCambioGrande { get; set; }
}

public record ErrorDTO(string? Campo, string Mensaje);

public class SemanaDTO
{
    public DateOnly Inicio { get; set; }
    public DateOnly Fin { get; set; }
    public int Controles { get; set; }
    public double PesoPromedio { get; set; }
    public double PesoFinal { get; set; }
    public double ImcFinal { get; set; }
    public double? Variacion { get; set; }
}

public class ReporteSemanalDTO
{
    public int EstudianteId { get; set; }
    public string Nombre { get; set; } = "";
    public string Objetivo { get; set; } = "";
    public List<SemanaDTO> Semanas { get; set; } = new();
}