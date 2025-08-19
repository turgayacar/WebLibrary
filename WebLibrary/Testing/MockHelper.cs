using Moq;
using Moq.Language.Flow;
using System.Linq.Expressions;
using WebLibrary.Models;

namespace WebLibrary.Testing
{
    /// <summary>
    /// Mock nesne oluşturma yardımcısı - Moq kütüphanesi kullanarak mock nesneler oluşturur ve yapılandırır
    /// </summary>
    public static class MockHelper
    {
        /// <summary>
        /// Belirtilen türde mock nesne oluşturur
        /// </summary>
        /// <typeparam name="T">Mock oluşturulacak tür</typeparam>
        /// <returns>Mock nesne</returns>
        public static Mock<T> CreateMock<T>() where T : class
        {
            return new Mock<T>();
        }

        /// <summary>
        /// Belirtilen türde mock nesne oluşturur ve davranışını yapılandırır
        /// </summary>
        /// <typeparam name="T">Mock oluşturulacak tür</typeparam>
        /// <param name="setupAction">Mock yapılandırma aksiyonu</param>
        /// <returns>Yapılandırılmış mock nesne</returns>
        public static Mock<T> CreateMock<T>(Action<Mock<T>> setupAction) where T : class
        {
            var mock = new Mock<T>();
            setupAction?.Invoke(mock);
            return mock;
        }

        /// <summary>
        /// Interface için mock nesne oluşturur
        /// </summary>
        /// <typeparam name="T">Mock oluşturulacak interface</typeparam>
        /// <returns>Interface mock nesnesi</returns>
        public static T CreateMockInterface<T>() where T : class
        {
            return new Mock<T>().Object;
        }

        /// <summary>
        /// Interface için mock nesne oluşturur ve davranışını yapılandırır
        /// </summary>
        /// <typeparam name="T">Mock oluşturulacak interface</typeparam>
        /// <param name="setupAction">Mock yapılandırma aksiyonu</param>
        /// <returns>Yapılandırılmış interface mock nesnesi</returns>
        public static T CreateMockInterface<T>(Action<Mock<T>> setupAction) where T : class
        {
            var mock = new Mock<T>();
            setupAction?.Invoke(mock);
            return mock.Object;
        }

        /// <summary>
        /// Generic repository mock'u oluşturur
        /// </summary>
        /// <typeparam name="T">Repository türü</typeparam>
        /// <returns>Generic repository mock'u</returns>
        public static Mock<IGenericRepository<T>> CreateRepositoryMock<T>() where T : class
        {
            return new Mock<IGenericRepository<T>>();
        }

        /// <summary>
        /// Generic repository mock'u oluşturur ve temel CRUD operasyonlarını yapılandırır
        /// </summary>
        /// <typeparam name="T">Repository türü</typeparam>
        /// <param name="testData">Test verisi</param>
        /// <returns>Yapılandırılmış repository mock'u</returns>
        public static Mock<IGenericRepository<T>> CreateRepositoryMock<T>(List<T> testData) where T : class
        {
            var mock = new Mock<IGenericRepository<T>>();
            
            // GetAllAsync
            mock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(testData);
            
            // GetByIdAsync
            mock.Setup(r => r.GetByIdAsync(It.IsAny<object>()))
                .ReturnsAsync((object id) => testData.FirstOrDefault());
            
            // CreateAsync
            mock.Setup(r => r.CreateAsync(It.IsAny<T>()))
                .ReturnsAsync((T entity) => entity);
            
            // UpdateAsync
            mock.Setup(r => r.UpdateAsync(It.IsAny<T>()))
                .ReturnsAsync((T entity) => entity);
            
            // DeleteAsync
            mock.Setup(r => r.DeleteAsync(It.IsAny<object>()))
                .ReturnsAsync(true);
            
            // CountAsync
            mock.Setup(r => r.CountAsync())
                .ReturnsAsync(testData.Count);
            
            return mock;
        }

        /// <summary>
        /// HTTP Client mock'u oluşturur
        /// </summary>
        /// <returns>HTTP Client mock'u</returns>
        public static Mock<HttpClient> CreateHttpClientMock()
        {
            return new Mock<HttpClient>();
        }

        /// <summary>
        /// HTTP Client mock'u oluşturur ve belirli URL'ler için yanıt yapılandırır
        /// </summary>
        /// <param name="urlResponses">URL-yanıt çiftleri</param>
        /// <returns>Yapılandırılmış HTTP Client mock'u</returns>
        public static Mock<HttpClient> CreateHttpClientMock(Dictionary<string, string> urlResponses)
        {
            var mock = new Mock<HttpClient>();
            
            foreach (var kvp in urlResponses)
            {
                var response = new HttpResponseMessage
                {
                    Content = new StringContent(kvp.Value),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
                
                // Bu basit bir mock - gerçek uygulamada HttpMessageHandler kullanılmalı
                // mock.Setup(x => x.GetAsync(kvp.Key)).ReturnsAsync(response);
            }
            
            return mock;
        }

        /// <summary>
        /// Logger mock'u oluşturur
        /// </summary>
        /// <typeparam name="T">Logger kategorisi</typeparam>
        /// <returns>Logger mock'u</returns>
        public static Mock<ILogger<T>> CreateLoggerMock<T>()
        {
            var mock = new Mock<ILogger<T>>();
            
            // Log seviyeleri için setup
            mock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ));
            
            return mock;
        }

        /// <summary>
        /// Configuration mock'u oluşturur
        /// </summary>
        /// <returns>Configuration mock'u</returns>
        public static Mock<IConfiguration> CreateConfigurationMock()
        {
            var mock = new Mock<IConfiguration>();
            return mock;
        }

        /// <summary>
        /// Configuration mock'u oluşturur ve belirli anahtarlar için değerler yapılandırır
        /// </summary>
        /// <param name="configurationValues">Anahtar-değer çiftleri</param>
        /// <returns>Yapılandırılmış configuration mock'u</returns>
        public static Mock<IConfiguration> CreateConfigurationMock(Dictionary<string, string> configurationValues)
        {
            var mock = new Mock<IConfiguration>();
            
            foreach (var kvp in configurationValues)
            {
                var sectionMock = new Mock<IConfigurationSection>();
                sectionMock.Setup(x => x.Value).Returns(kvp.Value);
                
                mock.Setup(x => x.GetSection(kvp.Key)).Returns(sectionMock.Object);
                mock.Setup(x => x[kvp.Key]).Returns(kvp.Value);
            }
            
            return mock;
        }

        /// <summary>
        /// Service Provider mock'u oluşturur
        /// </summary>
        /// <returns>Service Provider mock'u</returns>
        public static Mock<IServiceProvider> CreateServiceProviderMock()
        {
            return new Mock<IServiceProvider>();
        }

        /// <summary>
        /// Service Provider mock'u oluşturur ve belirli türler için servisler yapılandırır
        /// </summary>
        /// <param name="services">Tür-servis çiftleri</param>
        /// <returns>Yapılandırılmış service provider mock'u</returns>
        public static Mock<IServiceProvider> CreateServiceProviderMock(Dictionary<Type, object> services)
        {
            var mock = new Mock<IServiceProvider>();
            
            foreach (var kvp in services)
            {
                mock.Setup(x => x.GetService(kvp.Key)).Returns(kvp.Value);
            }
            
            return mock;
        }

        /// <summary>
        /// Unit of Work mock'u oluşturur
        /// </summary>
        /// <returns>Unit of Work mock'u</returns>
        public static Mock<IUnitOfWork> CreateUnitOfWorkMock()
        {
            var mock = new Mock<IUnitOfWork>();
            
            // Temel UoW operasyonları
            mock.Setup(x => x.BeginTransactionAsync()).ReturnsAsync(new Mock<IDbTransaction>().Object);
            mock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            mock.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
            mock.Setup(x => x.Dispose()).Verifiable();
            
            return mock;
        }

        /// <summary>
        /// Database Connection mock'u oluşturur
        /// </summary>
        /// <returns>Database Connection mock'u</returns>
        public static Mock<IDbConnection> CreateDbConnectionMock()
        {
            var mock = new Mock<IDbConnection>();
            
            // Temel connection operasyonları
            mock.Setup(x => x.Open()).Verifiable();
            mock.Setup(x => x.Close()).Verifiable();
            mock.Setup(x => x.Dispose()).Verifiable();
            
            return mock;
        }

        /// <summary>
        /// Mock nesnenin belirli metodunun çağrılıp çağrılmadığını doğrular
        /// </summary>
        /// <typeparam name="T">Mock türü</typeparam>
        /// <param name="mock">Mock nesne</param>
        /// <param name="expression">Doğrulanacak ifade</param>
        /// <param name="times">Kaç kez çağrılması gerektiği</param>
        public static void VerifyCall<T>(Mock<T> mock, Expression<Action<T>> expression, Times times) where T : class
        {
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Mock nesnenin belirli metodunun belirtilen parametrelerle çağrılıp çağrılmadığını doğrular
        /// </summary>
        /// <typeparam name="T">Mock türü</typeparam>
        /// <param name="mock">Mock nesne</param>
        /// <param name="expression">Doğrulanacak ifade</param>
        /// <param name="times">Kaç kez çağrılması gerektiği</param>
        public static void VerifyCall<T>(Mock<T> mock, Expression<Func<T, object>> expression, Times times) where T : class
        {
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Mock nesnenin belirli metodunun hiç çağrılmadığını doğrular
        /// </summary>
        /// <typeparam name="T">Mock türü</typeparam>
        /// <param name="mock">Mock nesne</param>
        /// <param name="expression">Doğrulanacak ifade</param>
        public static void VerifyNeverCalled<T>(Mock<T> mock, Expression<Action<T>> expression) where T : class
        {
            mock.Verify(expression, Times.Never);
        }

        /// <summary>
        /// Mock nesnenin belirli metodunun sadece bir kez çağrıldığını doğrular
        /// </summary>
        /// <typeparam name="T">Mock türü</typeparam>
        /// <param name="mock">Mock nesne</param>
        /// <param name="expression">Doğrulanacak ifade</param>
        public static void VerifyCalledOnce<T>(Mock<T> mock, Expression<Action<T>> expression) where T : class
        {
            mock.Verify(expression, Times.Once);
        }

        /// <summary>
        /// Mock nesnenin belirli metodunun belirtilen sayıda çağrıldığını doğrular
        /// </summary>
        /// <typeparam name="T">Mock türü</typeparam>
        /// <param name="mock">Mock nesne</param>
        /// <param name="expression">Doğrulanacak ifade</param>
        /// <param name="count">Kaç kez çağrılması gerektiği</param>
        public static void VerifyCalledTimes<T>(Mock<T> mock, Expression<Action<T>> expression, int count) where T : class
        {
            mock.Verify(expression, Times.Exactly(count));
        }
    }

    #region Interface Definitions

    /// <summary>
    /// Generic repository interface'i
    /// </summary>
    /// <typeparam name="T">Entity türü</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(object id);
        Task<int> CountAsync();
    }

    /// <summary>
    /// Unit of Work interface'i
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<IDbTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }

    /// <summary>
    /// Logger interface'i
    /// </summary>
    /// <typeparam name="T">Logger kategorisi</typeparam>
    public interface ILogger<T>
    {
        void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
        bool IsEnabled(LogLevel logLevel);
        IDisposable? BeginScope<TState>(TState state) where TState : notnull;
    }

    /// <summary>
    /// Configuration interface'i
    /// </summary>
    public interface IConfiguration
    {
        string? this[string key] { get; set; }
        IConfigurationSection GetSection(string key);
    }

    /// <summary>
    /// Configuration Section interface'i
    /// </summary>
    public interface IConfigurationSection : IConfiguration
    {
        string Key { get; }
        string? Value { get; set; }
        string Path { get; }
    }

    /// <summary>
    /// Service Provider interface'i
    /// </summary>
    public interface IServiceProvider
    {
        object? GetService(Type serviceType);
    }

    /// <summary>
    /// Database Connection interface'i
    /// </summary>
    public interface IDbConnection : IDisposable
    {
        void Open();
        void Close();
    }

    /// <summary>
    /// Database Transaction interface'i
    /// </summary>
    public interface IDbTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }

    #endregion

    #region Enums

    /// <summary>
    /// Log seviyeleri
    /// </summary>
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    /// <summary>
    /// Event ID
    /// </summary>
    public struct EventId
    {
        public int Id { get; }
        public string? Name { get; }

        public EventId(int id, string? name = null)
        {
            Id = id;
            Name = name;
        }
    }

    #endregion
}
