using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    // HU-29: registra las acciones de los usuarios en la tabla LogsAuditoria
    public class AuditoriaServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;
        private readonly ILogger<AuditoriaServicio> _logger;

        public AuditoriaServicio(
            IDbContextFactory<SmartNutriTrackerContext> contextFactory,
            ILogger<AuditoriaServicio> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task RegistrarAsync(int? usuarioId, string accion, string? descripcion = null)
        {
            _logger.LogInformation(
                "Acción {Accion} (usuario {UsuarioId}): {Descripcion}",
                accion, usuarioId, descripcion);

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                context.LogsAuditoria.Add(new LogsAuditorium
                {
                    UsuarioId = usuarioId,
                    Accion = Recortar(accion, 100)!,
                    Descripcion = Recortar(descripcion, 500),
                    Fecha = DateTime.Now
                });

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // La auditoría nunca debe romper la operación principal.
                _logger.LogError(ex, "No se pudo guardar la auditoría de la acción {Accion}.", accion);
            }
        }

        private static string? Recortar(string? texto, int max) =>
            texto != null && texto.Length > max ? texto[..max] : texto;
    }
}
