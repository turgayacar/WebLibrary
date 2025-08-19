namespace WebLibrary
{
    /// <summary>
    /// Global sabitler ve yapılandırma değerleri
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Base API URL
        /// </summary>
        public static string BaseApiUrl { get; set; } = "https://api.example.com";
        
        /// <summary>
        /// API Timeout süresi (saniye)
        /// </summary>
        public static int ApiTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Maksimum retry sayısı
        /// </summary>
        public static int MaxRetryCount { get; set; } = 3;
        
        /// <summary>
        /// Cache süresi (dakika)
        /// </summary>
        public static int CacheExpirationMinutes { get; set; } = 15;
        
        /// <summary>
        /// Log seviyesi
        /// </summary>
        public static string LogLevel { get; set; } = "Information";
        
        // Güvenlik ayarları
        /// <summary>
        /// JWT Secret Key
        /// </summary>
        public static string JwtSecretKey { get; set; } = "your-super-secret-jwt-key-here-change-in-production";
        
        /// <summary>
        /// JWT Issuer
        /// </summary>
        public static string JwtIssuer { get; set; } = "WebLibrary";
        
        /// <summary>
        /// JWT Audience
        /// </summary>
        public static string JwtAudience { get; set; } = "WebLibraryUsers";
        
        /// <summary>
        /// JWT Token süresi (dakika)
        /// </summary>
        public static int JwtExpirationMinutes { get; set; } = 60;
        
        /// <summary>
        /// BCrypt Work Factor
        /// </summary>
        public static int BcryptWorkFactor { get; set; } = 12;
        
        /// <summary>
        /// Minimum şifre uzunluğu
        /// </summary>
        public static int MinPasswordLength { get; set; } = 8;
        
        /// <summary>
        /// Şifre yenileme süresi (gün)
        /// </summary>
        public static int PasswordExpirationDays { get; set; } = 90;
    }
}
