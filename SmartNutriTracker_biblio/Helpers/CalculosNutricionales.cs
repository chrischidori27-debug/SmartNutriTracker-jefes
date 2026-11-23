namespace SmartNutriTracker_biblio.Helpers;

public static class CalculosNutricionales
{
    public static readonly string[] Objetivos = { "Bajar de peso", "Mantener", "Ganar masa" };

    public static double NormalizarAltura(double valor)
    {
        if (!double.IsFinite(valor) || valor <= 0) return 0;
        return Math.Round(valor > 3 ? valor / 100 : valor, 2, MidpointRounding.AwayFromZero);
    }

    public static bool AlturaValida(double metros) => metros >= 0.5 && metros <= 2.5;

    public static double CalcularImc(double peso, double alturaMetros)
    {
        if (!(peso > 0) || !(alturaMetros > 0)) return 0;
        return Math.Round(peso / (alturaMetros * alturaMetros), 2, MidpointRounding.AwayFromZero);
    }

    public static int CalcularEdad(DateOnly nacimiento, DateOnly hoy)
    {
        var edad = hoy.Year - nacimiento.Year;
        if (hoy.Month < nacimiento.Month || (hoy.Month == nacimiento.Month && hoy.Day < nacimiento.Day)) edad--;
        return edad;
    }

    public static DateOnly InicioSemana(DateOnly fecha) => fecha.AddDays(-(((int)fecha.DayOfWeek + 6) % 7));
}