
using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public class AlimentoServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;

        public AlimentoServicio(
            IDbContextFactory<SmartNutriTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Consultar alimentos activos
        public async Task<List<Alimento>> ListarAsync()
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Alimentos
                .Where(a => a.Estado == true)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        // Registrar alimento
        public async Task<(bool Exito, string Mensaje)> RegistrarAsync(
            Alimento alimento)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            alimento.Nombre = alimento.Nombre.Trim();

            bool existe = await context.Alimentos
                .AnyAsync(a => a.Nombre == alimento.Nombre);

            if (existe)
            {
                return (false, "Ya existe un alimento con ese nombre.");
            }

            alimento.FechaRegistro = DateTime.Now;
            alimento.Estado = true;

            context.Alimentos.Add(alimento);

            await context.SaveChangesAsync();

            return (true, "Alimento registrado correctamente.");
        }

        // Actualizar alimento
        public async Task<(bool Exito, string Mensaje)> ActualizarAsync(
            Alimento alimento)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var alimentoBD = await context.Alimentos
                .FirstOrDefaultAsync(a => a.Id == alimento.Id);

            if (alimentoBD == null)
            {
                return (false, "No se encontró el alimento.");
            }

            alimentoBD.Nombre = alimento.Nombre.Trim();
            alimentoBD.Descripcion = alimento.Descripcion;
            alimentoBD.Categoria = alimento.Categoria;
            alimentoBD.Porcion = alimento.Porcion;
            alimentoBD.Calorias = alimento.Calorias;
            alimentoBD.Proteinas = alimento.Proteinas;
            alimentoBD.Carbohidratos = alimento.Carbohidratos;
            alimentoBD.Grasas = alimento.Grasas;
            alimentoBD.Fibra = alimento.Fibra;

            await context.SaveChangesAsync();

            return (true, "Alimento actualizado correctamente.");
        }

        // Desactivar alimento
        public async Task<(bool Exito, string Mensaje)> DesactivarAsync(
            int id)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var alimento = await context.Alimentos
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alimento == null)
            {
                return (false, "No se encontró el alimento.");
            }

            alimento.Estado = false;

            await context.SaveChangesAsync();

            return (true, "Alimento desactivado correctamente.");
        }
    }
}
