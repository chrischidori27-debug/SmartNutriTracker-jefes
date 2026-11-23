using System.Text;

namespace SmartNutriTracker.Logging
{
    // HU-29: logger global que escribe errores y acciones en logs/smartnutri-AAAAMMDD.log
    public sealed class ArchivoLoggerProvider : ILoggerProvider
    {
        private readonly string _directorio;
        private readonly object _bloqueo = new();

        public ArchivoLoggerProvider(string directorio)
        {
            _directorio = directorio;
        }

        public ILogger CreateLogger(string categoryName) =>
            new ArchivoLogger(categoryName, this);

        internal void Escribir(string linea)
        {
            try
            {
                lock (_bloqueo)
                {
                    Directory.CreateDirectory(_directorio);

                    string archivo = Path.Combine(
                        _directorio,
                        $"smartnutri-{DateTime.Now:yyyyMMdd}.log");

                    File.AppendAllText(archivo, linea + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Un fallo al escribir el log nunca debe tumbar la aplicación.
            }
        }

        public void Dispose()
        {
        }

        private sealed class ArchivoLogger : ILogger
        {
            private readonly string _categoria;
            private readonly ArchivoLoggerProvider _proveedor;

            public ArchivoLogger(string categoria, ArchivoLoggerProvider proveedor)
            {
                _categoria = categoria;
                _proveedor = proveedor;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append(" [").Append(logLevel.ToString().ToUpperInvariant()).Append("] ");
                sb.Append(_categoria).Append(" - ");
                sb.Append(formatter(state, exception));

                if (exception != null)
                {
                    sb.AppendLine().Append(exception);
                }

                _proveedor.Escribir(sb.ToString());
            }
        }
    }
}
