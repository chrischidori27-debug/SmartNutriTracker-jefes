using System.Globalization;

namespace SmartNutriTracker_biblio.Helpers;

/// <summary>
/// Validaciones por campo compartidas entre la API y el cliente. Cada método devuelve
/// el mensaje de error (en español) o null si el valor es válido.
/// </summary>
public static class ValidadorCampos
{
    private const string LetrasPermitidas = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÁÉÍÓÚÜÑáéíóúüñ";
    private const string SeparadoresPermitidos = " '’-";

    private static string Num(double valor, int decimales = 1) =>
        valor.ToString("0." + new string('#', decimales), CultureInfo.InvariantCulture);

    // ---------- Texto ----------

    /// <summary>Quita espacios al inicio y al final y junta los espacios dobles.</summary>
    public static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        return string.Join(' ', texto.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    // ---------- Nombres y apellidos ----------

    /// <param name="valor">Texto ya normalizado con NormalizarTexto.</param>
    /// <param name="etiqueta">"los nombres" o "los apellidos".</param>
    public static string? Nombre(string valor, string etiqueta)
    {
        if (valor.Length == 0) return $"Ingresa {etiqueta}.";

        if (valor.Any(c => !LetrasPermitidas.Contains(c) && !SeparadoresPermitidos.Contains(c)))
            return "Usa solo letras, espacios, apóstrofo y guion. No se permiten números ni símbolos.";

        if (SeparadoresPermitidos.Contains(valor[0]) || SeparadoresPermitidos.Contains(valor[^1]) ||
            HaySeparadoresSeguidos(valor))
            return "El apóstrofo y el guion solo pueden ir entre letras.";

        if (valor.Count(c => LetrasPermitidas.Contains(c)) < ReglasValidacion.NombreMinLetras)
            return $"Debe tener al menos {ReglasValidacion.NombreMinLetras} letras.";

        return null;
    }

    private static bool HaySeparadoresSeguidos(string valor)
    {
        for (var i = 1; i < valor.Length; i++)
            if (SeparadoresPermitidos.Contains(valor[i]) && SeparadoresPermitidos.Contains(valor[i - 1]))
                return true;
        return false;
    }

    public static string? NombreCompleto(string nombres, string apellidos)
    {
        var total = nombres.Length + 1 + apellidos.Length;
        return total > ReglasValidacion.NombreCompletoMax
            ? $"Nombres y apellidos no pueden superar {ReglasValidacion.NombreCompletoMax} caracteres en total (llevas {total})."
            : null;
    }

    // ---------- Correo ----------

    public static string? Correo(string? correo)
    {
        var valor = correo?.Trim() ?? "";
        if (valor.Length == 0) return "Ingresa el correo institucional.";
        if (valor.Length > ReglasValidacion.CorreoMax)
            return $"El correo no puede superar {ReglasValidacion.CorreoMax} caracteres.";

        const string invalido = "Ingresa un correo válido (ej. alumno@innovare.edu).";
        var partes = valor.Split('@');
        if (partes.Length != 2) return invalido;
        var local = partes[0];
        var dominio = partes[1];
        if (local.Length == 0 || dominio.Length == 0) return invalido;

        const string puntos = "El correo no puede tener puntos seguidos ni empezar o terminar con punto.";
        if (local.StartsWith('.') || local.EndsWith('.') || dominio.StartsWith('.') || dominio.EndsWith('.') ||
            valor.Contains("..", StringComparison.Ordinal))
            return puntos;

        if (local.Length > ReglasValidacion.CorreoParteLocalMax) return invalido;
        if (local.Any(c => !(char.IsAsciiLetterOrDigit(c) || "._%+-".Contains(c)))) return invalido;

        var etiquetas = dominio.Split('.');
        if (etiquetas.Length < 2) return invalido;
        foreach (var etiqueta in etiquetas)
        {
            if (etiqueta.Length is < 1 or > 63) return invalido;
            if (etiqueta.StartsWith('-') || etiqueta.EndsWith('-')) return invalido;
            if (etiqueta.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '-'))) return invalido;
        }
        var extension = etiquetas[^1];
        if (extension.Length < 2 || !extension.All(char.IsAsciiLetter)) return invalido;

        return null;
    }

    // ---------- Contraseña ----------

    public static string? Password(string? password)
    {
        var valor = password ?? "";
        if (valor.Length == 0) return "Ingresa una contraseña.";
        if (string.IsNullOrWhiteSpace(valor)) return "La contraseña no puede estar formada solo por espacios.";
        if (valor.Length < ReglasValidacion.PasswordMin)
            return $"Usa al menos {ReglasValidacion.PasswordMin} caracteres.";
        if (valor.Length > ReglasValidacion.PasswordMax)
            return $"No puede superar los {ReglasValidacion.PasswordMax} caracteres.";
        if (!valor.Any(char.IsLetter) || !valor.Any(char.IsDigit))
            return "Debe incluir al menos una letra y un número.";
        return null;
    }

    // ---------- Fecha de nacimiento ----------

    public static string? FechaNacimiento(DateOnly nacimiento, DateOnly hoy)
    {
        if (nacimiento > hoy) return "La fecha de nacimiento no puede ser futura.";
        var edad = CalculosNutricionales.CalcularEdad(nacimiento, hoy);
        if (edad < ReglasValidacion.EdadMin || edad > ReglasValidacion.EdadMax)
            return $"La edad debe estar entre {ReglasValidacion.EdadMin} y {ReglasValidacion.EdadMax} años.";
        return null;
    }

    public static DateOnly FechaNacimientoMinima(DateOnly hoy) => hoy.AddYears(-(ReglasValidacion.EdadMax + 1)).AddDays(1);

    public static DateOnly FechaNacimientoMaxima(DateOnly hoy) => hoy.AddYears(-ReglasValidacion.EdadMin);

    // ---------- Peso ----------

    public static string? Peso(double peso)
    {
        if (!double.IsFinite(peso)) return "Ingresa el peso en kg.";
        if (peso < ReglasValidacion.PesoMin || peso > ReglasValidacion.PesoMax)
            return $"El peso debe estar entre {Num(ReglasValidacion.PesoMin)} y {Num(ReglasValidacion.PesoMax)} kg.";
        if (!TieneMaxDecimales(peso, ReglasValidacion.PesoDecimales))
            return $"El peso admite máximo {ReglasValidacion.PesoDecimales} decimales (ej. 68.45).";
        return null;
    }

    // ---------- Altura ----------

    /// <param name="valor">Altura tal como se escribió: metros (1.75) o centímetros (175).</param>
    public static string? Altura(double valor)
    {
        if (!double.IsFinite(valor) || valor <= 0) return "Ingresa la altura.";

        var enCentimetros = valor > 3;
        if (enCentimetros && !TieneMaxDecimales(valor, 0))
            return "Si escribes la altura en centímetros, usa un número entero (ej. 175).";
        if (!enCentimetros && !TieneMaxDecimales(valor, ReglasValidacion.AlturaDecimales))
            return $"La altura admite máximo {ReglasValidacion.AlturaDecimales} decimales (ej. 1.75).";

        var metros = enCentimetros ? valor / 100 : valor;
        if (!CalculosNutricionales.AlturaValida(metros))
            return "La altura debe estar entre 0.5 y 2.5 m (o entre 50 y 250 cm).";
        return null;
    }

    // ---------- Coherencia peso / altura ----------

    public static string? CoherenciaImc(double peso, double alturaMetros)
    {
        var imc = CalculosNutricionales.CalcularImc(peso, alturaMetros);
        if (imc <= 0) return null;
        if (imc < ReglasValidacion.ImcMin || imc > ReglasValidacion.ImcMax)
            return $"Con ese peso y esa altura el IMC sería {Num(imc)}, un valor poco realista. Revisa los datos.";
        return null;
    }

    // ---------- HU-10: cambio grande de peso ----------

    public static bool CambioPesoRequiereConfirmacion(double pesoAnterior, double pesoNuevo) =>
        Math.Abs(pesoNuevo - pesoAnterior) > ReglasValidacion.CambioPesoSinConfirmar;

    public static string MensajeCambioPeso(double pesoAnterior, double pesoNuevo) =>
        $"El peso nuevo ({Num(pesoNuevo, 2)} kg) difiere en más de {Num(ReglasValidacion.CambioPesoSinConfirmar)} kg " +
        $"del último control ({Num(pesoAnterior, 2)} kg). ¿Es correcto o fue un error de tecleo?";

    // ---------- Utilidades ----------

    private static bool TieneMaxDecimales(double valor, int decimales)
    {
        var escala = Math.Pow(10, decimales);
        return Math.Abs(valor * escala - Math.Round(valor * escala)) < 1e-6;
    }
}
