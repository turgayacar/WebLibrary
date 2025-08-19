using System.Data;
using System.Collections.Concurrent;

namespace WebLibrary.Database
{
    /// <summary>
    /// Unit of Work base class'ı
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed;

        public UnitOfWork(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _connectionFactory.CreateConnection();
                    _connection.Open();
                }
                return _connection;
            }
        }

        public IDbTransaction? CurrentTransaction => _transaction;

        public IDbTransaction BeginTransaction()
        {
            if (_transaction != null)
                throw new InvalidOperationException("Zaten aktif bir transaction var");

            _transaction = Connection.BeginTransaction();
            return _transaction;
        }

        public void Commit()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Aktif transaction yok");

            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Aktif transaction yok");

            try
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public IGenericRepository<T, TId> GetRepository<T, TId>() where T : class
        {
            var repositoryType = typeof(IGenericRepository<T, TId>);
            
            if (_repositories.TryGetValue(repositoryType, out var repository))
                return (IGenericRepository<T, TId>)repository;

            // Repository factory pattern kullanarak repository oluştur
            var newRepository = CreateRepository<T, TId>();
            _repositories.TryAdd(repositoryType, newRepository);
            
            return newRepository;
        }

        /// <summary>
        /// Repository oluşturur
        /// </summary>
        /// <typeparam name="T">Entity tipi</typeparam>
        /// <typeparam name="TId">ID tipi</typeparam>
        /// <returns>Repository instance</returns>
        protected virtual IGenericRepository<T, TId> CreateRepository<T, TId>() where T : class
        {
            // ESKİ KOD (YORUM SATIRINDA):
            // throw new NotImplementedException($"Repository tipi {typeof(T).Name} için factory method implement edilmemiş");
            
            // YENİ KOD: Repository Factory kullanarak repository oluştur
            return RepositoryFactory.CreateRepository<T, TId>(_connectionFactory);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}
