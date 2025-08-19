using System.ComponentModel.DataAnnotations;

namespace WebLibrary.Captcha
{
    /// <summary>
    /// CAPTCHA türleri
    /// </summary>
    public enum CaptchaType
    {
        /// <summary>
        /// Google reCAPTCHA v2
        /// </summary>
        GoogleRecaptchaV2,

        /// <summary>
        /// Google reCAPTCHA v3
        /// </summary>
        GoogleRecaptchaV3,

        /// <summary>
        /// HCaptcha
        /// </summary>
        HCaptcha,

        /// <summary>
        /// Custom matematik CAPTCHA
        /// </summary>
        MathCaptcha,

        /// <summary>
        /// Custom resim CAPTCHA
        /// </summary>
        ImageCaptcha,

        /// <summary>
        /// Custom ses CAPTCHA
        /// </summary>
        AudioCaptcha
    }

    /// <summary>
    /// CAPTCHA seçenekleri
    /// </summary>
    public class CaptchaOptions
    {
        /// <summary>
        /// CAPTCHA türü
        /// </summary>
        public CaptchaType Type { get; set; }

        /// <summary>
        /// CAPTCHA zorluğu
        /// </summary>
        public CaptchaDifficulty Difficulty { get; set; } = CaptchaDifficulty.Medium;

        /// <summary>
        /// CAPTCHA süresi (saniye)
        /// </summary>
        public int ExpirationSeconds { get; set; } = 300; // 5 dakika

        /// <summary>
        /// Özel ayarlar
        /// </summary>
        public Dictionary<string, object> CustomSettings { get; set; } = new();

        /// <summary>
        /// Dil kodu
        /// </summary>
        public string Language { get; set; } = "tr";

        /// <summary>
        /// Tema
        /// </summary>
        public CaptchaTheme Theme { get; set; } = CaptchaTheme.Light;
    }

    /// <summary>
    /// CAPTCHA zorluğu
    /// </summary>
    public enum CaptchaDifficulty
    {
        Easy,
        Medium,
        Hard,
        VeryHard
    }

    /// <summary>
    /// CAPTCHA teması
    /// </summary>
    public enum CaptchaTheme
    {
        Light,
        Dark,
        Auto
    }

    /// <summary>
    /// CAPTCHA sonucu
    /// </summary>
    public class CaptchaResult
    {
        /// <summary>
        /// CAPTCHA ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// CAPTCHA türü
        /// </summary>
        public CaptchaType Type { get; set; }

        /// <summary>
        /// CAPTCHA HTML kodu
        /// </summary>
        public string Html { get; set; } = string.Empty;

        /// <summary>
        /// CAPTCHA JavaScript kodu
        /// </summary>
        public string JavaScript { get; set; } = string.Empty;

        /// <summary>
        /// CAPTCHA CSS kodu
        /// </summary>
        public string Css { get; set; } = string.Empty;

        /// <summary>
        /// CAPTCHA verisi (JSON)
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturulma zamanı
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Son kullanma zamanı
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Kullanıldı mı?
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Başarılı mı?
        /// </summary>
        public bool IsSuccessful { get; set; }
    }

    /// <summary>
    /// Google reCAPTCHA ayarları
    /// </summary>
    public class GoogleRecaptchaSettings
    {
        /// <summary>
        /// Site anahtarı
        /// </summary>
        [Required]
        public string SiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Gizli anahtar
        /// </summary>
        [Required]
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Minimum skor (v3 için)
        /// </summary>
        public double MinScore { get; set; } = 0.5;

        /// <summary>
        /// Action adı (v3 için)
        /// </summary>
        public string Action { get; set; } = "submit";

        /// <summary>
        /// Tema
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Boyut
        /// </summary>
        public string Size { get; set; } = "normal";

        /// <summary>
        /// Dil
        /// </summary>
        public string Language { get; set; } = "tr";
    }

    /// <summary>
    /// HCaptcha ayarları
    /// </summary>
    public class HCaptchaSettings
    {
        /// <summary>
        /// Site anahtarı
        /// </summary>
        [Required]
        public string SiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Gizli anahtar
        /// </summary>
        [Required]
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Tema
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Boyut
        /// </summary>
        public string Size { get; set; } = "normal";
    }

    /// <summary>
    /// Custom CAPTCHA ayarları
    /// </summary>
    public class CustomCaptchaSettings
    {
        /// <summary>
        /// CAPTCHA genişliği
        /// </summary>
        public int Width { get; set; } = 200;

        /// <summary>
        /// CAPTCHA yüksekliği
        /// </summary>
        public int Height { get; set; } = 80;

        /// <summary>
        /// Font boyutu
        /// </summary>
        public int FontSize { get; set; } = 24;

        /// <summary>
        /// Karakter sayısı
        /// </summary>
        public int CharacterCount { get; set; } = 6;

        /// <summary>
        /// Karışıklık seviyesi
        /// </summary>
        public int NoiseLevel { get; set; } = 3;

        /// <summary>
        /// Renk teması
        /// </summary>
        public string ColorTheme { get; set; } = "default";

        /// <summary>
        /// Ses dosyası yolu
        /// </summary>
        public string AudioFilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// CAPTCHA doğrulama sonucu
    /// </summary>
    public class CaptchaValidationResult
    {
        /// <summary>
        /// CAPTCHA ID
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// Doğrulama başarılı mı?
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Skor (reCAPTCHA v3 için)
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Hata mesajı
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Doğrulama zamanı
        /// </summary>
        public DateTime ValidatedAt { get; set; }

        /// <summary>
        /// IP adresi
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// User agent
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// CAPTCHA istatistikleri
    /// </summary>
    public class CaptchaStatistics
    {
        /// <summary>
        /// Toplam CAPTCHA sayısı
        /// </summary>
        public int TotalCaptchas { get; set; }

        /// <summary>
        /// Başarılı doğrulama sayısı
        /// </summary>
        public int SuccessfulValidations { get; set; }

        /// <summary>
        /// Başarısız doğrulama sayısı
        /// </summary>
        public int FailedValidations { get; set; }

        /// <summary>
        /// Ortalama doğrulama süresi (ms)
        /// </summary>
        public double AverageValidationTime { get; set; }

        /// <summary>
        /// En yüksek skor
        /// </summary>
        public double HighestScore { get; set; }

        /// <summary>
        /// En düşük skor
        /// </summary>
        public double LowestScore { get; set; }

        /// <summary>
        /// Başarı oranı (%)
        /// </summary>
        public double SuccessRate => TotalCaptchas > 0 ? (double)SuccessfulValidations / TotalCaptchas * 100 : 0;
    }
}
