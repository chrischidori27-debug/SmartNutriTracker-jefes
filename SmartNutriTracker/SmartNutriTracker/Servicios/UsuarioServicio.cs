
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public class UsuarioServicio
    {
        private readonly IDbContextFactory<SmartNutriTrackerContext> _contextFactory;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuarioServicio(
            IDbContextFactory<SmartNutriTrackerContext> contextFactory,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _contextFactory = contextFactory;
            _passwordHasher = passwordHasher;
        }

        // Registrar un nuevo usuario
        public async Task<(bool Exito, string Mensaje)> RegistrarAsync(
            string nombre,
            string correo,
            string contrasena,
            string rolNombre)
        {
            nombre = nombre.Trim();
            correo = correo.Trim().ToLowerInvariant();
            rolNombre = rolNombre.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return (false, "El nombre es obligatorio.");
            }

            if (nombre.Length > 100)
            {
                return (false, "El nombre no puede superar los 100 caracteres.");
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(correo))
            {
                return (false, "Ingresa un correo electrónico válido.");
            }

            if (correo.Length > 150)
            {
                return (false, "El correo es demasiado largo.");
            }

            if (contrasena.Length < 8)
            {
                return (false, "La contraseña debe tener al menos 8 caracteres.");
            }

            string[] rolesPermitidos =
            {
                "Estudiante",
                "Docente",
                "Nutricionista"
            };

            if (!rolesPermitidos.Contains(rolNombre))
            {
                return (false, "No puedes registrarte con ese rol.");
            }

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            bool correoExiste = await context.Usuarios
                .AnyAsync(u => u.Correo == correo);

            if (correoExiste)
            {
                return (false, "Este correo ya está registrado.");
            }

            var rol = await context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == rolNombre);

            if (rol == null)
            {
                return (false, "El rol seleccionado no existe.");
            }

            var usuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                RolId = rol.Id,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(
                usuario,
                contrasena
            );

            context.Usuarios.Add(usuario);

            await context.SaveChangesAsync();

            return (true, "Usuario registrado correctamente.");
        }

        // Validar las credenciales de inicio de sesión
        public async Task<Usuario?> IniciarSesionAsync(
            string correo,
            string contrasena)
        {
            correo = correo.Trim().ToLowerInvariant();

            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var usuario = await context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                return null;
            }

            if (usuario.Estado == false)
            {
                return null;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                contrasena
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return usuario;
        }
    }
}
