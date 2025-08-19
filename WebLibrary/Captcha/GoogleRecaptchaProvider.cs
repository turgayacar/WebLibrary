using System.Text;
using System.Text.Json;
using WebLibrary.Models;

namespace WebLibrary.Captcha
{
    /// <summary>
    /// Google reCAPTCHA sağlayıcısı
    /// </summary>
    public class GoogleRecaptchaProvider : ICaptchaProvider
    {
        private readonly GoogleRecaptchaSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, CaptchaResult> _captchaStore;

        public CaptchaType Type { get; }

        public GoogleRecaptchaProvider(GoogleRecaptchaSettings settings, CaptchaType type = CaptchaType.GoogleRecaptchaV2)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Type = type;
            _httpClient = new HttpClient();
            _captchaStore = new Dictionary<string, CaptchaResult>();
        }

        public ServiceResult<CaptchaResult> GenerateCaptcha(CaptchaOptions options)
        {
            try
            {
                var captchaId = Guid.NewGuid().ToString();
                var expiresAt = DateTime.UtcNow.AddSeconds(options.ExpirationSeconds);

                var captchaResult = new CaptchaResult
                {
                    Id = captchaId,
                    Type = Type,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    IsSuccessful = false
                };

                // HTML ve JavaScript kodlarını oluştur
                if (Type == CaptchaType.GoogleRecaptchaV2)
                {
                    captchaResult.Html = GenerateRecaptchaV2Html(options);
                    captchaResult.JavaScript = GenerateRecaptchaV2JavaScript(options);
                }
                else if (Type == CaptchaType.GoogleRecaptchaV3)
                {
                    captchaResult.Html = GenerateRecaptchaV3Html(options);
                    captchaResult.JavaScript = GenerateRecaptchaV3JavaScript(options);
                }

                // CAPTCHA'yı sakla
                _captchaStore[captchaId] = captchaResult;

                return ServiceResult<CaptchaResult>.Success(captchaResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<CaptchaResult>.Error($"reCAPTCHA oluşturma hatası: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ValidateCaptcha(string captchaId, string userInput)
        {
            try
            {
                if (string.IsNullOrEmpty(captchaId))
                    return ServiceResult<bool>.Error("CAPTCHA ID boş olamaz");

                if (string.IsNullOrEmpty(userInput))
                    return ServiceResult<bool>.Error("Kullanıcı girişi boş olamaz");

                // CAPTCHA'yı bul
                if (!_captchaStore.TryGetValue(captchaId, out var captcha))
                    return ServiceResult<bool>.Error("CAPTCHA bulunamadı");

                // Süre kontrolü
                if (DateTime.UtcNow > captcha.ExpiresAt)
                    return ServiceResult<bool>.Error("CAPTCHA süresi dolmuş");

                // Kullanılmış mı kontrolü
                if (captcha.IsUsed)
                    return ServiceResult<bool>.Error("CAPTCHA zaten kullanılmış");

                // Google API'ye doğrulama isteği gönder
                var validationResult = await ValidateWithGoogle(userInput);
                if (!validationResult.IsSuccess)
                    return ServiceResult<bool>.Error(validationResult.ErrorMessages.FirstOrDefault() ?? "Doğrulama hatası");

                // CAPTCHA'yı güncelle
                captcha.IsUsed = true;
                captcha.IsSuccessful = validationResult.Data;

                return ServiceResult<bool>.Success(validationResult.Data);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"CAPTCHA doğrulama hatası: {ex.Message}");
            }
        }

        public ServiceResult<bool> CleanupCaptcha(string captchaId)
        {
            try
            {
                if (string.IsNullOrEmpty(captchaId))
                    return ServiceResult<bool>.Error("CAPTCHA ID boş olamaz");

                if (_captchaStore.Remove(captchaId))
                    return ServiceResult<bool>.Success(true);

                return ServiceResult<bool>.Error("CAPTCHA bulunamadı");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"CAPTCHA temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Google API'ye doğrulama isteği gönderir
        /// </summary>
        private async Task<ServiceResult<bool>> ValidateWithGoogle(string userInput)
        {
            try
            {
                var postData = new Dictionary<string, string>
                {
                    ["secret"] = _settings.SecretKey,
                    ["response"] = userInput
                };

                var content = new FormUrlEncodedContent(postData);
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                if (jsonResponse.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
                {
                    if (Type == CaptchaType.GoogleRecaptchaV3)
                    {
                        // V3 için skor kontrolü
                        if (jsonResponse.TryGetProperty("score", out var scoreElement))
                        {
                            var score = scoreElement.GetDouble();
                            return ServiceResult<bool>.Success(score >= _settings.MinScore);
                        }
                    }
                    else
                    {
                        // V2 için sadece success kontrolü
                        return ServiceResult<bool>.Success(true);
                    }
                }

                return ServiceResult<bool>.Success(false);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Google API doğrulama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// reCAPTCHA v2 HTML kodu oluşturur
        /// </summary>
        private string GenerateRecaptchaV2Html(CaptchaOptions options)
        {
            var theme = options.Theme == CaptchaTheme.Dark ? "dark" : "light";
            var size = options.Difficulty == CaptchaDifficulty.Easy ? "compact" : "normal";

            return $@"
                <div class=""g-recaptcha"" 
                     data-sitekey=""{_settings.SiteKey}""
                     data-theme=""{theme}""
                     data-size=""{size}""
                     data-callback=""onRecaptchaSuccess""
                     data-expired-callback=""onRecaptchaExpired"">
                </div>";
        }

        /// <summary>
        /// reCAPTCHA v2 JavaScript kodu oluşturur
        /// </summary>
        private string GenerateRecaptchaV2JavaScript(CaptchaOptions options)
        {
            return $@"
                <script src=""https://www.google.com/recaptcha/api.js?hl={options.Language}""></script>
                <script>
                    function onRecaptchaSuccess(token) {{
                        // CAPTCHA başarılı olduğunda yapılacak işlemler
                        console.log('reCAPTCHA başarılı:', token);
                        // Form submit edilebilir
                        document.getElementById('captcha-token').value = token;
                    }}
                    
                    function onRecaptchaExpired() {{
                        // CAPTCHA süresi dolduğunda yapılacak işlemler
                        console.log('reCAPTCHA süresi doldu');
                        grecaptcha.reset();
                    }}
                </script>";
        }

        /// <summary>
        /// reCAPTCHA v3 HTML kodu oluşturur
        /// </summary>
        private string GenerateRecaptchaV3Html(CaptchaOptions options)
        {
            return $@"
                <input type=""hidden"" id=""captcha-token"" name=""captcha-token"" />
                <div id=""recaptcha-v3-container""></div>";
        }

        /// <summary>
        /// reCAPTCHA v3 JavaScript kodu oluşturur
        /// </summary>
        private string GenerateRecaptchaV3JavaScript(CaptchaOptions options)
        {
            return $@"
                <script src=""https://www.google.com/recaptcha/api.js?render={_settings.SiteKey}""></script>
                <script>
                    grecaptcha.ready(function() {{
                        grecaptcha.execute('{_settings.SiteKey}', {{action: '{_settings.Action}'}})
                        .then(function(token) {{
                            document.getElementById('captcha-token').value = token;
                            console.log('reCAPTCHA v3 token:', token);
                        }});
                    }});
                    
                    // Form submit edildiğinde token'ı yenile
                    function refreshRecaptchaToken() {{
                        grecaptcha.ready(function() {{
                            grecaptcha.execute('{_settings.SiteKey}', {{action: '{_settings.Action}'}})
                            .then(function(token) {{
                                document.getElementById('captcha-token').value = token;
                            }});
                        }});
                    }}
                </script>";
        }

        /// <summary>
        /// Süresi dolmuş CAPTCHA'ları temizler
        /// </summary>
        public void CleanupExpiredCaptchas()
        {
            var expiredKeys = _captchaStore
                .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _captchaStore.Remove(key);
            }
        }

        /// <summary>
        /// CAPTCHA istatistiklerini alır
        /// </summary>
        public CaptchaStatistics GetStatistics()
        {
            var now = DateTime.UtcNow;
            var validCaptchas = _captchaStore.Values.Where(c => c.ExpiresAt > now);

            return new CaptchaStatistics
            {
                TotalCaptchas = _captchaStore.Count,
                SuccessfulValidations = validCaptchas.Count(c => c.IsSuccessful),
                FailedValidations = validCaptchas.Count(c => c.IsUsed && !c.IsSuccessful),
                AverageValidationTime = 0, // Bu bilgi saklanmıyor
                HighestScore = 0, // V3 için kullanılabilir
                LowestScore = 0
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
