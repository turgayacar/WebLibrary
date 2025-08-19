using WebLibrary.Models;

namespace WebLibrary.Captcha
{
    /// <summary>
    /// CAPTCHA sağlayıcı interface'i
    /// </summary>
    public interface ICaptchaProvider
    {
        /// <summary>
        /// CAPTCHA türü
        /// </summary>
        CaptchaType Type { get; }

        /// <summary>
        /// CAPTCHA oluşturur
        /// </summary>
        /// <param name="options">CAPTCHA seçenekleri</param>
        /// <returns>CAPTCHA sonucu</returns>
        ServiceResult<CaptchaResult> GenerateCaptcha(CaptchaOptions options);

        /// <summary>
        /// CAPTCHA'yı doğrular
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <param name="userInput">Kullanıcı girişi</param>
        /// <returns>Doğrulama sonucu</returns>
        Task<ServiceResult<bool>> ValidateCaptcha(string captchaId, string userInput);

        /// <summary>
        /// CAPTCHA'yı temizler
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID</param>
        /// <returns>Başarı durumu</returns>
        ServiceResult<bool> CleanupCaptcha(string captchaId);
    }
}
