using System.Security.Cryptography;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.Security
{
    /// <summary>
    /// Şifreleme işlemleri için helper sınıfı
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>
        /// AES ile veri şifreler
        /// </summary>
        /// <param name="plainText">Şifrelenecek metin</param>
        /// <param name="key">Şifreleme anahtarı (32 byte)</param>
        /// <param name="iv">Initialization vector (16 byte)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> EncryptAes(string plainText, byte[] key, byte[] iv)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return ServiceResult<string>.Error("Şifrelenecek metin boş olamaz");

                if (key == null || key.Length != 32)
                    return ServiceResult<string>.Error("Anahtar 32 byte olmalıdır");

                if (iv == null || iv.Length != 16)
                    return ServiceResult<string>.Error("IV 16 byte olmalıdır");

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);

                swEncrypt.Write(plainText);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();

                var encrypted = msEncrypt.ToArray();
                var result = Convert.ToBase64String(encrypted);

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"AES şifreleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// AES ile şifrelenmiş veriyi çözer
        /// </summary>
        /// <param name="cipherText">Şifrelenmiş metin (Base64)</param>
        /// <param name="key">Şifreleme anahtarı (32 byte)</param>
        /// <param name="iv">Initialization vector (16 byte)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> DecryptAes(string cipherText, byte[] key, byte[] iv)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return ServiceResult<string>.Error("Şifrelenmiş metin boş olamaz");

                if (key == null || key.Length != 32)
                    return ServiceResult<string>.Error("Anahtar 32 byte olmalıdır");

                if (iv == null || iv.Length != 16)
                    return ServiceResult<string>.Error("IV 16 byte olmalıdır");

                var cipherBytes = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                var decrypted = srDecrypt.ReadToEnd();
                return ServiceResult<string>.Success(decrypted);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"AES çözme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Güvenli anahtar ve IV oluşturur
        /// </summary>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<EncryptionKeyPair> GenerateAesKeyPair()
        {
            try
            {
                using var aes = Aes.Create();
                aes.GenerateKey();
                aes.GenerateIV();

                var keyPair = new EncryptionKeyPair
                {
                    Key = aes.Key,
                    IV = aes.IV
                };

                return ServiceResult<EncryptionKeyPair>.Success(keyPair);
            }
            catch (Exception ex)
            {
                return ServiceResult<EncryptionKeyPair>.Error($"Anahtar çifti oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// String anahtar ve IV oluşturur (Base64 formatında)
        /// </summary>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<StringEncryptionKeyPair> GenerateStringKeyPair()
        {
            try
            {
                var keyPairResult = GenerateAesKeyPair();
                if (!keyPairResult.IsSuccess)
                    return ServiceResult<StringEncryptionKeyPair>.Error(keyPairResult.ErrorMessages.First());

                if (keyPairResult.Data == null)
                    return ServiceResult<StringEncryptionKeyPair>.Error("Anahtar çifti oluşturulamadı");

                var stringKeyPair = new StringEncryptionKeyPair
                {
                    Key = Convert.ToBase64String(keyPairResult.Data.Key),
                    IV = Convert.ToBase64String(keyPairResult.Data.IV)
                };

                return ServiceResult<StringEncryptionKeyPair>.Success(stringKeyPair);
            }
            catch (Exception ex)
            {
                return ServiceResult<StringEncryptionKeyPair>.Error($"String anahtar çifti oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// String anahtar ve IV ile AES şifreleme yapar
        /// </summary>
        /// <param name="plainText">Şifrelenecek metin</param>
        /// <param name="base64Key">Base64 formatında anahtar</param>
        /// <param name="base64IV">Base64 formatında IV</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> EncryptAesString(string plainText, string base64Key, string base64IV)
        {
            try
            {
                var key = Convert.FromBase64String(base64Key);
                var iv = Convert.FromBase64String(base64IV);

                return EncryptAes(plainText, key, iv);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"String AES şifreleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// String anahtar ve IV ile AES çözme yapar
        /// </summary>
        /// <param name="cipherText">Şifrelenmiş metin</param>
        /// <param name="base64Key">Base64 formatında anahtar</param>
        /// <param name="base64IV">Base64 formatında IV</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> DecryptAesString(string cipherText, string base64Key, string base64IV)
        {
            try
            {
                var key = Convert.FromBase64String(base64Key);
                var iv = Convert.FromBase64String(base64IV);

                return DecryptAes(cipherText, key, iv);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"String AES çözme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// SHA256 hash oluşturur
        /// </summary>
        /// <param name="input">Hash'lenecek metin</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateSha256Hash(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return ServiceResult<string>.Error("Hash'lenecek metin boş olamaz");

                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                var result = Convert.ToBase64String(hash);

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"SHA256 hash hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// SHA512 hash oluşturur
        /// </summary>
        /// <param name="input">Hash'lenecek metin</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateSha512Hash(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return ServiceResult<string>.Error("Hash'lenecek metin boş olamaz");

                using var sha512 = SHA512.Create();
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha512.ComputeHash(bytes);
                var result = Convert.ToBase64String(hash);

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"SHA512 hash hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// MD5 hash oluşturur (güvenlik için önerilmez)
        /// </summary>
        /// <param name="input">Hash'lenecek metin</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateMd5Hash(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return ServiceResult<string>.Error("Hash'lenecek metin boş olamaz");

                using var md5 = MD5.Create();
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5.ComputeHash(bytes);
                var result = Convert.ToBase64String(hash);

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"MD5 hash hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Güvenli rastgele sayı oluşturur
        /// </summary>
        /// <param name="minValue">Minimum değer</param>
        /// <param name="maxValue">Maksimum değer</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<int> GenerateSecureRandomNumber(int minValue, int maxValue)
        {
            try
            {
                if (minValue >= maxValue)
                    return ServiceResult<int>.Error("Minimum değer maksimum değerden küçük olmalıdır");

                var randomNumber = RandomNumberGenerator.GetInt32(minValue, maxValue);
                return ServiceResult<int>.Success(randomNumber);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"Güvenli rastgele sayı oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Güvenli rastgele string oluşturur
        /// </summary>
        /// <param name="length">String uzunluğu</param>
        /// <param name="includeSpecialChars">Özel karakterler dahil edilsin mi?</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateSecureRandomString(int length, bool includeSpecialChars = false)
        {
            try
            {
                if (length <= 0)
                    return ServiceResult<string>.Error("Uzunluk pozitif olmalıdır");

                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                if (includeSpecialChars)
                    chars += "!@#$%^&*()_+-=[]{}|;:,.<>?";

                var result = new StringBuilder();
                var randomBytes = new byte[length];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }

                for (int i = 0; i < length; i++)
                {
                    result.Append(chars[randomBytes[i] % chars.Length]);
                }

                return ServiceResult<string>.Success(result.ToString());
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Güvenli rastgele string oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Veri bütünlüğü için HMAC oluşturur
        /// </summary>
        /// <param name="data">Veri</param>
        /// <param name="key">Anahtar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateHmac(string data, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return ServiceResult<string>.Error("Veri boş olamaz");

                if (string.IsNullOrEmpty(key))
                    return ServiceResult<string>.Error("Anahtar boş olamaz");

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hash = hmac.ComputeHash(dataBytes);
                var result = Convert.ToBase64String(hash);

                return ServiceResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"HMAC oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// HMAC doğrulaması yapar
        /// </summary>
        /// <param name="data">Veri</param>
        /// <param name="key">Anahtar</param>
        /// <param name="expectedHmac">Beklenen HMAC</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> VerifyHmac(string data, string key, string expectedHmac)
        {
            try
            {
                var hmacResult = GenerateHmac(data, key);
                if (!hmacResult.IsSuccess)
                    return ServiceResult<bool>.Error(hmacResult.ErrorMessages.First());

                var isValid = hmacResult.Data == expectedHmac;
                return ServiceResult<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"HMAC doğrulama hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Şifreleme anahtar çifti
    /// </summary>
    public class EncryptionKeyPair
    {
        /// <summary>
        /// Şifreleme anahtarı
        /// </summary>
        public byte[] Key { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Initialization vector
        /// </summary>
        public byte[] IV { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// String formatında şifreleme anahtar çifti
    /// </summary>
    public class StringEncryptionKeyPair
    {
        /// <summary>
        /// Base64 formatında şifreleme anahtarı
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Base64 formatında initialization vector
        /// </summary>
        public string IV { get; set; } = string.Empty;
    }
}
