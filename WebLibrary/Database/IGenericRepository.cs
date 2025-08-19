using WebLibrary.Models;

namespace WebLibrary.Database
{
    /// <summary>
    /// Generic repository interface'i
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <typeparam name="TId">ID tipi</typeparam>
    public interface IGenericRepository<T, TId> where T : class
    {
        /// <summary>
        /// Tüm kayıtları getirir
        /// </summary>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<IEnumerable<T>>> GetAllAsync();

        /// <summary>
        /// ID'ye göre kayıt getirir
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<T?>> GetByIdAsync(TId id);

        /// <summary>
        /// Yeni kayıt ekler
        /// </summary>
        /// <param name="entity">Eklenecek entity</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<T>> AddAsync(T entity);

        /// <summary>
        /// Kayıt günceller
        /// </summary>
        /// <param name="entity">Güncellenecek entity</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<bool>> UpdateAsync(T entity);

        /// <summary>
        /// Kayıt siler
        /// </summary>
        /// <param name="id">Silinecek kaydın ID'si</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<bool>> DeleteAsync(TId id);

        /// <summary>
        /// Kayıt var mı kontrol eder
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<bool>> ExistsAsync(TId id);

        /// <summary>
        /// Toplam kayıt sayısını getirir
        /// </summary>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<int>> GetCountAsync();

        /// <summary>
        /// Sayfalı veri getirir
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<IEnumerable<T>>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Koşula göre veri getirir
        /// </summary>
        /// <param name="predicate">Koşul</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<IEnumerable<T>>> GetWhereAsync(Func<T, bool> predicate);

        /// <summary>
        /// İlk eşleşen kaydı getirir
        /// </summary>
        /// <param name="predicate">Koşul</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<T?>> GetFirstOrDefaultAsync(Func<T, bool> predicate);

        /// <summary>
        /// Toplu kayıt ekler
        /// </summary>
        /// <param name="entities">Eklenecek entity'ler</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<bool>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Toplu kayıt günceller
        /// </summary>
        /// <param name="entities">Güncellenecek entity'ler</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<bool>> UpdateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Toplu kayıt siler
        /// </summary>
        /// <param name="entities">Silinecek entity'ler</param>
        /// <returns>ServiceResult</returns>
        Task<ServiceResult<bool>> DeleteRangeAsync(IEnumerable<T> entities);
    }
}
