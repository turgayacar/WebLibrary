using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using WebLibrary.Models;

namespace WebLibrary.Logging
{
    /// <summary>
    /// Health check için helper sınıfı
    /// </summary>
    public static class HealthCheckHelper
    {
        private static readonly Dictionary<string, HealthCheckInfo> _healthChecks = new();

        /// <summary>
        /// Health check kaydeder
        /// </summary>
        /// <param name="name">Health check adı</param>
        /// <param name="status">Durum</param>
        /// <param name="description">Açıklama</param>
        /// <param name="data">Ek veriler</param>
        public static void RecordHealthCheck(string name, HealthStatus status, string description = "", Dictionary<string, object>? data = null)
        {
            var healthCheck = new HealthCheckInfo
            {
                Name = name,
                Status = status,
                Description = description,
                Data = data ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            _healthChecks[name] = healthCheck;
        }

        /// <summary>
        /// Health check durumunu alır
        /// </summary>
        /// <param name="name">Health check adı</param>
        /// <returns>Health check durumu</returns>
        public static ServiceResult<HealthCheckInfo> GetHealthCheck(string name)
        {
            if (_healthChecks.TryGetValue(name, out var healthCheck))
                return ServiceResult<HealthCheckInfo>.Success(healthCheck);

            return ServiceResult<HealthCheckInfo>.Error($"Health check bulunamadı: {name}");
        }

        /// <summary>
        /// Tüm health check'leri alır
        /// </summary>
        /// <returns>Tüm health check'ler</returns>
        public static ServiceResult<List<HealthCheckInfo>> GetAllHealthChecks()
        {
            var checks = _healthChecks.Values.ToList();
            return ServiceResult<List<HealthCheckInfo>>.Success(checks);
        }

        /// <summary>
        /// Genel sistem durumunu alır
        /// </summary>
        /// <returns>Sistem durumu</returns>
        public static ServiceResult<SystemHealthStatus> GetSystemHealthStatus()
        {
            var checks = _healthChecks.Values.ToList();
            
            if (!checks.Any())
                return ServiceResult<SystemHealthStatus>.Success(new SystemHealthStatus { OverallStatus = HealthStatus.Healthy });

            var overallStatus = checks.All(c => c.Status == HealthStatus.Healthy) ? HealthStatus.Healthy :
                               checks.Any(c => c.Status == HealthStatus.Unhealthy) ? HealthStatus.Unhealthy :
                               HealthStatus.Degraded;

            var status = new SystemHealthStatus
            {
                OverallStatus = overallStatus,
                TotalChecks = checks.Count,
                HealthyChecks = checks.Count(c => c.Status == HealthStatus.Healthy),
                UnhealthyChecks = checks.Count(c => c.Status == HealthStatus.Unhealthy),
                DegradedChecks = checks.Count(c => c.Status == HealthStatus.Degraded),
                LastCheckTime = checks.Max(c => c.Timestamp),
                HealthChecks = checks
            };

            return ServiceResult<SystemHealthStatus>.Success(status);
        }

        /// <summary>
        /// Database bağlantı health check'i
        /// </summary>
        /// <param name="connectionString">Bağlantı string'i</param>
        /// <returns>Health check sonucu</returns>
        public static async Task<HealthStatus> CheckDatabaseConnection(string connectionString)
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                
                var stopwatch = Stopwatch.StartNew();
                using var command = new Microsoft.Data.SqlClient.SqlCommand("SELECT 1", connection);
                await command.ExecuteScalarAsync();
                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    { "ResponseTime", stopwatch.ElapsedMilliseconds },
                    { "ConnectionString", connectionString }
                };

                RecordHealthCheck("DatabaseConnection", HealthStatus.Healthy, "Database bağlantısı başarılı", data);
                return HealthStatus.Healthy;
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "ConnectionString", connectionString }
                };

                RecordHealthCheck("DatabaseConnection", HealthStatus.Unhealthy, "Database bağlantısı başarısız", data);
                return HealthStatus.Unhealthy;
            }
        }

        /// <summary>
        /// HTTP endpoint health check'i
        /// </summary>
        /// <param name="url">Endpoint URL'i</param>
        /// <param name="timeoutSeconds">Timeout süresi (saniye)</param>
        /// <returns>Health check sonucu</returns>
        public static async Task<HealthStatus> CheckHttpEndpoint(string url, int timeoutSeconds = 30)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                var stopwatch = Stopwatch.StartNew();
                var response = await httpClient.GetAsync(url);
                stopwatch.Stop();

                var data = new Dictionary<string, object>
                {
                    { "ResponseTime", stopwatch.ElapsedMilliseconds },
                    { "StatusCode", (int)response.StatusCode },
                    { "Url", url }
                };

                var status = response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Degraded;
                var description = response.IsSuccessStatusCode ? "HTTP endpoint erişilebilir" : "HTTP endpoint hata döndürüyor";

                RecordHealthCheck("HttpEndpoint", status, description, data);
                return status;
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "Url", url }
                };

                RecordHealthCheck("HttpEndpoint", HealthStatus.Unhealthy, "HTTP endpoint erişilemiyor", data);
                return HealthStatus.Unhealthy;
            }
        }

        /// <summary>
        /// Disk kullanımı health check'i
        /// </summary>
        /// <param name="drivePath">Disk yolu</param>
        /// <param name="warningThreshold">Uyarı eşiği (GB)</param>
        /// <param name="criticalThreshold">Kritik eşik (GB)</param>
        /// <returns>Health check sonucu</returns>
        public static HealthStatus CheckDiskUsage(string drivePath = "C:\\", double warningThreshold = 80, double criticalThreshold = 90)
        {
            try
            {
                var drive = new DriveInfo(drivePath);
                var usagePercentage = 100 - (drive.AvailableFreeSpace * 100.0 / drive.TotalSize);

                var data = new Dictionary<string, object>
                {
                    { "DrivePath", drivePath },
                    { "TotalSize", drive.TotalSize / (1024 * 1024 * 1024) }, // GB
                    { "AvailableSpace", drive.AvailableFreeSpace / (1024 * 1024 * 1024) }, // GB
                    { "UsagePercentage", Math.Round(usagePercentage, 2) }
                };

                HealthStatus status;
                string description;

                if (usagePercentage >= criticalThreshold)
                {
                    status = HealthStatus.Unhealthy;
                    description = "Disk kullanımı kritik seviyede";
                }
                else if (usagePercentage >= warningThreshold)
                {
                    status = HealthStatus.Degraded;
                    description = "Disk kullanımı uyarı seviyesinde";
                }
                else
                {
                    status = HealthStatus.Healthy;
                    description = "Disk kullanımı normal";
                }

                RecordHealthCheck("DiskUsage", status, description, data);
                return status;
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>
                {
                    { "Error", ex.Message },
                    { "DrivePath", drivePath }
                };

                RecordHealthCheck("DiskUsage", HealthStatus.Unhealthy, "Disk kullanımı kontrol edilemiyor", data);
                return HealthStatus.Unhealthy;
            }
        }

        /// <summary>
        /// Health check verilerini temizler
        /// </summary>
        public static void ClearHealthChecks()
        {
            _healthChecks.Clear();
        }
    }

    /// <summary>
    /// Health check bilgisi
    /// </summary>
    public class HealthCheckInfo
    {
        /// <summary>
        /// Health check adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Durum
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Açıklama
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Ek veriler
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// Zaman damgası
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Sistem genel sağlık durumu
    /// </summary>
    public class SystemHealthStatus
    {
        /// <summary>
        /// Genel durum
        /// </summary>
        public HealthStatus OverallStatus { get; set; }

        /// <summary>
        /// Toplam health check sayısı
        /// </summary>
        public int TotalChecks { get; set; }

        /// <summary>
        /// Sağlıklı check sayısı
        /// </summary>
        public int HealthyChecks { get; set; }

        /// <summary>
        /// Sağlıksız check sayısı
        /// </summary>
        public int UnhealthyChecks { get; set; }

        /// <summary>
        /// Bozulmuş check sayısı
        /// </summary>
        public int DegradedChecks { get; set; }

        /// <summary>
        /// Son kontrol zamanı
        /// </summary>
        public DateTime LastCheckTime { get; set; }

        /// <summary>
        /// Tüm health check'ler
        /// </summary>
        public List<HealthCheckInfo> HealthChecks { get; set; } = new();
    }
}
