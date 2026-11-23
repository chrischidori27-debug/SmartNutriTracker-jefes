using System.Net.Http.Json;
using SmartNutriTracker_biblio.DTOs;

namespace SmartNutriTracker_blazor.Services;

public class ResultadoApi
{
    public bool Ok { get; private set; }
    public ErrorDTO? Error { get; private set; }

    public static ResultadoApi Correcto() => new() { Ok = true };
    public static ResultadoApi Fallo(ErrorDTO error) => new() { Ok = false, Error = error };
}

public class PerfilService
{
    private readonly HttpClient _http;

    public PerfilService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EstudianteDTO>> ObtenerTodosAsync() =>
        await _http.GetFromJsonAsync<List<EstudianteDTO>>("api/perfiles") ?? new();

    public async Task<ReporteSemanalDTO?> ObtenerReporteSemanalAsync(int id) =>
        await _http.GetFromJsonAsync<ReporteSemanalDTO>($"api/perfiles/{id}/reporte-semanal");

    public Task<ResultadoApi> RegistrarAsync(RegistroEstudianteDTO dto) => EnviarAsync("api/perfiles", dto);

    public Task<ResultadoApi> GuardarControlAsync(int id, ActualizarControlDTO dto) =>
        EnviarAsync($"api/perfiles/{id}/controles", dto);

    private async Task<ResultadoApi> EnviarAsync<T>(string url, T cuerpo)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync(url, cuerpo);
            if (respuesta.IsSuccessStatusCode) return ResultadoApi.Correcto();

            ErrorDTO? error = null;
            try { error = await respuesta.Content.ReadFromJsonAsync<ErrorDTO>(); }
            catch (Exception) { /* la respuesta no era un ErrorDTO */ }

            if (string.IsNullOrEmpty(error?.Mensaje))
                error = new ErrorDTO(null, $"Error del servidor ({(int)respuesta.StatusCode}).");
            return ResultadoApi.Fallo(error);
        }
        catch (HttpRequestException)
        {
            return ResultadoApi.Fallo(new ErrorDTO(null, "No se pudo conectar con la API."));
        }
    }
}