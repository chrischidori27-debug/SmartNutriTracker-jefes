
using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public class MenuServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;

        public MenuServicio(IDbContextFactory<SmartNutriTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // LISTAR TODOS LOS MENUS
        public async Task<List<MenusDiario>> ListarMenusAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.MenusDiarios
                .Include(m => m.MenuAlimentos)
                .ThenInclude(ma => ma.Alimento)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        // LISTAR ALIMENTOS ACTIVOS
        public async Task<List<Alimento>> ListarAlimentosAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Alimentos
                .Where(a => a.Estado == true)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        // CREAR UN MENU
        public async Task<(bool Exito, string Mensaje)> CrearMenuAsync(
            string nombre,
            string descripcion,
            DateOnly fecha,
            int usuarioId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return (false, "El nombre del menú es obligatorio.");
            }

            if (usuarioId <= 0)
            {
                return (false, "Debes iniciar sesión para crear un menú.");
            }

            bool usuarioExiste = await context.Usuarios
                .AnyAsync(u => u.Id == usuarioId);

            if (!usuarioExiste)
            {
                return (false, "No se encontró el usuario.");
            }

            var menu = new MenusDiario
            {
                Nombre = nombre.Trim(),
                Descripcion = descripcion,
                Fecha = fecha,
                UsuarioId = usuarioId,
                FechaRegistro = DateTime.Now
            };

            context.MenusDiarios.Add(menu);

            await context.SaveChangesAsync();

            return (true, "Menú creado correctamente.");
        }

        // AGREGAR ALIMENTO A UN MENU
        public async Task<(bool Exito, string Mensaje)> AgregarAlimentoAsync(
            int menuId,
            int alimentoId,
            decimal cantidad)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            if (cantidad <= 0)
            {
                return (false, "La cantidad debe ser mayor que cero.");
            }

            var menu = await context.MenusDiarios
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (menu == null)
            {
                return (false, "No se encontró el menú.");
            }

            var alimento = await context.Alimentos
                .FirstOrDefaultAsync(a => a.Id == alimentoId && a.Estado == true);

            if (alimento == null)
            {
                return (false, "El alimento no existe o está desactivado.");
            }

            var existente = await context.MenuAlimentos
                .FirstOrDefaultAsync(ma =>
                    ma.MenuId == menuId &&
                    ma.AlimentoId == alimentoId);

            if (existente != null)
            {
                existente.Cantidad = cantidad;
            }
            else
            {
                var menuAlimento = new MenuAlimento
                {
                    MenuId = menuId,
                    AlimentoId = alimentoId,
                    Cantidad = cantidad
                };

                context.MenuAlimentos.Add(menuAlimento);
            }

            await context.SaveChangesAsync();

            return (true, "Alimento agregado al menú correctamente.");
        }

        // OBTENER DETALLE DE UN MENU
        public async Task<MenusDiario?> ObtenerMenuAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.MenusDiarios
                .Include(m => m.MenuAlimentos)
                .ThenInclude(ma => ma.Alimento)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
