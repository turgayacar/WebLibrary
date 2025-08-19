using System.Security.Cryptography;
using WebLibrary.Models;

namespace WebLibrary.Captcha
{
    /// <summary>
    /// Matematik CAPTCHA sağlayıcısı
    /// </summary>
    public class MathCaptchaProvider : ICaptchaProvider
    {
        private readonly Dictionary<string, CaptchaResult> _captchaStore;
        private readonly Dictionary<string, MathCaptchaData> _mathDataStore;

        public CaptchaType Type => CaptchaType.MathCaptcha;

        public MathCaptchaProvider()
        {
            _captchaStore = new Dictionary<string, CaptchaResult>();
            _mathDataStore = new Dictionary<string, MathCaptchaData>();
        }

        public ServiceResult<CaptchaResult> GenerateCaptcha(CaptchaOptions options)
        {
            try
            {
                var captchaId = Guid.NewGuid().ToString();
                var expiresAt = DateTime.UtcNow.AddSeconds(options.ExpirationSeconds);

                // Matematik problemi oluştur
                var mathProblem = GenerateMathProblem(options.Difficulty);
                var answer = CalculateAnswer(mathProblem);

                // Matematik verilerini sakla
                _mathDataStore[captchaId] = new MathCaptchaData
                {
                    Problem = mathProblem,
                    Answer = answer,
                    CreatedAt = DateTime.UtcNow
                };

                var captchaResult = new CaptchaResult
                {
                    Id = captchaId,
                    Type = Type,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    IsSuccessful = false,
                    Html = GenerateMathCaptchaHtml(mathProblem, options),
                    JavaScript = GenerateMathCaptchaJavaScript(options),
                    Css = GenerateMathCaptchaCss(options),
                    Data = $"{{\"problem\":\"{mathProblem}\",\"difficulty\":\"{options.Difficulty}\"}}"
                };

                // CAPTCHA'yı sakla
                _captchaStore[captchaId] = captchaResult;

                return ServiceResult<CaptchaResult>.Success(captchaResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<CaptchaResult>.Error($"Matematik CAPTCHA oluşturma hatası: {ex.Message}");
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

                // Matematik verilerini bul
                if (!_mathDataStore.TryGetValue(captchaId, out var mathData))
                    return ServiceResult<bool>.Error("Matematik verileri bulunamadı");

                // Süre kontrolü
                if (DateTime.UtcNow > captcha.ExpiresAt)
                    return ServiceResult<bool>.Error("CAPTCHA süresi dolmuş");

                // Kullanılmış mı kontrolü
                if (captcha.IsUsed)
                    return ServiceResult<bool>.Error("CAPTCHA zaten kullanılmış");

                // Cevabı doğrula
                var isValid = ValidateAnswer(userInput, mathData.Answer);
                
                // CAPTCHA'yı güncelle
                captcha.IsUsed = true;
                captcha.IsSuccessful = isValid;

                return ServiceResult<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Matematik CAPTCHA doğrulama hatası: {ex.Message}");
            }
        }

        public ServiceResult<bool> CleanupCaptcha(string captchaId)
        {
            try
            {
                if (string.IsNullOrEmpty(captchaId))
                    return ServiceResult<bool>.Error("CAPTCHA ID boş olamaz");

                var success = false;
                if (_captchaStore.Remove(captchaId))
                    success = true;
                if (_mathDataStore.Remove(captchaId))
                    success = true;

                return ServiceResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Matematik CAPTCHA temizleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Matematik problemi oluşturur
        /// </summary>
        private string GenerateMathProblem(CaptchaDifficulty difficulty)
        {
            var random = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            random.GetBytes(bytes);
            var seed = BitConverter.ToInt32(bytes, 0);
            var rnd = new Random(seed);

            return difficulty switch
            {
                CaptchaDifficulty.Easy => GenerateEasyProblem(rnd),
                CaptchaDifficulty.Medium => GenerateMediumProblem(rnd),
                CaptchaDifficulty.Hard => GenerateHardProblem(rnd),
                CaptchaDifficulty.VeryHard => GenerateVeryHardProblem(rnd),
                _ => GenerateMediumProblem(rnd)
            };
        }

        /// <summary>
        /// Kolay matematik problemi
        /// </summary>
        private string GenerateEasyProblem(Random rnd)
        {
            var a = rnd.Next(1, 10);
            var b = rnd.Next(1, 10);
            var operation = rnd.Next(0, 2); // 0: toplama, 1: çıkarma

            return operation == 0 ? $"{a} + {b} = ?" : $"{a} - {b} = ?";
        }

        /// <summary>
        /// Orta matematik problemi
        /// </summary>
        private string GenerateMediumProblem(Random rnd)
        {
            var a = rnd.Next(1, 20);
            var b = rnd.Next(1, 20);
            var operation = rnd.Next(0, 3); // 0: toplama, 1: çıkarma, 2: çarpma

            return operation switch
            {
                0 => $"{a} + {b} = ?",
                1 => $"{a} - {b} = ?",
                _ => $"{a} × {b} = ?"
            };
        }

        /// <summary>
        /// Zor matematik problemi
        /// </summary>
        private string GenerateHardProblem(Random rnd)
        {
            var a = rnd.Next(1, 50);
            var b = rnd.Next(1, 50);
            var c = rnd.Next(1, 20);
            var operation = rnd.Next(0, 4);

            return operation switch
            {
                0 => $"{a} + {b} - {c} = ?",
                1 => $"{a} × {b} + {c} = ?",
                2 => $"{a} - {b} × {c} = ?",
                _ => $"({a} + {b}) × {c} = ?"
            };
        }

        /// <summary>
        /// Çok zor matematik problemi
        /// </summary>
        private string GenerateVeryHardProblem(Random rnd)
        {
            var a = rnd.Next(1, 100);
            var b = rnd.Next(1, 100);
            var c = rnd.Next(1, 50);
            var d = rnd.Next(1, 20);
            var operation = rnd.Next(0, 5);

            return operation switch
            {
                0 => $"{a} + {b} × {c} - {d} = ?",
                1 => $"({a} + {b}) × ({c} - {d}) = ?",
                2 => $"{a} × {b} + {c} × {d} = ?",
                3 => $"{a}² + {b} = ?",
                _ => $"√{a} + {b} = ?"
            };
        }

        /// <summary>
        /// Cevabı hesaplar
        /// </summary>
        private double CalculateAnswer(string problem)
        {
            try
            {
                // Basit matematik işlemleri için güvenli hesaplama
                var cleanProblem = problem.Replace("=", "").Replace("?", "").Replace("²", "^2").Replace("√", "Math.Sqrt(");
                
                // Güvenlik için sadece belirli karakterlere izin ver
                if (!IsSafeMathExpression(cleanProblem))
                    throw new ArgumentException("Güvenli olmayan matematik ifadesi");

                // Basit hesaplama (gerçek uygulamada daha güvenli bir parser kullanılmalı)
                return EvaluateSimpleExpression(cleanProblem);
            }
            catch
            {
                // Hata durumunda basit bir toplama döndür
                return 0;
            }
        }

        /// <summary>
        /// Güvenli matematik ifadesi kontrolü
        /// </summary>
        private bool IsSafeMathExpression(string expression)
        {
            var allowedChars = "0123456789+-×() .";
            return expression.All(c => allowedChars.Contains(c));
        }

        /// <summary>
        /// Basit matematik ifadesi değerlendirir
        /// </summary>
        private double EvaluateSimpleExpression(string expression)
        {
            // Bu basit bir implementasyon, gerçek uygulamada daha güvenli bir parser kullanılmalı
            try
            {
                // Sadece basit işlemler için
                expression = expression.Replace("×", "*");
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(expression, "");
                return Convert.ToDouble(result);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Cevabı doğrular
        /// </summary>
        private bool ValidateAnswer(string userInput, double correctAnswer)
        {
            if (double.TryParse(userInput, out var userAnswer))
            {
                // Küçük hata payı ile karşılaştır
                return Math.Abs(userAnswer - correctAnswer) < 0.01;
            }
            return false;
        }

        /// <summary>
        /// Matematik CAPTCHA HTML kodu oluşturur
        /// </summary>
        private string GenerateMathCaptchaHtml(string problem, CaptchaOptions options)
        {
            var theme = options.Theme == CaptchaTheme.Dark ? "dark" : "light";
            
            return $@"
                <div class=""math-captcha {theme}"">
                    <div class=""math-problem"">{problem}</div>
                    <input type=""text"" 
                           id=""math-answer"" 
                           name=""math-answer"" 
                           class=""math-input"" 
                           placeholder=""Cevabınızı yazın"" 
                           maxlength=""10"" />
                    <button type=""button"" 
                            class=""math-refresh"" 
                            onclick=""refreshMathCaptcha()"">
                        ↻ Yeni Soru
                    </button>
                </div>";
        }

        /// <summary>
        /// Matematik CAPTCHA JavaScript kodu oluşturur
        /// </summary>
        private string GenerateMathCaptchaJavaScript(CaptchaOptions options)
        {
            return @"
                <script>
                    function refreshMathCaptcha() {
                        // AJAX ile yeni CAPTCHA iste
                        fetch('/api/captcha/refresh', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            }
                        })
                        .then(response => response.json())
                        .then(data => {
                            if (data.isSuccess) {
                                location.reload();
                            }
                        })
                        .catch(error => {
                            console.error('CAPTCHA yenileme hatası:', error);
                        });
                    }
                    
                    // Input validation
                    document.getElementById('math-answer').addEventListener('input', function(e) {
                        var value = e.target.value;
                        // Sadece sayı ve işaretlere izin ver
                        e.target.value = value.replace(/[^0-9+\-.]/g, '');
                    });
                </script>";
        }

        /// <summary>
        /// Matematik CAPTCHA CSS kodu oluşturur
        /// </summary>
        private string GenerateMathCaptchaCss(CaptchaOptions options)
        {
            var theme = options.Theme;
            
            return $@"
                <style>
                    .math-captcha {{
                        padding: 15px;
                        border: 2px solid #ddd;
                        border-radius: 8px;
                        background: {(theme == CaptchaTheme.Dark ? "#2c2c2c" : "#f9f9f9")};
                        color: {(theme == CaptchaTheme.Dark ? "#ffffff" : "#333333")};
                        font-family: 'Arial', sans-serif;
                        text-align: center;
                        max-width: 300px;
                        margin: 10px auto;
                    }}
                    
                    .math-problem {{
                        font-size: 24px;
                        font-weight: bold;
                        margin-bottom: 15px;
                        color: {(theme == CaptchaTheme.Dark ? "#ffffff" : "#333333")};
                    }}
                    
                    .math-input {{
                        width: 100%;
                        padding: 10px;
                        border: 1px solid #ccc;
                        border-radius: 4px;
                        font-size: 16px;
                        text-align: center;
                        margin-bottom: 10px;
                        background: {(theme == CaptchaTheme.Dark ? "#444" : "#ffffff")};
                        color: {(theme == CaptchaTheme.Dark ? "#ffffff" : "#333333")};
                    }}
                    
                    .math-refresh {{
                        background: #007bff;
                        color: white;
                        border: none;
                        padding: 8px 16px;
                        border-radius: 4px;
                        cursor: pointer;
                        font-size: 14px;
                    }}
                    
                    .math-refresh:hover {{
                        background: #0056b3;
                    }}
                    
                    .math-captcha.dark {{
                        border-color: #555;
                    }}
                </style>";
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
                _mathDataStore.Remove(key);
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
                AverageValidationTime = 0,
                HighestScore = 0,
                LowestScore = 0
            };
        }
    }

    /// <summary>
    /// Matematik CAPTCHA verisi
    /// </summary>
    public class MathCaptchaData
    {
        /// <summary>
        /// Matematik problemi
        /// </summary>
        public string Problem { get; set; } = string.Empty;

        /// <summary>
        /// Doğru cevap
        /// </summary>
        public double Answer { get; set; }

        /// <summary>
        /// Oluşturulma zamanı
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
