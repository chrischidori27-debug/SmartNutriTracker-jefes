
using SmartNutriTracker.Modelos;

namespace SmartNutriTracker.Servicios
{
    public class UsuarioSesion
    {
        public int UsuarioId { get; private set; }
        public string Nombre { get; private set; } = "";
        public string Correo { get; private set; } = "";
        public string Rol { get; private set; } = "";

        public bool EstaAutenticado { get; private set; }

        public void IniciarSesion(Usuario usuario)
        {
            UsuarioId = usuario.Id;
            Nombre = usuario.Nombre;
            Correo = usuario.Correo;
            Rol = usuario.Rol.Nombre;

            EstaAutenticado = true;
        }

        public void CerrarSesion()
        {
            UsuarioId = 0;
            Nombre = "";
            Correo = "";
            Rol = "";
            EstaAutenticado = false;
        }
    }
}
