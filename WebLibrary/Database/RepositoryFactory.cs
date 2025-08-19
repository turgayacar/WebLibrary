using System.Data;
using WebLibrary.Models;

namespace WebLibrary.Database
{
    /// <summary>
    /// Repository factory sınıfı - Generic repository'leri oluşturur
    /// </summary>
    public static class RepositoryFactory
    {
        /// <summary>
        /// Generic repository oluşturur
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <typeparam name="TId">ID tipi</typeparam>
        /// <param name="connectionFactory">Veritabanı bağlantı fabrikası</param>
        /// <returns>Generic repository instance</returns>
        public static IGenericRepository<T, TId> CreateRepository<T, TId>(IDbConnectionFactory connectionFactory) where T : class
        {
            // Entity tipine göre özel repository oluştur
            if (typeof(T) == typeof(User))
            {
                return (IGenericRepository<T, TId>)new UserRepository(connectionFactory);
            }
            
            // Genel durum için DapperGenericRepositoryWrapper kullan
            return new DapperGenericRepositoryWrapper<T, TId>(connectionFactory);
        }
    }
}
