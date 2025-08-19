using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WebLibrary.Models;

namespace WebLibrary.Security
{
    /// <summary>
    /// JWT token işlemleri için helper sınıfı
    /// </summary>
    public static class JwtTokenHelper
    {
        /// <summary>
        /// JWT token oluşturur
        /// </summary>
        /// <param name="claims">Token içindeki claim'ler</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <param name="issuer">Token yayınlayıcı</param>
        /// <param name="audience">Token hedef kitle</param>
        /// <param name="expirationMinutes">Geçerlilik süresi (dakika)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateToken(
            List<Claim> claims,
            string secretKey,
            string issuer = "WebLibrary",
            string audience = "WebLibraryUsers",
            int expirationMinutes = 60)
        {
            try
            {
                if (string.IsNullOrEmpty(secretKey))
                    return ServiceResult<string>.Error("Gizli anahtar boş olamaz");

                if (claims == null || !claims.Any())
                    return ServiceResult<string>.Error("Claim'ler boş olamaz");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);

                return ServiceResult<string>.Success(tokenString);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Token oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Kullanıcı bilgileri ile JWT token oluşturur
        /// </summary>
        /// <param name="userId">Kullanıcı ID'si</param>
        /// <param name="username">Kullanıcı adı</param>
        /// <param name="email">Email</param>
        /// <param name="roles">Roller</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <param name="expirationMinutes">Geçerlilik süresi (dakika)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> GenerateUserToken(
            int userId,
            string username,
            string email,
            List<string> roles,
            string secretKey,
            int expirationMinutes = 60)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("UserId", userId.ToString())
                };

                // Rolleri ekle
                if (roles != null && roles.Any())
                {
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                return GenerateToken(claims, secretKey, expirationMinutes: expirationMinutes);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Kullanıcı token oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// JWT token'ı doğrular
        /// </summary>
        /// <param name="token">Doğrulanacak token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <param name="issuer">Beklenen yayınlayıcı</param>
        /// <param name="audience">Beklenen hedef kitle</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ValidateToken(
            string token,
            string secretKey,
            string issuer = "WebLibrary",
            string audience = "WebLibraryUsers")
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return ServiceResult<bool>.Error("Token boş olamaz");

                if (string.IsNullOrEmpty(secretKey))
                    return ServiceResult<bool>.Error("Gizli anahtar boş olamaz");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return ServiceResult<bool>.Success(true);
            }
            catch (SecurityTokenExpiredException)
            {
                return ServiceResult<bool>.Error("Token süresi dolmuş");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return ServiceResult<bool>.Error("Token imzası geçersiz");
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                return ServiceResult<bool>.Error("Token yayınlayıcısı geçersiz");
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                return ServiceResult<bool>.Error("Token hedef kitlesi geçersiz");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Token doğrulama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Token'dan claim'leri çıkarır
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<Claim>> ExtractClaims(string token, string secretKey)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return ServiceResult<List<Claim>>.Error("Token boş olamaz");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var claims = principal.Claims.ToList();

                return ServiceResult<List<Claim>>.Success(claims);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Claim>>.Error($"Claim çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Token'dan kullanıcı ID'sini çıkarır
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<int> ExtractUserId(string token, string secretKey)
        {
            try
            {
                var claimsResult = ExtractClaims(token, secretKey);
                if (!claimsResult.IsSuccess)
                    return ServiceResult<int>.Error(claimsResult.ErrorMessages.First());

                var userIdClaim = claimsResult.Data?.FirstOrDefault(c => c.Type == "UserId" || c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return ServiceResult<int>.Error("Kullanıcı ID bulunamadı");

                if (!int.TryParse(userIdClaim.Value, out var userId))
                    return ServiceResult<int>.Error("Kullanıcı ID geçersiz format");

                return ServiceResult<int>.Success(userId);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"Kullanıcı ID çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Token'dan rolleri çıkarır
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<string>> ExtractRoles(string token, string secretKey)
        {
            try
            {
                var claimsResult = ExtractClaims(token, secretKey);
                if (!claimsResult.IsSuccess)
                    return ServiceResult<List<string>>.Error(claimsResult.ErrorMessages.First());

                var roleClaims = claimsResult.Data?.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                return ServiceResult<List<string>>.Success(roleClaims ?? new List<string>());
            }
            catch (Exception ex)
            {
                return ServiceResult<List<string>>.Error($"Rol çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Token'ın süresini kontrol eder
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> IsTokenExpired(string token, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken?.ValidTo == null)
                    return ServiceResult<bool>.Error("Token süre bilgisi bulunamadı");

                var isExpired = jwtToken.ValidTo < DateTime.UtcNow;
                return ServiceResult<bool>.Success(isExpired);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Token süre kontrolü hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Token'ın kalan süresini hesaplar
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<TimeSpan> GetRemainingTime(string token, string secretKey)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken?.ValidTo == null)
                    return ServiceResult<TimeSpan>.Error("Token süre bilgisi bulunamadı");

                var remainingTime = jwtToken.ValidTo - DateTime.UtcNow;
                return ServiceResult<TimeSpan>.Success(remainingTime);
            }
            catch (Exception ex)
            {
                return ServiceResult<TimeSpan>.Error($"Kalan süre hesaplama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Token'ı yeniler
        /// </summary>
        /// <param name="token">Eski token</param>
        /// <param name="secretKey">Gizli anahtar</param>
        /// <param name="expirationMinutes">Yeni geçerlilik süresi (dakika)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> RefreshToken(string token, string secretKey, int expirationMinutes = 60)
        {
            try
            {
                var claimsResult = ExtractClaims(token, secretKey);
                if (!claimsResult.IsSuccess)
                    return ServiceResult<string>.Error(claimsResult.ErrorMessages.First());

                // Süre ile ilgili claim'leri kaldır
                var claims = claimsResult.Data?.Where(c => 
                    c.Type != "exp" && 
                    c.Type != "iat" && 
                    c.Type != "nbf").ToList() ?? new List<Claim>();

                return GenerateToken(claims, secretKey, expirationMinutes: expirationMinutes);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Token yenileme hatası: {ex.Message}");
            }
        }
    }
}
