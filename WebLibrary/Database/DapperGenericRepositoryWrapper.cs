using System.Data;

namespace WebLibrary.Database
{
    /// <summary>
    /// DapperGenericRepository'yi wrap eden concrete sınıf
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <typeparam name="TId">ID tipi</typeparam>
    public class DapperGenericRepositoryWrapper<T, TId> : DapperGenericRepository<T, TId> where T : class
    {
        public DapperGenericRepositoryWrapper(IDbConnectionFactory connectionFactory) 
            : base(connectionFactory)
        {
        }
    }
}
