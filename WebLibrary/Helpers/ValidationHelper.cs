using System.ComponentModel.DataAnnotations;
using System.Reflection;
using WebLibrary.Models;

namespace WebLibrary.Helpers
{
    /// <summary>
    /// Veri doğrulama işlemleri için helper sınıfı
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Generic model doğrulaması yapar
        /// </summary>
        /// <typeparam name="T">Doğrulanacak model tipi</typeparam>
        /// <param name="model">Doğrulanacak model</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult ValidateModel<T>(T model) where T : class
        {
            if (model == null)
            {
                return ServiceResult.Error("Model null olamaz");
            }
            
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            
            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
            {
                var errorMessages = validationResults.Select(v => v.ErrorMessage ?? "Bilinmeyen hata").Where(m => !string.IsNullOrEmpty(m)).ToList();
                return ServiceResult.Error(errorMessages);
            }
            
            return ServiceResult.Success();
        }
        
        /// <summary>
        /// Generic model doğrulaması yapar ve hataları döndürür
        /// </summary>
        /// <typeparam name="T">Doğrulanacak model tipi</typeparam>
        /// <param name="model">Doğrulanacak model</param>
        /// <returns>ValidationResult listesi</returns>
        public static List<ValidationResult> GetValidationErrors<T>(T model) where T : class
        {
            if (model == null)
            {
                return new List<ValidationResult> { new ValidationResult("Model null olamaz") };
            }
            
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            
            return validationResults;
        }
        
        /// <summary>
        /// Belirli bir property'nin doğrulamasını yapar
        /// </summary>
        /// <typeparam name="T">Model tipi</typeparam>
        /// <param name="model">Doğrulanacak model</param>
        /// <param name="propertyName">Property adı</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult ValidateProperty<T>(T model, string propertyName) where T : class
        {
            if (model == null)
            {
                return ServiceResult.Error("Model null olamaz");
            }
            
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                return ServiceResult.Error($"Property '{propertyName}' bulunamadı");
            }
            
            var value = property.GetValue(model);
            var validationContext = new ValidationContext(model) { MemberName = propertyName };
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateProperty(value, validationContext, validationResults))
            {
                var errorMessages = validationResults.Select(v => v.ErrorMessage ?? "Bilinmeyen hata").Where(m => !string.IsNullOrEmpty(m)).ToList();
                return ServiceResult.Error(errorMessages);
            }
            
            return ServiceResult.Success();
        }
        
        /// <summary>
        /// Email formatını doğrular
        /// </summary>
        /// <param name="email">Doğrulanacak email</param>
        /// <returns>Geçerli mi?</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// TC Kimlik numarasını doğrular
        /// </summary>
        /// <param name="tcKimlik">Doğrulanacak TC Kimlik</param>
        /// <returns>Geçerli mi?</returns>
        public static bool IsValidTcKimlik(string tcKimlik)
        {
            if (string.IsNullOrWhiteSpace(tcKimlik) || tcKimlik.Length != 11)
                return false;
            
            if (!tcKimlik.All(char.IsDigit))
                return false;
            
            if (tcKimlik[0] == '0')
                return false;
            
            var digits = tcKimlik.Select(c => int.Parse(c.ToString())).ToArray();
            
            // 1, 3, 5, 7, 9. hanelerin toplamının 7 katından, 2, 4, 6, 8. hanelerin toplamı çıkartılıp 10'a bölümünden kalan, yani Mod10'u bize 10. haneyi verir.
            var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
            var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
            var digit10 = ((oddSum * 7) - evenSum) % 10;
            
            if (digits[9] != digit10)
                return false;
            
            // İlk 10 hanenin toplamından elde edilen sonucun 10'a bölümünden kalan, yani Mod10'u bize 11. haneyi verir.
            var sum = digits.Take(10).Sum();
            var digit11 = sum % 10;
            
            return digits[10] == digit11;
        }
        
        /// <summary>
        /// Telefon numarasını doğrular (Türkiye formatı)
        /// </summary>
        /// <param name="phone">Doğrulanacak telefon</param>
        /// <returns>Geçerli mi?</returns>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            
            // Sadece rakamları al
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            
            // Türkiye telefon numarası: 10 haneli (5xx xxx xx xx) veya 11 haneli (05xx xxx xx xx)
            if (digits.Length == 10)
            {
                return digits.StartsWith("5");
            }
            else if (digits.Length == 11)
            {
                return digits.StartsWith("05");
            }
            
            return false;
        }
        
        /// <summary>
        /// Şifre güvenliğini kontrol eder
        /// </summary>
        /// <param name="password">Doğrulanacak şifre</param>
        /// <returns>Güvenli mi?</returns>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;
            
            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));
            
            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }
        
        /// <summary>
        /// URL formatını doğrular
        /// </summary>
        /// <param name="url">Doğrulanacak URL</param>
        /// <returns>Geçerli mi?</returns>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
