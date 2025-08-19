using System.Diagnostics;
using System.Data;
using WebLibrary.Models;

namespace WebLibrary.Testing
{
    /// <summary>
    /// Test base sınıfı - Tüm test sınıfları için ortak setup ve teardown işlevselliği sağlar
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected readonly Stopwatch _stopwatch;
        protected readonly List<IDisposable> _disposables;
        protected readonly Dictionary<string, object> _testData;

        protected TestBase()
        {
            _stopwatch = new Stopwatch();
            _disposables = new List<IDisposable>();
            _testData = new Dictionary<string, object>();
            
            SetupTest();
        }

        /// <summary>
        /// Test setup - Her test öncesi çalışır
        /// </summary>
        protected virtual void SetupTest()
        {
            _stopwatch.Start();
            Console.WriteLine($"🧪 Test başlatılıyor: {GetType().Name}");
        }

        /// <summary>
        /// Test teardown - Her test sonrası çalışır
        /// </summary>
        protected virtual void TeardownTest()
        {
            _stopwatch.Stop();
            Console.WriteLine($"✅ Test tamamlandı: {GetType().Name} - Süre: {_stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Test verisi ekler
        /// </summary>
        /// <param name="key">Veri anahtarı</param>
        /// <param name="value">Veri değeri</param>
        protected void AddTestData(string key, object value)
        {
            _testData[key] = value;
        }

        /// <summary>
        /// Test verisi alır
        /// </summary>
        /// <typeparam name="T">Veri türü</typeparam>
        /// <param name="key">Veri anahtarı</param>
        /// <returns>Veri değeri</returns>
        protected T? GetTestData<T>(string key)
        {
            if (_testData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        /// <summary>
        /// Test verisi var mı kontrol eder
        /// </summary>
        /// <param name="key">Veri anahtarı</param>
        /// <returns>Varsa true, yoksa false</returns>
        protected bool HasTestData(string key)
        {
            return _testData.ContainsKey(key);
        }

        /// <summary>
        /// Test verisi kaldırır
        /// </summary>
        /// <param name="key">Veri anahtarı</param>
        /// <returns>Kaldırıldıysa true, kaldırılamadıysa false</returns>
        protected bool RemoveTestData(string key)
        {
            return _testData.Remove(key);
        }

        /// <summary>
        /// Tüm test verilerini temizler
        /// </summary>
        protected void ClearTestData()
        {
            _testData.Clear();
        }

        /// <summary>
        /// Disposable nesne ekler
        /// </summary>
        /// <param name="disposable">Disposable nesne</param>
        protected void AddDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Test performansını ölçer
        /// </summary>
        /// <param name="action">Ölçülecek aksiyon</param>
        /// <param name="operationName">Operasyon adı</param>
        /// <returns>Geçen süre (milisaniye)</returns>
        protected long MeasurePerformance(Action action, string operationName = "Operation")
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            
            Console.WriteLine($"⏱️ {operationName} süresi: {stopwatch.ElapsedMilliseconds}ms");
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Async test performansını ölçer
        /// </summary>
        /// <param name="asyncAction">Ölçülecek async aksiyon</param>
        /// <param name="operationName">Operasyon adı</param>
        /// <returns>Geçen süre (milisaniye)</returns>
        protected async Task<long> MeasurePerformanceAsync(Func<Task> asyncAction, string operationName = "Async Operation")
        {
            var stopwatch = Stopwatch.StartNew();
            await asyncAction();
            stopwatch.Stop();
            
            Console.WriteLine($"⏱️ {operationName} süresi: {stopwatch.ElapsedMilliseconds}ms");
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Test sonucunu loglar
        /// </summary>
        /// <param name="testName">Test adı</param>
        /// <param name="result">Test sonucu</param>
        /// <param name="duration">Test süresi</param>
        /// <param name="details">Ek detaylar</param>
        protected void LogTestResult(string testName, bool result, long duration, string? details = null)
        {
            var status = result ? "✅ PASS" : "❌ FAIL";
            var logMessage = $"{status} | {testName} | {duration}ms";
            
            if (!string.IsNullOrEmpty(details))
            {
                logMessage += $" | {details}";
            }
            
            Console.WriteLine(logMessage);
        }

        /// <summary>
        /// Test exception'ını loglar
        /// </summary>
        /// <param name="testName">Test adı</param>
        /// <param name="exception">Fırlatılan exception</param>
        protected void LogTestException(string testName, Exception exception)
        {
            Console.WriteLine($"💥 EXCEPTION | {testName} | {exception.GetType().Name}: {exception.Message}");
            
            if (exception.InnerException != null)
            {
                Console.WriteLine($"   Inner Exception: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}");
            }
        }

        /// <summary>
        /// Test verilerini dosyaya kaydeder
        /// </summary>
        /// <param name="filename">Dosya adı</param>
        protected void SaveTestDataToFile(string filename)
        {
            try
            {
                var lines = _testData.Select(kvp => $"{kvp.Key}: {kvp.Value}");
                var content = string.Join(Environment.NewLine, lines);
                File.WriteAllText(filename, content);
                
                Console.WriteLine($"💾 Test verileri kaydedildi: {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Test verileri kaydedilemedi: {ex.Message}");
            }
        }

        /// <summary>
        /// Test verilerini dosyadan yükler
        /// </summary>
        /// <param name="filename">Dosya adı</param>
        protected void LoadTestDataFromFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    var lines = File.ReadAllLines(filename);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            _testData[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                    
                    Console.WriteLine($"📂 Test verileri yüklendi: {filename}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Test verileri yüklenemedi: {ex.Message}");
            }
        }

        /// <summary>
        /// Test ortamını temizler
        /// </summary>
        public virtual void Dispose()
        {
            TeardownTest();
            
            // Tüm disposable nesneleri dispose et
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Disposable dispose hatası: {ex.Message}");
                }
            }
            
            _disposables.Clear();
            _testData.Clear();
            
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Async test base sınıfı - Async test'ler için özel işlevsellik
    /// </summary>
    public abstract class AsyncTestBase : TestBase
    {
        protected readonly CancellationTokenSource _cancellationTokenSource;

        protected AsyncTestBase()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            AddDisposable(_cancellationTokenSource);
        }

        /// <summary>
        /// Test timeout'u ayarlar
        /// </summary>
        /// <param name="timeout">Timeout süresi (milisaniye)</param>
        protected void SetTestTimeout(int timeout)
        {
            _cancellationTokenSource.CancelAfter(timeout);
        }

        /// <summary>
        /// Test timeout'unu iptal eder
        /// </summary>
        protected void CancelTestTimeout()
        {
            _cancellationTokenSource.CancelAfter(Timeout.Infinite);
        }

        /// <summary>
        /// Cancellation token'ı alır
        /// </summary>
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Async test performansını ölçer ve timeout kontrolü yapar
        /// </summary>
        /// <param name="asyncAction">Ölçülecek async aksiyon</param>
        /// <param name="timeout">Timeout süresi (milisaniye)</param>
        /// <param name="operationName">Operasyon adı</param>
        /// <returns>Geçen süre (milisaniye)</returns>
        protected async Task<long> MeasurePerformanceWithTimeoutAsync(Func<Task> asyncAction, int timeout, string operationName = "Async Operation")
        {
            SetTestTimeout(timeout);
            
            try
            {
                return await MeasurePerformanceAsync(asyncAction, operationName);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"⏰ {operationName} timeout: {timeout}ms");
                throw;
            }
        }

        /// <summary>
        /// Test ortamını temizler
        /// </summary>
        public override void Dispose()
        {
            _cancellationTokenSource.Cancel();
            base.Dispose();
        }
    }

    /// <summary>
    /// Database test base sınıfı - Database test'leri için özel işlevsellik
    /// </summary>
    public abstract class DatabaseTestBase : AsyncTestBase
    {
        protected readonly string _connectionString;
        protected readonly bool _useTransaction;

        protected DatabaseTestBase(string connectionString, bool useTransaction = true)
        {
            _connectionString = connectionString;
            _useTransaction = useTransaction;
        }

        /// <summary>
        /// Test veritabanı bağlantısını açar
        /// </summary>
        /// <returns>Database connection</returns>
        protected virtual Microsoft.Data.SqlClient.SqlConnection OpenConnection()
        {
            var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            connection.Open();
            AddDisposable(connection);
            return connection;
        }

        /// <summary>
        /// Test transaction'ını başlatır
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <returns>Database transaction</returns>
        protected virtual Microsoft.Data.SqlClient.SqlTransaction? BeginTransaction(Microsoft.Data.SqlClient.SqlConnection connection)
        {
            if (_useTransaction)
            {
                var transaction = connection.BeginTransaction();
                AddDisposable(transaction);
                return transaction;
            }
            
            return null;
        }

        /// <summary>
        /// Test verilerini veritabanına ekler
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="sql">SQL sorgusu</param>
        /// <param name="parameters">SQL parametreleri</param>
        protected virtual void InsertTestData(Microsoft.Data.SqlClient.SqlConnection connection, string sql, object? parameters = null)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            
            if (parameters != null)
            {
                // Basit parameter mapping - gerçek uygulamada Dapper kullanılabilir
                var properties = parameters.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{property.Name}";
                    parameter.Value = property.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
            
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Test verilerini veritabanından temizler
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="tableName">Tablo adı</param>
        /// <param name="whereClause">WHERE koşulu</param>
        protected virtual void CleanupTestData(Microsoft.Data.SqlClient.SqlConnection connection, string tableName, string? whereClause = null)
        {
            var sql = whereClause != null 
                ? $"DELETE FROM {tableName} WHERE {whereClause}"
                : $"DELETE FROM {tableName}";
                
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
        }
    }
}
