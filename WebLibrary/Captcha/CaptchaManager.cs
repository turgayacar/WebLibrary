using WebLibrary.Models;

namespace WebLibrary.Captcha
{
    /// <summary>
    /// CAPTCHA yöneticisi - birden fazla CAPTCHA sağlayıcısını yönetir
    /// </summary>
    public class CaptchaManager
    {
        private readonly Dictionary<CaptchaType, ICaptchaProvider> _providers;
        private readonly Dictionary<string, CaptchaResult> _globalCaptchaStore;
        private readonly Timer _cleanupTimer;

        public CaptchaManager()
        {
            _providers = new Dictionary<CaptchaType, ICaptchaProvider>();
            _globalCaptchaStore = new Dictionary<string, CaptchaResult>();
            
            // Otomatik temizleme timer'ı (her 5 dakikada bir)
            _cleanupTimer = new Timer(CleanupExpiredCaptchas, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// CAPTCHA sağlayıcısı ekler
        /// </summary>
        /// <param name="provider">CAPTCHA sağlayıcısı</param>
        public void AddProvider(ICaptchaProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _providers[provider.Type] = provider;
        }

        /// <summary>
        /// CAPTCHA sağlayıcısı kaldırır
        /// </summary>
        /// <param name="type">CAPTCHA türü</param>
        public void RemoveProvider(CaptchaType type)
        {
            if (_providers.ContainsKey(type))
                _providers.Remove(type);
        }

        /// <summary>
        /// CAPTCHA oluşturur
        /// </summary>
        /// <param name="type">CAPTCHA türü</param>
        /// <param name="options">CAPTCHA seçenekleri</param>
        /// <returns>CAPTCHA sonucu</returns>
        public ServiceResult<CaptchaResult> GenerateCaptcha(CaptchaType type, CaptchaOptions options)
        {
            try
            {
                if (!_providers.TryGetValue(type, out var provider))
                    return ServiceResult<CaptchaResult>.Error($"CAPTCHA sağlayıcısı bulunamadı: {type}");

                options.Type = type;
                var result = provider.GenerateCaptcha(options);

                if (result.IsSuccess)
                {
                    // Global store'a ekle
                    _globalCaptchaStore[result.Data.Id] = result.Data;
                }

                return result;
            }
            catch (Exception ex)
            {
                return ServiceResult<CaptchaResult>.Error($"CAPTCHA oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CAPTCHA'yı doğrular
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <param name="userInput">Kullanıcı girişi</param>
        /// <returns>Doğrulama sonucu</returns>
        public async Task<ServiceResult<bool>> ValidateCaptcha(string captchaId, string userInput)
        {
            try
            {
                if (string.IsNullOrEmpty(captchaId))
                    return ServiceResult<bool>.Error("CAPTCHA ID boş olamaz");

                if (string.IsNullOrEmpty(userInput))
                    return ServiceResult<bool>.Error("Kullanıcı girişi boş olamaz");

                // Global store'da CAPTCHA'yı bul
                if (!_globalCaptchaStore.TryGetValue(captchaId, out var captcha))
                    return ServiceResult<bool>.Error("CAPTCHA bulunamadı");

                // İlgili sağlayıcıyı bul
                if (!_providers.TryGetValue(captcha.Type, out var provider))
                    return ServiceResult<bool>.Error($"CAPTCHA sağlayıcısı bulunamadı: {captcha.Type}");

                // Doğrulama yap
                var result = await provider.ValidateCaptcha(captchaId, userInput);

                if (result.IsSuccess)
                {
                    // Global store'ı güncelle
                    captcha.IsUsed = true;
                    captcha.IsSuccessful = result.Data;
                }

                return result;
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"CAPTCHA doğrulama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CAPTCHA'yı temizler
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <returns>Başarı durumu</returns>
        public ServiceResult<bool> CleanupCaptcha(string captchaId)
        {
            try
            {
                if (string.IsNullOrEmpty(captchaId))
                    return ServiceResult<bool>.Error("CAPTCHA ID boş olamaz");

                // Global store'dan kaldır
                if (_globalCaptchaStore.Remove(captchaId))
                {
                    // İlgili sağlayıcıdan da temizle
                    foreach (var provider in _providers.Values)
                    {
                        provider.CleanupCaptcha(captchaId);
                    }
                    return ServiceResult<bool>.Success(true);
                }

                return ServiceResult<bool>.Error("CAPTCHA bulunamadı");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"CAPTCHA temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli türde CAPTCHA oluşturur
        /// </summary>
        /// <param name="type">CAPTCHA türü</param>
        /// <param name="difficulty">Zorluk seviyesi</param>
        /// <param name="expirationSeconds">Süre (saniye)</param>
        /// <param name="language">Dil</param>
        /// <param name="theme">Tema</param>
        /// <returns>CAPTCHA sonucu</returns>
        public ServiceResult<CaptchaResult> GenerateSimpleCaptcha(CaptchaType type, 
            CaptchaDifficulty difficulty = CaptchaDifficulty.Medium, 
            int expirationSeconds = 300, 
            string language = "tr", 
            CaptchaTheme theme = CaptchaTheme.Light)
        {
            var options = new CaptchaOptions
            {
                Type = type,
                Difficulty = difficulty,
                ExpirationSeconds = expirationSeconds,
                Language = language,
                Theme = theme
            };

            return GenerateCaptcha(type, options);
        }

        /// <summary>
        /// Google reCAPTCHA oluşturur
        /// </summary>
        /// <param name="settings">reCAPTCHA ayarları</param>
        /// <param name="type">reCAPTCHA türü</param>
        /// <param name="options">CAPTCHA seçenekleri</param>
        /// <returns>CAPTCHA sonucu</returns>
        public ServiceResult<CaptchaResult> GenerateGoogleRecaptcha(GoogleRecaptchaSettings settings, 
            CaptchaType type = CaptchaType.GoogleRecaptchaV2, 
            CaptchaOptions? options = null)
        {
            try
            {
                // Provider'ı oluştur veya güncelle
                var provider = new GoogleRecaptchaProvider(settings, type);
                _providers[type] = provider;

                options ??= new CaptchaOptions();
                options.Type = type;

                return provider.GenerateCaptcha(options);
            }
            catch (Exception ex)
            {
                return ServiceResult<CaptchaResult>.Error($"Google reCAPTCHA oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Matematik CAPTCHA oluşturur
        /// </summary>
        /// <param name="difficulty">Zorluk seviyesi</param>
        /// <param name="options">CAPTCHA seçenekleri</param>
        /// <returns>CAPTCHA sonucu</returns>
        public ServiceResult<CaptchaResult> GenerateMathCaptcha(CaptchaDifficulty difficulty = CaptchaDifficulty.Medium, 
            CaptchaOptions? options = null)
        {
            try
            {
                // Provider'ı oluştur veya güncelle
                var provider = new MathCaptchaProvider();
                _providers[CaptchaType.MathCaptcha] = provider;

                options ??= new CaptchaOptions();
                options.Type = CaptchaType.MathCaptcha;
                options.Difficulty = difficulty;

                return provider.GenerateCaptcha(options);
            }
            catch (Exception ex)
            {
                return ServiceResult<CaptchaResult>.Error($"Matematik CAPTCHA oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CAPTCHA HTML kodunu alır
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <returns>HTML kodu</returns>
        public ServiceResult<string> GetCaptchaHtml(string captchaId)
        {
            try
            {
                if (!_globalCaptchaStore.TryGetValue(captchaId, out var captcha))
                    return ServiceResult<string>.Error("CAPTCHA bulunamadı");

                return ServiceResult<string>.Success(captcha.Html);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"HTML alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CAPTCHA JavaScript kodunu alır
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <returns>JavaScript kodu</returns>
        public ServiceResult<string> GetCaptchaJavaScript(string captchaId)
        {
            try
            {
                if (!_globalCaptchaStore.TryGetValue(captchaId, out var captcha))
                    return ServiceResult<string>.Error("CAPTCHA bulunamadı");

                return ServiceResult<string>.Success(captcha.JavaScript);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"JavaScript alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CAPTCHA CSS kodunu alır
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <returns>CSS kodu</returns>
        public ServiceResult<string> GetCaptchaCss(string captchaId)
        {
            try
            {
                if (!_globalCaptchaStore.TryGetValue(captchaId, out var captcha))
                    return ServiceResult<string>.Error("CAPTCHA bulunamadı");

                return ServiceResult<string>.Success(captcha.Css);
                }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"CSS alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CAPTCHA bilgilerini alır
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <returns>CAPTCHA bilgileri</returns>
        public ServiceResult<CaptchaResult> GetCaptchaInfo(string captchaId)
        {
            try
            {
                if (!_globalCaptchaStore.TryGetValue(captchaId, out var captcha))
                    return ServiceResult<CaptchaResult>.Error("CAPTCHA bulunamadı");

                return ServiceResult<CaptchaResult>.Success(captcha);
            }
            catch (Exception ex)
            {
                return ServiceResult<CaptchaResult>.Error($"CAPTCHA bilgisi alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm CAPTCHA istatistiklerini alır
        /// </summary>
        /// <returns>CAPTCHA istatistikleri</returns>
        public CaptchaStatistics GetAllStatistics()
        {
            var stats = new CaptchaStatistics();
            var now = DateTime.UtcNow;

            foreach (var captcha in _globalCaptchaStore.Values)
            {
                stats.TotalCaptchas++;
                
                if (captcha.IsUsed)
                {
                    if (captcha.IsSuccessful)
                        stats.SuccessfulValidations++;
                    else
                        stats.FailedValidations++;
                }
            }

            return stats;
        }

        /// <summary>
        /// Belirli türde CAPTCHA istatistiklerini alır
        /// </summary>
        /// <param name="type">CAPTCHA türü</param>
        /// <returns>CAPTCHA istatistikleri</returns>
        public CaptchaStatistics GetStatisticsByType(CaptchaType type)
        {
            var stats = new CaptchaStatistics();
            var now = DateTime.UtcNow;

            foreach (var captcha in _globalCaptchaStore.Values.Where(c => c.Type == type))
            {
                stats.TotalCaptchas++;
                
                if (captcha.IsUsed)
                {
                    if (captcha.IsSuccessful)
                        stats.SuccessfulValidations++;
                    else
                        stats.FailedValidations++;
                }
            }

            return stats;
        }

        /// <summary>
        /// Süresi dolmuş CAPTCHA'ları temizler
        /// </summary>
        private void CleanupExpiredCaptchas(object? state)
        {
            try
            {
                var expiredKeys = _globalCaptchaStore
                    .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _globalCaptchaStore.Remove(key);
                }

                // Her provider'dan da temizle
                foreach (var provider in _providers.Values)
                {
                    if (provider is GoogleRecaptchaProvider googleProvider)
                        googleProvider.CleanupExpiredCaptchas();
                    else if (provider is MathCaptchaProvider mathProvider)
                        mathProvider.CleanupExpiredCaptchas();
                }
            }
            catch (Exception ex)
            {
                // Log hatası
                Console.WriteLine($"CAPTCHA temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm CAPTCHA'ları temizler
        /// </summary>
        public void ClearAllCaptchas()
        {
            _globalCaptchaStore.Clear();
            
            foreach (var provider in _providers.Values)
            {
                if (provider is GoogleRecaptchaProvider googleProvider)
                    googleProvider.CleanupExpiredCaptchas();
                else if (provider is MathCaptchaProvider mathProvider)
                    mathProvider.CleanupExpiredCaptchas();
            }
        }

        /// <summary>
        /// Mevcut CAPTCHA türlerini alır
        /// </summary>
        /// <returns>CAPTCHA türleri listesi</returns>
        public List<CaptchaType> GetAvailableTypes()
        {
            return _providers.Keys.ToList();
        }

        /// <summary>
        /// Provider sayısını alır
        /// </summary>
        /// <returns>Provider sayısı</returns>
        public int GetProviderCount()
        {
            return _providers.Count;
        }

        /// <summary>
        /// CAPTCHA sayısını alır
        /// </returns>
        public int GetCaptchaCount()
        {
            return _globalCaptchaStore.Count;
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
}
