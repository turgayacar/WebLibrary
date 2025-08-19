using WebLibrary.Models;

namespace WebLibrary.Services
{
    /// <summary>
    /// Generic servis base sınıfı
    /// </summary>
    /// <typeparam name="T">Model tipi</typeparam>
    public abstract class GenericServiceBase<T> where T : class
    {
        /// <summary>
        /// Tüm kayıtları getirir
        /// </summary>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetAllAsync();
        
        /// <summary>
        /// ID'ye göre kayıt getirir
        /// </summary>
        /// <param name="id">Kayıt ID'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<T>> GetByIdAsync(int id);
        
        /// <summary>
        /// Yeni kayıt ekler
        /// </summary>
        /// <param name="entity">Eklenecek kayıt</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<T>> AddAsync(T entity);
        
        /// <summary>
        /// Kayıt günceller
        /// </summary>
        /// <param name="entity">Güncellenecek kayıt</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<T>> UpdateAsync(T entity);
        
        /// <summary>
        /// Kayıt siler
        /// </summary>
        /// <param name="id">Silinecek kayıt ID'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult> DeleteAsync(int id);
        
        /// <summary>
        /// Kayıt var mı kontrol eder
        /// </summary>
        /// <param name="id">Kayıt ID'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<bool>> ExistsAsync(int id);
        
        /// <summary>
        /// Toplam kayıt sayısını getirir
        /// </summary>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<int>> GetCountAsync();
        
        /// <summary>
        /// Sayfalama ile kayıtları getirir
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetPagedAsync(int pageNumber, int pageSize);
        
        /// <summary>
        /// Belirtilen koşula göre kayıtları getirir
        /// </summary>
        /// <param name="predicate">Filtreleme koşulu</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetWhereAsync(Func<T, bool> predicate);
        
        /// <summary>
        /// Belirtilen koşula göre tek kayıt getirir
        /// </summary>
        /// <param name="predicate">Filtreleme koşulu</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<T>> GetFirstOrDefaultAsync(Func<T, bool> predicate);
        
        /// <summary>
        /// Toplu kayıt ekler
        /// </summary>
        /// <param name="entities">Eklenecek kayıtlar</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> AddRangeAsync(List<T> entities);
        
        /// <summary>
        /// Toplu kayıt günceller
        /// </summary>
        /// <param name="entities">Güncellenecek kayıtlar</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> UpdateRangeAsync(List<T> entities);
        
        /// <summary>
        /// Toplu kayıt siler
        /// </summary>
        /// <param name="ids">Silinecek kayıt ID'leri</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult> DeleteRangeAsync(List<int> ids);
        
        /// <summary>
        /// Belirtilen koşula göre kayıtları siler
        /// </summary>
        /// <param name="predicate">Silme koşulu</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult> DeleteWhereAsync(Func<T, bool> predicate);
        
        /// <summary>
        /// Tüm kayıtları siler
        /// </summary>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult> DeleteAllAsync();
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre sıralar
        /// </summary>
        /// <param name="propertyName">Sıralama property'si</param>
        /// <param name="ascending">Artan sıralama mı?</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetOrderedAsync(string propertyName, bool ascending = true);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre gruplar
        /// </summary>
        /// <param name="propertyName">Gruplama property'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<object>> GetGroupedAsync(string propertyName);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre filtreler
        /// </summary>
        /// <param name="propertyName">Filtreleme property'si</param>
        /// <param name="value">Filtreleme değeri</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetFilteredAsync(string propertyName, object value);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre arama yapar
        /// </summary>
        /// <param name="propertyName">Arama property'si</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> SearchAsync(string propertyName, string searchTerm);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre sayısal karşılaştırma yapar
        /// </summary>
        /// <param name="propertyName">Karşılaştırma property'si</param>
        /// <param name="operator">Karşılaştırma operatörü</param>
        /// <param name="value">Karşılaştırma değeri</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetByComparisonAsync(string propertyName, string @operator, object value);
        
        /// <summary>
        /// Kayıtları belirtilen tarih aralığında getirir
        /// </summary>
        /// <param name="propertyName">Tarih property'si</param>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<T>>> GetByDateRangeAsync(string propertyName, DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre distinct yapar
        /// </summary>
        /// <param name="propertyName">Distinct property'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<List<object>>> GetDistinctAsync(string propertyName);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre sayar
        /// </summary>
        /// <param name="propertyName">Sayma property'si</param>
        /// <param name="value">Sayılacak değer</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<int>> CountByPropertyAsync(string propertyName, object value);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre toplar
        /// </summary>
        /// <param name="propertyName">Toplama property'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<decimal>> SumByPropertyAsync(string propertyName);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre ortalamasını alır
        /// </summary>
        /// <param name="propertyName">Ortalama property'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<decimal>> AverageByPropertyAsync(string propertyName);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre minimum değerini bulur
        /// </summary>
        /// <param name="propertyName">Minimum property'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<T>> GetMinByPropertyAsync(string propertyName);
        
        /// <summary>
        /// Kayıtları belirtilen property'ye göre maksimum değerini bulur
        /// </summary>
        /// <param name="propertyName">Maksimum property'si</param>
        /// <returns>ServiceResult</returns>
        public abstract Task<ServiceResult<T>> GetMaxByPropertyAsync(string propertyName);
    }
}
