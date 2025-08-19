using System.Data;
using Microsoft.Data.SqlClient;

namespace WebLibrary.Database
{
    /// <summary>
    /// SQL Server bağlantı fabrikası
    /// </summary>
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlServerConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public string ConnectionString => _connectionString;

        public string ProviderName => "System.Data.SqlClient";
    }
}
