namespace WebLibrary.Models
{
    /// <summary>
    /// Generic servis sonuç sınıfı
    /// </summary>
    /// <typeparam name="T">Döndürülecek veri tipi</typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Döndürülecek veri
        /// </summary>
        public T? Data { get; set; }
        
        /// <summary>
        /// Hata mesajları listesi
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new();
        
        /// <summary>
        /// Toplam kayıt sayısı (sayfalama için)
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Başarılı sonuç oluşturur
        /// </summary>
        /// <param name="data">Döndürülecek veri</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                ErrorMessages = new List<string>()
            };
        }
        
        /// <summary>
        /// Başarılı sonuç oluşturur (veri olmadan)
        /// </summary>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> Success()
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = default,
                ErrorMessages = new List<string>()
            };
        }
        
        /// <summary>
        /// Hata sonucu oluşturur
        /// </summary>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> Error(string errorMessage)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessages = new List<string> { errorMessage }
            };
        }
        
        /// <summary>
        /// Hata sonucu oluşturur (çoklu hata mesajları ile)
        /// </summary>
        /// <param name="errorMessages">Hata mesajları listesi</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> Error(List<string> errorMessages)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Data = default,
                ErrorMessages = errorMessages
            };
        }
    }
    
    /// <summary>
    /// Non-generic servis sonuç sınıfı
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Hata mesajları listesi
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new();
        
        /// <summary>
        /// Başarılı sonuç oluşturur
        /// </summary>
        /// <returns>ServiceResult</returns>
        public static ServiceResult Success()
        {
            return new ServiceResult
            {
                IsSuccess = true,
                ErrorMessages = new List<string>()
            };
        }
        
        /// <summary>
        /// Hata sonucu oluşturur
        /// </summary>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult Error(string errorMessage)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessages = new List<string> { errorMessage }
            };
        }
        
        /// <summary>
        /// Hata sonucu oluşturur (çoklu hata mesajları ile)
        /// </summary>
        /// <param name="errorMessages">Hata mesajları listesi</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult Error(List<string> errorMessages)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                ErrorMessages = errorMessages
            };
        }
    }
}
