using System.Collections.Concurrent;
using WebLibrary.Models;

namespace WebLibrary.Logging
{
    /// <summary>
    /// Metrics collection için helper sınıfı
    /// </summary>
    public static class MetricsCollectionHelper
    {
        private static readonly ConcurrentDictionary<string, MetricCounter> _counters = new();
        private static readonly ConcurrentDictionary<string, MetricGauge> _gauges = new();
        private static readonly ConcurrentDictionary<string, MetricHistogram> _histograms = new();

        /// <summary>
        /// Counter metric oluşturur veya günceller
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <param name="value">Artırılacak değer</param>
        /// <param name="tags">Etiketler</param>
        public static void IncrementCounter(string name, long value = 1, Dictionary<string, string>? tags = null)
        {
            var counter = _counters.GetOrAdd(name, _ => new MetricCounter { Name = name, Tags = tags ?? new Dictionary<string, string>() });
            counter.Increment(value);
        }

        /// <summary>
        /// Counter metric değerini alır
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <returns>Counter değeri</returns>
        public static ServiceResult<MetricCounter> GetCounter(string name)
        {
            if (_counters.TryGetValue(name, out var counter))
                return ServiceResult<MetricCounter>.Success(counter);

            return ServiceResult<MetricCounter>.Error($"Counter bulunamadı: {name}");
        }

        /// <summary>
        /// Gauge metric oluşturur veya günceller
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <param name="value">Yeni değer</param>
        /// <param name="tags">Etiketler</param>
        public static void SetGauge(string name, double value, Dictionary<string, string>? tags = null)
        {
            var gauge = _gauges.GetOrAdd(name, _ => new MetricGauge { Name = name, Tags = tags ?? new Dictionary<string, string>() });
            gauge.SetValue(value);
        }

        /// <summary>
        /// Gauge metric değerini alır
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <returns>Gauge değeri</returns>
        public static ServiceResult<MetricGauge> GetGauge(string name)
        {
            if (_gauges.TryGetValue(name, out var gauge))
                return ServiceResult<MetricGauge>.Success(gauge);

            return ServiceResult<MetricGauge>.Error($"Gauge bulunamadı: {name}");
        }

        /// <summary>
        /// Histogram metric oluşturur veya günceller
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <param name="value">Eklenen değer</param>
        /// <param name="tags">Etiketler</param>
        public static void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null)
        {
            var histogram = _histograms.GetOrAdd(name, _ => new MetricHistogram { Name = name, Tags = tags ?? new Dictionary<string, string>() });
            histogram.RecordValue(value);
        }

        /// <summary>
        /// Histogram metric değerini alır
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <returns>Histogram değeri</returns>
        public static ServiceResult<MetricHistogram> GetHistogram(string name)
        {
            if (_histograms.TryGetValue(name, out var histogram))
                return ServiceResult<MetricHistogram>.Success(histogram);

            return ServiceResult<MetricHistogram>.Error($"Histogram bulunamadı: {name}");
        }

        /// <summary>
        /// Tüm counter'ları alır
        /// </summary>
        /// <returns>Tüm counter'lar</returns>
        public static ServiceResult<List<MetricCounter>> GetAllCounters()
        {
            var counters = _counters.Values.ToList();
            return ServiceResult<List<MetricCounter>>.Success(counters);
        }

        /// <summary>
        /// Tüm gauge'ları alır
        /// </summary>
        /// <returns>Tüm gauge'lar</returns>
        public static ServiceResult<List<MetricGauge>> GetAllGauges()
        {
            var gauges = _gauges.Values.ToList();
            return ServiceResult<List<MetricGauge>>.Success(gauges);
        }

        /// <summary>
        /// Tüm histogram'ları alır
        /// </summary>
        /// <returns>Tüm histogram'lar</returns>
        public static ServiceResult<List<MetricHistogram>> GetAllHistograms()
        {
            var histograms = _histograms.Values.ToList();
            return ServiceResult<List<MetricHistogram>>.Success(histograms);
        }

        /// <summary>
        /// Tüm metrics'leri alır
        /// </summary>
        /// <returns>Tüm metrics'ler</returns>
        public static ServiceResult<AllMetrics> GetAllMetrics()
        {
            var allMetrics = new AllMetrics
            {
                Counters = _counters.Values.ToList(),
                Gauges = _gauges.Values.ToList(),
                Histograms = _histograms.Values.ToList(),
                Timestamp = DateTime.UtcNow
            };

            return ServiceResult<AllMetrics>.Success(allMetrics);
        }

        /// <summary>
        /// Belirli bir metric'i temizler
        /// </summary>
        /// <param name="name">Metric adı</param>
        /// <param name="type">Metric tipi</param>
        public static void ClearMetric(string name, MetricType type)
        {
            switch (type)
            {
                case MetricType.Counter:
                    _counters.TryRemove(name, out _);
                    break;
                case MetricType.Gauge:
                    _gauges.TryRemove(name, out _);
                    break;
                case MetricType.Histogram:
                    _histograms.TryRemove(name, out _);
                    break;
            }
        }

        /// <summary>
        /// Tüm metrics'leri temizler
        /// </summary>
        public static void ClearAllMetrics()
        {
            _counters.Clear();
            _gauges.Clear();
            _histograms.Clear();
        }

        /// <summary>
        /// Metrics'i JSON formatında export eder
        /// </summary>
        /// <returns>JSON formatında metrics</returns>
        public static ServiceResult<string> ExportMetricsAsJson()
        {
            try
            {
                var allMetrics = GetAllMetrics();
                if (!allMetrics.IsSuccess)
                    return ServiceResult<string>.Error("Metrics alınamadı");

                var json = System.Text.Json.JsonSerializer.Serialize(allMetrics.Data, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return ServiceResult<string>.Success(json);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Metrics export hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Metric tipi
    /// </summary>
    public enum MetricType
    {
        Counter,
        Gauge,
        Histogram
    }

    /// <summary>
    /// Base metric sınıfı
    /// </summary>
    public abstract class MetricBase
    {
        /// <summary>
        /// Metric adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Etiketler
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new();

        /// <summary>
        /// Son güncelleme zamanı
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Counter metric
    /// </summary>
    public class MetricCounter : MetricBase
    {
        /// <summary>
        /// Mevcut değer
        /// </summary>
        public long Value { get; private set; }

        /// <summary>
        /// Değeri artırır
        /// </summary>
        /// <param name="increment">Artırılacak değer</param>
        public void Increment(long increment = 1)
        {
            Value += increment;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Değeri sıfırlar
        /// </summary>
        public void Reset()
        {
            Value = 0;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gauge metric
    /// </summary>
    public class MetricGauge : MetricBase
    {
        /// <summary>
        /// Mevcut değer
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// Değeri ayarlar
        /// </summary>
        /// <param name="value">Yeni değer</param>
        public void SetValue(double value)
        {
            Value = value;
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Histogram metric
    /// </summary>
    public class MetricHistogram : MetricBase
    {
        private readonly List<double> _values = new();

        /// <summary>
        /// Tüm değerler
        /// </summary>
        public IReadOnlyList<double> Values => _values.AsReadOnly();

        /// <summary>
        /// Minimum değer
        /// </summary>
        public double Min => _values.Any() ? _values.Min() : 0;

        /// <summary>
        /// Maksimum değer
        /// </summary>
        public double Max => _values.Any() ? _values.Max() : 0;

        /// <summary>
        /// Ortalama değer
        /// </summary>
        public double Average => _values.Any() ? _values.Average() : 0;

        /// <summary>
        /// Toplam değer
        /// </summary>
        public double Sum => _values.Sum();

        /// <summary>
        /// Değer sayısı
        /// </summary>
        public int Count => _values.Count;

        /// <summary>
        /// Yeni değer ekler
        /// </summary>
        /// <param name="value">Eklenen değer</param>
        public void RecordValue(double value)
        {
            _values.Add(value);
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Histogram'ı temizler
        /// </summary>
        public void Clear()
        {
            _values.Clear();
            LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tüm metrics'leri içeren sınıf
    /// </summary>
    public class AllMetrics
    {
        /// <summary>
        /// Counter'lar
        /// </summary>
        public List<MetricCounter> Counters { get; set; } = new();

        /// <summary>
        /// Gauge'lar
        /// </summary>
        public List<MetricGauge> Gauges { get; set; } = new();

        /// <summary>
        /// Histogram'lar
        /// </summary>
        public List<MetricHistogram> Histograms { get; set; } = new();

        /// <summary>
        /// Zaman damgası
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
