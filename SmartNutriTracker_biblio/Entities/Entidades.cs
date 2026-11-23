namespace SmartNutriTracker_biblio.Entities;

public class Role
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public List<Usuario> Usuarios { get; set; } = new();
}

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public int RolId { get; set; }
    public Role? Role { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; }
    public PerfilEstudiante? Perfil { get; set; }
}

public class LogAuditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Accion { get; set; } = "";
    public string? Descripcion { get; set; }
    public DateTime Fecha { get; set; }
}

public class PerfilEstudiante
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string Objetivo { get; set; } = "";
    public List<HistorialFisico> Historiales { get; set; } = new();
}

public class HistorialFisico
{
    public int Id { get; set; }
    public int PerfilId { get; set; }
    public PerfilEstudiante? Perfil { get; set; }
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
    public decimal Imc { get; set; }
    public DateTime FechaRegistro { get; set; }
}