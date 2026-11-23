using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Data;
using SmartNutriTracker.Services;
using SmartNutriTracker_biblio.DTOs;
using SmartNutriTracker_biblio.Entities;
using SmartNutriTracker_biblio.Helpers;

namespace SmartNutriTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PerfilesController : ControllerBase
{
    private const string RolEstudiante = "Estudiante";

    private readonly ApplicationDbContext _context;

    public PerfilesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<PerfilEstudiante> Perfiles() =>
        _context.PerfilesEstudiante.Include(p => p.Usuario).Include(p => p.Historiales);

    // Panel, Seguimiento y Perfil: todos los estudiantes con su historial
    [HttpGet]
    public async Task<ActionResult<List<EstudianteDTO>>> GetPerfiles()
    {
        var perfiles = await Perfiles().AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        return perfiles.Select(EstudianteMapper.Mapear).ToList();
    }

    // HU-09: ver perfil de un estudiante
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstudianteDTO>> GetPerfil(int id)
    {
        var perfil = await Perfiles().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (perfil is null) return NotFound(new ErrorDTO(null, "No se encontró al estudiante."));
        return EstudianteMapper.Mapear(perfil);
    }

    // HU-08: registrar estudiante (usuario + perfil + primer control)
    [HttpPost]
    public async Task<ActionResult<EstudianteDTO>> PostPerfil([FromBody] RegistroEstudianteDTO r)
    {
        var error = ValidarRegistro(r);
        if (error is not null) return BadRequest(error);

        var nombre = $"{ValidadorCampos.NormalizarTexto(r.Nombres)} {ValidadorCampos.NormalizarTexto(r.Apellidos)}";
        var correo = r.Correo.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
            return Conflict(new ErrorDTO("correo", "Ya existe un estudiante con este correo."));

        var rol = await _context.Roles.FirstOrDefaultAsync(x => x.Nombre == RolEstudiante);
        if (rol is null)
            return Problem($"No existe el rol '{RolEstudiante}' en la base de datos.");

        var ahora = DateTime.Now;
        var altura = CalculosNutricionales.NormalizarAltura(r.Altura);

        var usuario = new Usuario
        {
            Nombre = nombre,
            Correo = correo,
            PasswordHash = PasswordHasher.Hash(r.Password),
            RolId = rol.Id,
            Estado = true,
            FechaRegistro = ahora,
        };

        var perfil = new PerfilEstudiante
        {
            Usuario = usuario,
            FechaNacimiento = r.FechaNacimiento,
            Objetivo = r.Objetivo,
        };

        perfil.Historiales.Add(new HistorialFisico
        {
            Peso = Math.Round((decimal)r.Peso, 2),
            Altura = (decimal)altura,
            Imc = (decimal)CalculosNutricionales.CalcularImc(r.Peso, altura),
            FechaRegistro = ahora,
        });

        _context.PerfilesEstudiante.Add(perfil);
        _context.LogsAuditoria.Add(new LogAuditoria
        {
            Usuario = usuario,
            Accion = "REGISTRO_ESTUDIANTE",
            Descripcion = $"Se registró al estudiante {correo}.",
            Fecha = ahora,
        });
        await _context.SaveChangesAsync();

        return Ok(EstudianteMapper.Mapear(perfil));
    }

    // HU-10: actualizar objetivo y registrar el control de hoy (si ya hay uno de hoy, se reemplaza)
    [HttpPost("{id:int}/controles")]
    public async Task<ActionResult<EstudianteDTO>> PostControl(int id, [FromBody] ActualizarControlDTO r)
    {
        var errorPeso = ValidadorCampos.Peso(r.Peso);
        if (errorPeso is not null) return BadRequest(new ErrorDTO("peso", errorPeso));
        if (!CalculosNutricionales.Objetivos.Contains(r.Objetivo))
            return BadRequest(new ErrorDTO("objetivo", "Selecciona un objetivo válido."));

        var perfil = await Perfiles().FirstOrDefaultAsync(p => p.Id == id);
        if (perfil is null) return NotFound(new ErrorDTO(null, "No se encontró al estudiante."));

        var ultimo = perfil.Historiales.OrderBy(h => h.FechaRegistro).LastOrDefault();
        if (ultimo is null)
            return BadRequest(new ErrorDTO(null, "El estudiante no tiene una altura registrada."));

        // Coherencia: el IMC con la altura registrada no debe ser absurdo
        var errorImc = ValidadorCampos.CoherenciaImc(r.Peso, (double)ultimo.Altura);
        if (errorImc is not null) return BadRequest(new ErrorDTO("peso", errorImc));

        // Un cambio de más de 10 kg respecto al último control debe confirmarse (probable error de tecleo)
        var pesoAnterior = (double)ultimo.Peso;
        if (!r.ConfirmarCambioGrande && ValidadorCampos.CambioPesoRequiereConfirmacion(pesoAnterior, r.Peso))
            return Conflict(new ErrorDTO("confirmacion", ValidadorCampos.MensajeCambioPeso(pesoAnterior, r.Peso)));

        var ahora = DateTime.Now;
        var peso = Math.Round((decimal)r.Peso, 2);
        var imc = (decimal)CalculosNutricionales.CalcularImc(r.Peso, (double)ultimo.Altura);

        var deHoy = perfil.Historiales.FirstOrDefault(h => h.FechaRegistro.Date == ahora.Date);
        if (deHoy is null)
        {
            perfil.Historiales.Add(new HistorialFisico
            {
                Peso = peso,
                Altura = ultimo.Altura,
                Imc = imc,
                FechaRegistro = ahora,
            });
        }
        else
        {
            deHoy.Peso = peso;
            deHoy.Imc = imc;
            deHoy.FechaRegistro = ahora;
        }

        perfil.Objetivo = r.Objetivo;

        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = perfil.UsuarioId,
            Accion = "ACTUALIZAR_PERFIL",
            Descripcion = $"Peso {peso} kg, objetivo '{r.Objetivo}'.",
            Fecha = ahora,
        });
        await _context.SaveChangesAsync();

        return Ok(EstudianteMapper.Mapear(perfil));
    }

    // HU-11: reporte nutricional semanal (semanas de lunes a domingo)
    [HttpGet("{id:int}/reporte-semanal")]
    public async Task<ActionResult<ReporteSemanalDTO>> GetReporteSemanal(int id)
    {
        var perfil = await Perfiles().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (perfil is null) return NotFound(new ErrorDTO(null, "No se encontró al estudiante."));

        var semanas = new List<SemanaDTO>();
        double? anterior = null;

        var grupos = perfil.Historiales
            .GroupBy(h => CalculosNutricionales.InicioSemana(DateOnly.FromDateTime(h.FechaRegistro)))
            .OrderBy(g => g.Key);

        foreach (var grupo in grupos)
        {
            var controles = grupo.OrderBy(h => h.FechaRegistro).ToList();
            var ultimo = controles[^1];
            var pesoFinal = (double)ultimo.Peso;

            semanas.Add(new SemanaDTO
            {
                Inicio = grupo.Key,
                Fin = grupo.Key.AddDays(6),
                Controles = controles.Count,
                PesoPromedio = Math.Round(controles.Average(h => (double)h.Peso), 1, MidpointRounding.AwayFromZero),
                PesoFinal = pesoFinal,
                ImcFinal = (double)ultimo.Imc,
                Variacion = anterior is null
                    ? null
                    : Math.Round(pesoFinal - anterior.Value, 1, MidpointRounding.AwayFromZero),
            });
            anterior = pesoFinal;
        }

        return new ReporteSemanalDTO
        {
            EstudianteId = perfil.Id,
            Nombre = perfil.Usuario?.Nombre ?? "",
            Objetivo = perfil.Objetivo,
            Semanas = semanas,
        };
    }

    private static ErrorDTO? ValidarRegistro(RegistroEstudianteDTO r)
    {
        var nombres = ValidadorCampos.NormalizarTexto(r.Nombres);
        var apellidos = ValidadorCampos.NormalizarTexto(r.Apellidos);

        var error = ValidadorCampos.Nombre(nombres, "los nombres");
        if (error is not null) return new("nombres", error);

        error = ValidadorCampos.Nombre(apellidos, "los apellidos");
        if (error is not null) return new("apellidos", error);

        error = ValidadorCampos.NombreCompleto(nombres, apellidos);
        if (error is not null) return new("apellidos", error);

        error = ValidadorCampos.Correo(r.Correo);
        if (error is not null) return new("correo", error);

        error = ValidadorCampos.Password(r.Password);
        if (error is not null) return new("password", error);

        error = ValidadorCampos.FechaNacimiento(r.FechaNacimiento, DateOnly.FromDateTime(DateTime.Today));
        if (error is not null) return new("fechaNacimiento", error);

        error = ValidadorCampos.Peso(r.Peso);
        if (error is not null) return new("peso", error);

        error = ValidadorCampos.Altura(r.Altura);
        if (error is not null) return new("altura", error);

        error = ValidadorCampos.CoherenciaImc(r.Peso, CalculosNutricionales.NormalizarAltura(r.Altura));
        if (error is not null) return new("peso", error);

        if (!CalculosNutricionales.Objetivos.Contains(r.Objetivo))
            return new("objetivo", "Selecciona un objetivo válido.");

        return null;
    }
}
