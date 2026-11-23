namespace SmartNutriTracker_blazor.Services;

public static class Estilos
{
    public const string Campo =
        "h-11 w-full rounded-lg border border-neutral-800 bg-black px-3.5 text-sm text-white placeholder:text-neutral-600 transition-colors focus:border-green-500 focus:outline-none focus:ring-2 focus:ring-green-500/30 aria-[invalid=true]:border-red-400 aria-[invalid=true]:focus:ring-red-400/30";

    public const string BotonPrimario =
        "inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-green-500 px-5 text-sm font-bold text-black transition-colors hover:bg-green-400";

    public const string BotonSecundario =
        "inline-flex h-11 items-center justify-center gap-2 rounded-lg border border-neutral-700 px-5 text-sm font-semibold text-white transition-colors hover:bg-neutral-900";
}

public record CategoriaImc(string Clave, string Texto, double Maximo, string Chip, string Punto);
public record PuntoGrafico(DateOnly Etiqueta, double Valor);
public record AvisoInfo(long Id, string Mensaje);
public record GuardarControlArgs(int Id, double Peso, string Objetivo);
public record Seccion(string Id, string Etiqueta, string Icono);

public static class Secciones
{
    public static readonly Seccion[] Todas =
    {
        new("panel", "Panel general", "layout-dashboard"),
        new("registro", "Registro", "user-plus"),
        new("seguimiento", "Seguimiento", "trending-up"),
        new("perfil", "Mi perfil", "user"),
    };
}