using System.Globalization;
using SmartNutriTracker_biblio.DTOs;
using SmartNutriTracker_biblio.Helpers;

namespace SmartNutriTracker_blazor.Services;

public static class Nutricion
{
    public static string[] Objetivos => CalculosNutricionales.Objetivos;

    public static readonly CategoriaImc[] CategoriasImc =
    {
        new("bajo", "Bajo peso", 18.5, "border-yellow-400/30 bg-yellow-400/10 text-yellow-300", "bg-yellow-400"),
        new("normal", "Normal", 25, "border-green-400/30 bg-green-400/10 text-green-300", "bg-green-400"),
        new("sobrepeso", "Sobrepeso", 30, "border-orange-400/30 bg-orange-400/10 text-orange-300", "bg-orange-400"),
        new("obesidad", "Obesidad", double.PositiveInfinity, "border-red-400/30 bg-red-400/10 text-red-300", "bg-red-400"),
    };

    private static readonly string[] Meses =
        { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

    public static CategoriaImc CategoriaIMC(double imc) =>
        CategoriasImc.FirstOrDefault(c => imc < c.Maximo) ?? CategoriasImc[^1];

    public static double Numero(string? texto) =>
        double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : double.NaN;

    public static double NormalizarAltura(string? valor) => CalculosNutricionales.NormalizarAltura(Numero(valor));

    public static bool AlturaValida(double metros) => CalculosNutricionales.AlturaValida(metros);

    public static double CalcularIMC(double peso, double alturaMetros) => CalculosNutricionales.CalcularImc(peso, alturaMetros);

    public static int CalcularEdad(DateOnly nacimiento) => CalculosNutricionales.CalcularEdad(nacimiento, Hoy());

    public static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.Today);

    public static string HoyIso() => Hoy().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string FormatearFecha(DateOnly fecha, bool corta = false) =>
        corta ? $"{fecha.Day} {Meses[fecha.Month - 1]}" : $"{fecha.Day} {Meses[fecha.Month - 1]} {fecha.Year}";

    public static string Iniciales(string nombre)
    {
        var palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (palabras.Length == 0) return "?";
        if (palabras.Length == 1) return palabras[0][0].ToString().ToUpperInvariant();
        return (palabras[0][0].ToString() + palabras[^1][0]).ToUpperInvariant();
    }

    public static ControlDTO? UltimoControl(EstudianteDTO e) => e.Historial.LastOrDefault();

    public static double VariacionPeso(EstudianteDTO e)
    {
        if (e.Historial.Count < 2) return 0;
        return Math.Round(e.Historial[^1].Peso - e.Historial[0].Peso, 1, MidpointRounding.AwayFromZero);
    }

    public static bool VariacionFavorable(string objetivo, double variacion) => objetivo switch
    {
        "Bajar de peso" => variacion < 0,
        "Ganar masa" => variacion > 0,
        "Mantener" => Math.Abs(variacion) <= 1,
        _ => false,
    };

    public static string Dec(double valor, int decimales = 1) =>
        valor.ToString("F" + decimales, CultureInfo.InvariantCulture);

    public static string Signo(double valor, int decimales = 1)
    {
        if (valor > 0) return "+" + valor.ToString("F" + decimales, CultureInfo.InvariantCulture);
        if (valor < 0) return "−" + Math.Abs(valor).ToString("F" + decimales, CultureInfo.InvariantCulture);
        return Math.Abs(valor).ToString("F" + decimales, CultureInfo.InvariantCulture);
    }

    public static string Pct(double valor) => valor.ToString("0.####", CultureInfo.InvariantCulture);
}