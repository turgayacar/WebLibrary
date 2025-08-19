using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace WebLibrary.Logging
{
    /// <summary>
    /// Structured logging için helper sınıfı
    /// </summary>
    public static class StructuredLoggingHelper
    {
        private static Serilog.ILogger? _logger;

        /// <summary>
        /// Logger'ı yapılandırır
        /// </summary>
        /// <param name="logLevel">Minimum log seviyesi</param>
        /// <param name="outputTemplate">Log formatı</param>
        /// <param name="filePath">Log dosya yolu (opsiyonel)</param>
        public static void ConfigureLogger(LogEventLevel logLevel = LogEventLevel.Information, 
            string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            string? filePath = null)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .WriteTo.Console(outputTemplate: outputTemplate);

            if (!string.IsNullOrEmpty(filePath))
            {
                loggerConfig.WriteTo.File(filePath, 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: outputTemplate);
            }

            _logger = loggerConfig.CreateLogger();
        }

        /// <summary>
        /// Information seviyesinde log yazar
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="properties">Ek özellikler</param>
        public static void LogInformation(string message, params object[] properties)
        {
            _logger?.Information(message, properties);
        }

        /// <summary>
        /// Warning seviyesinde log yazar
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="properties">Ek özellikler</param>
        public static void LogWarning(string message, params object[] properties)
        {
            _logger?.Warning(message, properties);
        }

        /// <summary>
        /// Error seviyesinde log yazar
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="exception">Hata</param>
        /// <param name="properties">Ek özellikler</param>
        public static void LogError(string message, Exception? exception = null, params object[] properties)
        {
            if (exception != null)
                _logger?.Error(exception, message, properties);
            else
                _logger?.Error(message, properties);
        }

        /// <summary>
        /// Debug seviyesinde log yazar
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="properties">Ek özellikler</param>
        public static void LogDebug(string message, params object[] properties)
        {
            _logger?.Debug(message, properties);
        }

        /// <summary>
        /// Fatal seviyesinde log yazar
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="exception">Hata</param>
        /// <param name="properties">Ek özellikler</param>
        public static void LogFatal(string message, Exception? exception = null, params object[] properties)
        {
            if (exception != null)
                _logger?.Fatal(exception, message, properties);
            else
                _logger?.Fatal(message, properties);
        }

        /// <summary>
        /// Structured log yazar
        /// </summary>
        /// <param name="level">Log seviyesi</param>
        /// <param name="message">Log mesajı</param>
        /// <param name="properties">Ek özellikler</param>
        public static void Log(LogEventLevel level, string message, params object[] properties)
        {
            _logger?.Write(level, message, properties);
        }

        /// <summary>
        /// Logger'ı kapatır
        /// </summary>
        public static void CloseLogger()
        {
            // Serilog ILogger otomatik olarak dispose edilir
            _logger = null;
        }
    }
}
