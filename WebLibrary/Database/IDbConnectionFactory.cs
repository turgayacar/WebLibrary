using System.Data;

namespace WebLibrary.Database
{
    /// <summary>
    /// Veritabanı bağlantı fabrikası interface'i
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Yeni bir veritabanı bağlantısı oluşturur
        /// </summary>
        /// <returns>Veritabanı bağlantısı</returns>
        IDbConnection CreateConnection();

        /// <summary>
        /// Bağlantı string'ini alır
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Veritabanı sağlayıcısını alır
        /// </summary>
        string ProviderName { get; }
    }
}
