using WebLibrary.Models;
using Dapper;

namespace WebLibrary.Database
{
    /// <summary>
    /// User entity'si için özel repository
    /// </summary>
    public class UserRepository : DapperGenericRepository<User, int>
    {
        public UserRepository(IDbConnectionFactory connectionFactory) 
            : base(connectionFactory, "Users", "Id")
        {
        }
        
        /// <summary>
        /// Email'e göre user getirir
        /// </summary>
        /// <param name="email">Email adresi</param>
        /// <returns>User veya null</returns>
        public async Task<ServiceResult<User?>> GetByEmailAsync(string email)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = "SELECT * FROM Users WHERE Email = @Email";
                var result = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
                return ServiceResult<User?>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<User?>.Error($"Email ile user getirme hatası: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Aktif user'ları getirir
        /// </summary>
        /// <returns>Aktif user listesi</returns>
        public async Task<ServiceResult<IEnumerable<User>>> GetActiveUsersAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = "SELECT * FROM Users WHERE IsActive = 1 ORDER BY CreatedDate DESC";
                var result = await connection.QueryAsync<User>(sql);
                return ServiceResult<IEnumerable<User>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<User>>.Error($"Aktif user'ları getirme hatası: {ex.Message}");
            }
        }
    }
}
