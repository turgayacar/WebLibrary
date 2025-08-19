using System.Diagnostics;
using WebLibrary.Models;

namespace WebLibrary.Logging
{
    /// <summary>
    /// Performance monitoring için helper sınıfı
    /// </summary>
    public static class PerformanceMonitoringHelper
    {
        private static readonly Dictionary<string, Stopwatch> _timers = new();
        private static readonly Dictionary<string, List<long>> _measurements = new();

        /// <summary>
        /// Performance timer başlatır
        /// </summary>
        /// <param name="operationName">İşlem adı</param>
        public static void StartTimer(string operationName)
        {
            if (_timers.ContainsKey(operationName))
            {
                _timers[operationName].Restart();
            }
            else
            {
                _timers[operationName] = Stopwatch.StartNew();
            }
        }

        /// <summary>
        /// Performance timer durdurur ve süreyi kaydeder
        /// </summary>
        /// <param name="operationName">İşlem adı</param>
        /// <returns>Geçen süre (milisaniye)</returns>
        public static long StopTimer(string operationName)
        {
            if (_timers.TryGetValue(operationName, out var timer))
            {
                timer.Stop();
                var elapsed = timer.ElapsedMilliseconds;

                if (!_measurements.ContainsKey(operationName))
                    _measurements[operationName] = new List<long>();

                _measurements[operationName].Add(elapsed);
                return elapsed;
            }

            return 0;
        }

        /// <summary>
        /// İşlem süresini ölçer
        /// </summary>
        /// <param name="operationName">İşlem adı</param>
        /// <param name="action">Çalıştırılacak işlem</param>
        /// <returns>Geçen süre (milisaniye)</returns>
        public static long MeasureOperation(string operationName, Action action)
        {
            StartTimer(operationName);
            action();
            return StopTimer(operationName);
        }

        /// <summary>
        /// Async işlem süresini ölçer
        /// </summary>
        /// <param name="operationName">İşlem adı</param>
        /// <param name="action">Çalıştırılacak async işlem</param>
        /// <returns>Geçen süre (milisaniye)</returns>
        public static async Task<long> MeasureOperationAsync(string operationName, Func<Task> action)
        {
            StartTimer(operationName);
            await action();
            return StopTimer(operationName);
        }

        /// <summary>
        /// İşlem istatistiklerini alır
        /// </summary>
        /// <param name="operationName">İşlem adı</param>
        /// <returns>İstatistikler</returns>
        public static ServiceResult<PerformanceStats> GetPerformanceStats(string operationName)
        {
            if (!_measurements.ContainsKey(operationName) || !_measurements[operationName].Any())
                return ServiceResult<PerformanceStats>.Error($"İşlem bulunamadı: {operationName}");

            var measurements = _measurements[operationName];
            var stats = new PerformanceStats
            {
                OperationName = operationName,
                TotalExecutions = measurements.Count,
                AverageExecutionTime = measurements.Average(),
                MinExecutionTime = measurements.Min(),
                MaxExecutionTime = measurements.Max(),
                TotalExecutionTime = measurements.Sum()
            };

            return ServiceResult<PerformanceStats>.Success(stats);
        }

        /// <summary>
        /// Tüm işlem istatistiklerini alır
        /// </summary>
        /// <returns>Tüm istatistikler</returns>
        public static ServiceResult<List<PerformanceStats>> GetAllPerformanceStats()
        {
            var stats = new List<PerformanceStats>();

            foreach (var operation in _measurements.Keys)
            {
                var operationStats = GetPerformanceStats(operation);
                if (operationStats.IsSuccess)
                    stats.Add(operationStats.Data!);
            }

            return ServiceResult<List<PerformanceStats>>.Success(stats);
        }

        /// <summary>
        /// Performance verilerini temizler
        /// </summary>
        public static void ClearPerformanceData()
        {
            _timers.Clear();
            _measurements.Clear();
        }

        /// <summary>
        /// Belirli bir işlemin verilerini temizler
        /// </summary>
        /// <param name="operationName">İşlem adı</param>
        public static void ClearOperationData(string operationName)
        {
            _timers.Remove(operationName);
            _measurements.Remove(operationName);
        }
    }

    /// <summary>
    /// Performance istatistikleri
    /// </summary>
    public class PerformanceStats
    {
        /// <summary>
        /// İşlem adı
        /// </summary>
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// Toplam çalıştırma sayısı
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// Ortalama çalışma süresi (ms)
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// Minimum çalışma süresi (ms)
        /// </summary>
        public long MinExecutionTime { get; set; }

        /// <summary>
        /// Maksimum çalışma süresi (ms)
        /// </summary>
        public long MaxExecutionTime { get; set; }

        /// <summary>
        /// Toplam çalışma süresi (ms)
        /// </summary>
        public long TotalExecutionTime { get; set; }
    }
}
