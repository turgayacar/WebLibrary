using BCrypt.Net;
using System.Security.Cryptography;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.Security
{
    /// <summary>
    /// Şifre işlemleri için helper sınıfı
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Şifreyi BCrypt ile hashler
        /// </summary>
        /// <param name="password">Hash'lenecek şifre</param>
        /// <param name="workFactor">BCrypt work factor (varsayılan: 12)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> HashPassword(string password, int workFactor = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return ServiceResult<string>.Error("Şifre boş olamaz");

                if (workFactor < 4 || workFactor > 31)
                    return ServiceResult<string>.Error("Work factor 4-31 arasında olmalıdır");

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor);
                return ServiceResult<string>.Success(hashedPassword);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Şifre hashleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifreyi doğrular
        /// </summary>
        /// <param name="password">Doğrulanacak şifre</param>
        /// <param name="hashedPassword">Hash'lenmiş şifre</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return ServiceResult<bool>.Error("Şifre boş olamaz");

                if (string.IsNullOrWhiteSpace(hashedPassword))
                    return ServiceResult<bool>.Error("Hash'lenmiş şifre boş olamaz");

                var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                return ServiceResult<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Şifre doğrulama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Güçlü şifre oluşturur
        /// </summary>
        /// <param name="length">Şifre uzunluğu (varsayılan: 16)</param>
        /// <param name="includeSpecialChars">Özel karakterler dahil edilsin mi?</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateStrongPassword(int length = 16, bool includeSpecialChars = true)
        {
            try
            {
                if (length < 8)
                    return ServiceResult<string>.Error("Şifre uzunluğu en az 8 karakter olmalıdır");

                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                
                if (includeSpecialChars)
                    chars += "!@#$%^&*()_+-=[]{}|;:,.<>?";

                var random = new Random();
                var password = new StringBuilder();

                // En az bir büyük harf
                password.Append((char)random.Next('A', 'Z' + 1));
                
                // En az bir küçük harf
                password.Append((char)random.Next('a', 'z' + 1));
                
                // En az bir rakam
                password.Append((char)random.Next('0', '9' + 1));
                
                // En az bir özel karakter (eğer isteniyorsa)
                if (includeSpecialChars)
                {
                    var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
                    password.Append(specialChars[random.Next(specialChars.Length)]);
                }

                // Kalan karakterleri rastgele ekle
                for (int i = password.Length; i < length; i++)
                {
                    password.Append(chars[random.Next(chars.Length)]);
                }

                // Karakterleri karıştır
                var passwordArray = password.ToString().ToCharArray();
                for (int i = passwordArray.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
                }

                return ServiceResult<string>.Success(new string(passwordArray));
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Güçlü şifre oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifre güvenliğini kontrol eder
        /// </summary>
        /// <param name="password">Kontrol edilecek şifre</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<PasswordStrengthResult> CheckPasswordStrength(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return ServiceResult<PasswordStrengthResult>.Error("Şifre boş olamaz");

                var result = new PasswordStrengthResult
                {
                    Score = 0,
                    Feedback = new List<string>()
                };

                // Uzunluk kontrolü
                if (password.Length >= 8) result.Score += 1;
                else result.Feedback.Add("Şifre en az 8 karakter olmalıdır");

                if (password.Length >= 12) result.Score += 1;
                if (password.Length >= 16) result.Score += 1;

                // Karakter çeşitliliği kontrolü
                if (password.Any(char.IsUpper)) result.Score += 1;
                else result.Feedback.Add("En az bir büyük harf içermelidir");

                if (password.Any(char.IsLower)) result.Score += 1;
                else result.Feedback.Add("En az bir küçük harf içermelidir");

                if (password.Any(char.IsDigit)) result.Score += 1;
                else result.Feedback.Add("En az bir rakam içermelidir");

                if (password.Any(c => !char.IsLetterOrDigit(c))) result.Score += 1;
                else result.Feedback.Add("En az bir özel karakter içermelidir");

                // Güvenlik seviyesi belirleme
                if (result.Score <= 2)
                {
                    result.Strength = PasswordStrength.Weak;
                    result.Feedback.Add("Şifre çok zayıf");
                }
                else if (result.Score <= 4)
                {
                    result.Strength = PasswordStrength.Fair;
                    result.Feedback.Add("Şifre orta seviyede");
                }
                else if (result.Score <= 6)
                {
                    result.Strength = PasswordStrength.Good;
                    result.Feedback.Add("Şifre iyi");
                }
                else
                {
                    result.Strength = PasswordStrength.Strong;
                    result.Feedback.Add("Şifre çok güçlü");
                }

                return ServiceResult<PasswordStrengthResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PasswordStrengthResult>.Error($"Şifre güvenlik kontrolü hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifre hash'ini yeniler (eski hash'i günceller)
        /// </summary>
        /// <param name="password">Mevcut şifre</param>
        /// <param name="oldHash">Eski hash</param>
        /// <param name="newWorkFactor">Yeni work factor</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> RehashPassword(string password, string oldHash, int newWorkFactor = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return ServiceResult<string>.Error("Şifre boş olamaz");

                if (string.IsNullOrWhiteSpace(oldHash))
                    return ServiceResult<string>.Error("Eski hash boş olamaz");

                // Önce şifreyi doğrula
                var verifyResult = VerifyPassword(password, oldHash);
                if (!verifyResult.IsSuccess)
                    return ServiceResult<string>.Error(verifyResult.ErrorMessages.First());

                if (!verifyResult.Data)
                    return ServiceResult<string>.Error("Şifre yanlış");

                // Yeni hash oluştur
                var newHash = HashPassword(password, newWorkFactor);
                return newHash;
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Şifre yeniden hashleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifre hash'inin güncel olup olmadığını kontrol eder
        /// </summary>
        /// <param name="hashedPassword">Hash'lenmiş şifre</param>
        /// <param name="workFactor">Beklenen work factor</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> NeedsRehash(string hashedPassword, int workFactor = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hashedPassword))
                    return ServiceResult<bool>.Error("Hash'lenmiş şifre boş olamaz");

                var needsRehash = BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, workFactor);
                return ServiceResult<bool>.Success(needsRehash);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Hash kontrol hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Şifre hash'inden work factor'ı çıkarır
        /// </summary>
        /// <param name="hashedPassword">Hash'lenmiş şifre</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<int> GetWorkFactor(string hashedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hashedPassword))
                    return ServiceResult<int>.Error("Hash'lenmiş şifre boş olamaz");

                // BCrypt.Net-Next'te GetHashInfo metodu yok, bu yüzden hash'ten work factor'ı parse ediyoruz
                if (hashedPassword.StartsWith("$2a$") || hashedPassword.StartsWith("$2b$") || hashedPassword.StartsWith("$2y$"))
                {
                    var parts = hashedPassword.Split('$');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out var workFactor))
                    {
                        return ServiceResult<int>.Success(workFactor);
                    }
                }

                return ServiceResult<int>.Error("Geçersiz BCrypt hash formatı");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"Work factor çıkarma hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Şifre güvenlik sonucu
    /// </summary>
    public class PasswordStrengthResult
    {
        /// <summary>
        /// Güvenlik skoru (0-7)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Güvenlik seviyesi
        /// </summary>
        public PasswordStrength Strength { get; set; }

        /// <summary>
        /// Geri bildirim mesajları
        /// </summary>
        public List<string> Feedback { get; set; } = new();
    }

    /// <summary>
    /// Şifre güvenlik seviyesi
    /// </summary>
    public enum PasswordStrength
    {
        /// <summary>
        /// Zayıf
        /// </summary>
        Weak = 0,

        /// <summary>
        /// Orta
        /// </summary>
        Fair = 1,

        /// <summary>
        /// İyi
        /// </summary>
        Good = 2,

        /// <summary>
        /// Güçlü
        /// </summary>
        Strong = 3
    }
}
