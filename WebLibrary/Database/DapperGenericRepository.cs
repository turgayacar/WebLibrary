using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using WebLibrary.Models;

namespace WebLibrary.Database
{
    /// <summary>
    /// Dapper ile kullanılabilecek generic repository base class'ı
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <typeparam name="TId">ID tipi</typeparam>
    public abstract class DapperGenericRepository<T, TId> : IGenericRepository<T, TId> where T : class
    {
        protected readonly IDbConnectionFactory _connectionFactory;
        protected readonly string _tableName;
        protected readonly string _idColumnName;
        protected readonly PropertyInfo[] _properties;

        protected DapperGenericRepository(IDbConnectionFactory connectionFactory, string? tableName = null, string? idColumnName = null)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _tableName = tableName ?? GetTableName();
            _idColumnName = idColumnName ?? GetIdColumnName();
            _properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite).ToArray();
        }

        /// <summary>
        /// Tablo adını alır
        /// </summary>
        /// <returns>Tablo adı</returns>
        protected virtual string GetTableName()
        {
            var type = typeof(T);
            var tableAttribute = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
            return tableAttribute?.Name ?? type.Name;
        }

        /// <summary>
        /// ID sütun adını alır
        /// </summary>
        /// <returns>ID sütun adı</returns>
        protected virtual string GetIdColumnName()
        {
            var idProperty = _properties.FirstOrDefault(p => 
                p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null);
            return idProperty?.Name ?? "Id";
        }

        /// <summary>
        /// Tüm kayıtları getirir
        /// </summary>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<IEnumerable<T>>> GetAllAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = $"SELECT * FROM {_tableName}";
                var result = await connection.QueryAsync<T>(sql);
                return ServiceResult<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<T>>.Error($"Veri getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ID'ye göre kayıt getirir
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<T?>> GetByIdAsync(TId id)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = $"SELECT * FROM {_tableName} WHERE {_idColumnName} = @Id";
                var result = await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
                return ServiceResult<T?>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<T?>.Error($"ID ile veri getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Yeni kayıt ekler
        /// </summary>
        /// <param name="entity">Eklenecek entity</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<T>> AddAsync(T entity)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var (insertSql, parameters) = BuildInsertQuery(entity);
                var result = await connection.ExecuteAsync(insertSql, parameters);
                
                if (result > 0)
                    return ServiceResult<T>.Success(entity);
                else
                    return ServiceResult<T>.Error("Kayıt eklenemedi");
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"Kayıt ekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Kayıt günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<bool>> UpdateAsync(T entity)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var (updateSql, parameters) = BuildUpdateQuery(entity);
                var result = await connection.ExecuteAsync(updateSql, parameters);
                
                if (result > 0)
                    return ServiceResult<bool>.Success(true);
                else
                    return ServiceResult<bool>.Error("Kayıt güncellenemedi");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Kayıt güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Kayıt siler
        /// </summary>
        /// <param name="id">Silinecek kaydın ID'si</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<bool>> DeleteAsync(TId id)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = $"DELETE FROM {_tableName} WHERE {_idColumnName} = @Id";
                var result = await connection.ExecuteAsync(sql, new { Id = id });
                
                if (result > 0)
                    return ServiceResult<bool>.Success(true);
                else
                    return ServiceResult<bool>.Error("Kayıt silinemedi");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Kayıt silme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Kayıt var mı kontrol eder
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<bool>> ExistsAsync(TId id)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE {_idColumnName} = @Id";
                var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
                return ServiceResult<bool>.Success(count > 0);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Varlık kontrolü hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplam kayıt sayısını getirir
        /// </summary>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<int>> GetCountAsync()
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var sql = $"SELECT COUNT(*) FROM {_tableName}";
                var count = await connection.ExecuteScalarAsync<int>(sql);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"Sayım hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Sayfalı veri getirir
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<IEnumerable<T>>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var offset = (pageNumber - 1) * pageSize;
                var sql = $"SELECT * FROM {_tableName} ORDER BY {_idColumnName} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                var result = await connection.QueryAsync<T>(sql, new { Offset = offset, PageSize = pageSize });
                return ServiceResult<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<T>>.Error($"Sayfalı veri getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Koşula göre veri getirir
        /// </summary>
        /// <param name="predicate">Koşul</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<IEnumerable<T>>> GetWhereAsync(Func<T, bool> predicate)
        {
            try
            {
                var allData = await GetAllAsync();
                if (!allData.IsSuccess)
                    return ServiceResult<IEnumerable<T>>.Error(allData.ErrorMessages);

                var filteredData = allData.Data.Where(predicate);
                return ServiceResult<IEnumerable<T>>.Success(filteredData);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<T>>.Error($"Koşullu veri getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// İlk eşleşen kaydı getirir
        /// </summary>
        /// <param name="predicate">Koşul</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<T?>> GetFirstOrDefaultAsync(Func<T, bool> predicate)
        {
            try
            {
                var allData = await GetAllAsync();
                if (!allData.IsSuccess)
                    return ServiceResult<T?>.Error(allData.ErrorMessages);

                var result = allData.Data.FirstOrDefault(predicate);
                return ServiceResult<T?>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<T?>.Error($"İlk eşleşen kayıt getirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu kayıt ekler
        /// </summary>
        /// <param name="entities">Eklenecek entity'ler</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<bool>> AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var transaction = connection.BeginTransaction();
                
                try
                {
                    foreach (var entity in entities)
                    {
                        var (insertSql, parameters) = BuildInsertQuery(entity);
                        await connection.ExecuteAsync(insertSql, parameters, transaction);
                    }
                    
                    transaction.Commit();
                    return ServiceResult<bool>.Success(true);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Toplu kayıt ekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu kayıt günceller
        /// </summary>
        /// <param name="entities">Güncellenecek entity'ler</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<bool>> UpdateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var transaction = connection.BeginTransaction();
                
                try
                {
                    foreach (var entity in entities)
                    {
                        var (updateSql, parameters) = BuildUpdateQuery(entity);
                        await connection.ExecuteAsync(updateSql, parameters, transaction);
                    }
                    
                    transaction.Commit();
                    return ServiceResult<bool>.Success(true);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Toplu kayıt güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu kayıt siler
        /// </summary>
        /// <param name="entities">Silinecek entity'ler</param>
        /// <returns>ServiceResult</returns>
        public virtual async Task<ServiceResult<bool>> DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var transaction = connection.BeginTransaction();
                
                try
                {
                    foreach (var entity in entities)
                    {
                        var id = GetEntityId(entity);
                        var sql = $"DELETE FROM {_tableName} WHERE {_idColumnName} = @Id";
                        await connection.ExecuteAsync(sql, new { Id = id }, transaction);
                    }
                    
                    transaction.Commit();
                    return ServiceResult<bool>.Success(true);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Toplu kayıt silme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// INSERT sorgusu oluşturur
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>(SQL, Parameters)</returns>
        protected virtual (string sql, object parameters) BuildInsertQuery(T entity)
        {
            var columns = _properties.Where(p => !p.Name.Equals(_idColumnName, StringComparison.OrdinalIgnoreCase))
                                   .Select(p => p.Name)
                                   .ToArray();
            
            var values = columns.Select(c => $"@{c}").ToArray();
            
            var sql = $"INSERT INTO {_tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            
            var parameters = new DynamicParameters();
            foreach (var column in columns)
            {
                var property = _properties.First(p => p.Name.Equals(column, StringComparison.OrdinalIgnoreCase));
                var value = property.GetValue(entity);
                parameters.Add(column, value);
            }
            
            return (sql, parameters);
        }

        /// <summary>
        /// UPDATE sorgusu oluşturur
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>(SQL, Parameters)</returns>
        protected virtual (string sql, object parameters) BuildUpdateQuery(T entity)
        {
            var columns = _properties.Where(p => !p.Name.Equals(_idColumnName, StringComparison.OrdinalIgnoreCase))
                                   .Select(p => $"{p.Name} = @{p.Name}")
                                   .ToArray();
            
            var sql = $"UPDATE {_tableName} SET {string.Join(", ", columns)} WHERE {_idColumnName} = @{_idColumnName}";
            
            var parameters = new DynamicParameters();
            foreach (var property in _properties)
            {
                var value = property.GetValue(entity);
                parameters.Add(property.Name, value);
            }
            
            return (sql, parameters);
        }

        /// <summary>
        /// Entity'nin ID'sini alır
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>ID değeri</returns>
        protected virtual TId GetEntityId(T entity)
        {
            var idProperty = _properties.First(p => p.Name.Equals(_idColumnName, StringComparison.OrdinalIgnoreCase));
            return (TId)idProperty.GetValue(entity)!;
        }

        /// <summary>
        /// Özel SQL sorgusu çalıştırır
        /// </summary>
        /// <param name="sql">SQL sorgusu</param>
        /// <param name="parameters">Parametreler</param>
        /// <returns>ServiceResult</returns>
        protected async Task<ServiceResult<IEnumerable<T>>> ExecuteQueryAsync(string sql, object? parameters = null)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var result = await connection.QueryAsync<T>(sql, parameters);
                return ServiceResult<IEnumerable<T>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<T>>.Error($"Sorgu çalıştırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Özel SQL sorgusu çalıştırır (tek kayıt)
        /// </summary>
        /// <param name="sql">SQL sorgusu</param>
        /// <param name="parameters">Parametreler</param>
        /// <returns>ServiceResult</returns>
        protected async Task<ServiceResult<T?>> ExecuteQueryFirstOrDefaultAsync(string sql, object? parameters = null)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
                return ServiceResult<T?>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<T?>.Error($"Sorgu çalıştırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Özel SQL komutu çalıştırır
        /// </summary>
        /// <param name="sql">SQL komutu</param>
        /// <param name="parameters">Parametreler</param>
        /// <returns>ServiceResult</returns>
        protected async Task<ServiceResult<int>> ExecuteCommandAsync(string sql, object? parameters = null)
        {
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                var result = await connection.ExecuteAsync(sql, parameters);
                return ServiceResult<int>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"Komut çalıştırma hatası: {ex.Message}");
            }
        }
    }
}
