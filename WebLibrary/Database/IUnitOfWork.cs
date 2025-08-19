using System.Data;

namespace WebLibrary.Database
{
    /// <summary>
    /// Unit of Work interface'i
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Veritabanı bağlantısını alır
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Transaction'ı başlatır
        /// </summary>
        /// <returns>Transaction</returns>
        IDbTransaction BeginTransaction();

        /// <summary>
        /// Mevcut transaction'ı alır
        /// </summary>
        IDbTransaction? CurrentTransaction { get; }

        /// <summary>
        /// Transaction'ı commit eder
        /// </summary>
        void Commit();

        /// <summary>
        /// Transaction'ı rollback eder
        /// </summary>
        void Rollback();

        /// <summary>
        /// Repository'yi alır
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <typeparam name="TId">ID tipi</typeparam>
        /// <returns>Repository instance</returns>
        IGenericRepository<T, TId> GetRepository<T, TId>() where T : class;
    }
}
