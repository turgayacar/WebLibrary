using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebLibrary.Models;

namespace WebLibrary.Helpers
{
    /// <summary>
    /// HTTP client işlemleri için helper sınıfı
    /// </summary>
    public static class HttpClientHelper
    {
        private static readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions;
        
        static HttpClientHelper()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Global.ApiTimeoutSeconds)
            };
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
        
        /// <summary>
        /// GET isteği yapar ve generic tip döndürür
        /// </summary>
        /// <typeparam name="T">Döndürülecek tip</typeparam>
        /// <param name="url">Endpoint URL</param>
        /// <param name="headers">Ek HTTP headers</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<T>> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return ServiceResult<T>.Success(result!);
                }
                
                return ServiceResult<T>.Error($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"GET isteği başarısız: {ex.Message}");
            }
        }
        
        /// <summary>
        /// POST isteği yapar ve generic tip döndürür
        /// </summary>
        /// <typeparam name="T">Döndürülecek tip</typeparam>
        /// <param name="url">Endpoint URL</param>
        /// <param name="data">Gönderilecek veri</param>
        /// <param name="headers">Ek HTTP headers</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<T>> PostAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    return ServiceResult<T>.Success(result!);
                }
                
                return ServiceResult<T>.Error($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"POST isteği başarısız: {ex.Message}");
            }
        }
        
        /// <summary>
        /// PUT isteği yapar ve generic tip döndürür
        /// </summary>
        /// <typeparam name="T">Döndürülecek tip</typeparam>
        /// <param name="url">Endpoint URL</param>
        /// <param name="data">Gönderilecek veri</param>
        /// <param name="headers">Ek HTTP headers</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult<T>> PutAsync<T>(string url, object data, Dictionary<string, string>? headers = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    return ServiceResult<T>.Success(result!);
                }
                
                return ServiceResult<T>.Error($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"PUT isteği başarısız: {ex.Message}");
            }
        }
        
        /// <summary>
        /// DELETE isteği yapar
        /// </summary>
        /// <param name="url">Endpoint URL</param>
        /// <param name="headers">Ek HTTP headers</param>
        /// <returns>ServiceResult</returns>
        public static async Task<ServiceResult> DeleteAsync(string url, Dictionary<string, string>? headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return ServiceResult.Success();
                }
                
                return ServiceResult.Error($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                return ServiceResult.Error($"DELETE isteği başarısız: {ex.Message}");
            }
        }
        
        /// <summary>
        /// HTTP client'ı dispose eder
        /// </summary>
        public static void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
