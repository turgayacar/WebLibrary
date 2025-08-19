using WebLibrary.Models;
using System.Diagnostics;
using System.Text;

namespace WebLibrary.FileMedia
{
    /// <summary>
    /// Media conversion için helper sınıfı
    /// </summary>
    public static class MediaConversionHelper
    {
        /// <summary>
        /// Base64 string'i dosyaya dönüştürür
        /// </summary>
        /// <param name="base64String">Base64 string</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertBase64ToFile(string base64String, string outputPath)
        {
            try
            {
                if (string.IsNullOrEmpty(base64String))
                    return ServiceResult<bool>.Error("Base64 string boş olamaz");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Data URL formatını kontrol et (data:image/jpeg;base64,...)
                if (base64String.StartsWith("data:"))
                {
                    var commaIndex = base64String.IndexOf(',');
                    if (commaIndex > 0)
                        base64String = base64String.Substring(commaIndex + 1);
                }

                var bytes = Convert.FromBase64String(base64String);
                File.WriteAllBytes(outputPath, bytes);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Base64 dosya dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosyayı Base64 string'e dönüştürür
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="includeDataUrl">Data URL formatında döndür</param>
        /// <returns>Base64 string</returns>
        public static ServiceResult<string> ConvertFileToBase64(string filePath, bool includeDataUrl = false)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<string>.Error($"Dosya bulunamadı: {filePath}");

                var bytes = File.ReadAllBytes(filePath);
                var base64String = Convert.ToBase64String(bytes);

                if (includeDataUrl)
                {
                    var mimeType = GetMimeType(filePath);
                    base64String = $"data:{mimeType};base64,{base64String}";
                }

                return ServiceResult<string>.Success(base64String);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Dosya Base64 dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Hex string'i dosyaya dönüştürür
        /// </summary>
        /// <param name="hexString">Hex string</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertHexToFile(string hexString, string outputPath)
        {
            try
            {
                if (string.IsNullOrEmpty(hexString))
                    return ServiceResult<bool>.Error("Hex string boş olamaz");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Hex string'i temizle
                hexString = hexString.Replace(" ", "").Replace("-", "").Replace("0x", "");

                if (hexString.Length % 2 != 0)
                    return ServiceResult<bool>.Error("Geçersiz hex string formatı");

                var bytes = new byte[hexString.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                File.WriteAllBytes(outputPath, bytes);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Hex dosya dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosyayı Hex string'e dönüştürür
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="format">Hex format</param>
        /// <returns>Hex string</returns>
        public static ServiceResult<string> ConvertFileToHex(string filePath, HexFormat format = HexFormat.Plain)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<string>.Error($"Dosya bulunamadı: {filePath}");

                var bytes = File.ReadAllBytes(filePath);
                var hexString = format switch
                {
                    HexFormat.Plain => BitConverter.ToString(bytes).Replace("-", ""),
                    HexFormat.WithDashes => BitConverter.ToString(bytes),
                    HexFormat.WithSpaces => BitConverter.ToString(bytes).Replace("-", " "),
                    HexFormat.WithPrefix => "0x" + BitConverter.ToString(bytes).Replace("-", ""),
                    _ => BitConverter.ToString(bytes).Replace("-", "")
                };

                return ServiceResult<string>.Success(hexString);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"Dosya Hex dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Metin dosyasını farklı encoding'e dönüştürür
        /// </summary>
        /// <param name="inputPath">Giriş dosya yolu</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <param name="sourceEncoding">Kaynak encoding</param>
        /// <param name="targetEncoding">Hedef encoding</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertTextEncoding(string inputPath, string outputPath, 
            Encoding sourceEncoding, Encoding targetEncoding)
        {
            try
            {
                if (!File.Exists(inputPath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {inputPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                var text = File.ReadAllText(inputPath, sourceEncoding);
                File.WriteAllText(outputPath, text, targetEncoding);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Encoding dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// JSON dosyasını XML'e dönüştürür
        /// </summary>
        /// <param name="jsonPath">JSON dosya yolu</param>
        /// <param name="xmlPath">XML çıkış yolu</param>
        /// <param name="rootElementName">XML root element adı</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertJsonToXml(string jsonPath, string xmlPath, string rootElementName = "root")
        {
            try
            {
                if (!File.Exists(jsonPath))
                    return ServiceResult<bool>.Error($"JSON dosyası bulunamadı: {jsonPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(xmlPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                var jsonContent = File.ReadAllText(jsonPath);
                var jsonDocument = System.Text.Json.JsonDocument.Parse(jsonContent);

                var xmlBuilder = new StringBuilder();
                xmlBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                xmlBuilder.AppendLine($"<{rootElementName}>");

                ConvertJsonElementToXml(jsonDocument.RootElement, xmlBuilder, 1);

                xmlBuilder.AppendLine($"</{rootElementName}>");

                File.WriteAllText(xmlPath, xmlBuilder.ToString());

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"JSON XML dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosya bilgilerini alır
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Dosya bilgileri</returns>
        public static ServiceResult<FileInfo> GetFileInfo(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<FileInfo>.Error($"Dosya bulunamadı: {filePath}");

                var fileInfo = new System.IO.FileInfo(filePath);
                var mediaFileInfo = new FileInfo
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Size = fileInfo.Length,
                    Extension = fileInfo.Extension,
                    CreatedDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime,
                    MimeType = GetMimeType(filePath),
                    IsReadOnly = fileInfo.IsReadOnly
                };

                return ServiceResult<FileInfo>.Success(mediaFileInfo);
            }
            catch (Exception ex)
            {
                return ServiceResult<FileInfo>.Error($"Dosya bilgisi alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosya uzantısından MIME type alır
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>MIME type</returns>
        private static string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".wav" => "audio/wav",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// JSON elementini XML'e dönüştürür (recursive)
        /// </summary>
        /// <param name="element">JSON element</param>
        /// <param name="xmlBuilder">XML builder</param>
        /// <param name="indentLevel">Indent seviyesi</param>
        private static void ConvertJsonElementToXml(System.Text.Json.JsonElement element, StringBuilder xmlBuilder, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);

            switch (element.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        xmlBuilder.AppendLine($"{indent}<{property.Name}>");
                        ConvertJsonElementToXml(property.Value, xmlBuilder, indentLevel + 1);
                        xmlBuilder.AppendLine($"{indent}</{property.Name}>");
                    }
                    break;

                case System.Text.Json.JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        xmlBuilder.AppendLine($"{indent}<item{index}>");
                        ConvertJsonElementToXml(item, xmlBuilder, indentLevel + 1);
                        xmlBuilder.AppendLine($"{indent}</item{index}>");
                        index++;
                    }
                    break;

                default:
                    var value = element.ToString();
                    xmlBuilder.AppendLine($"{indent}{value}");
                    break;
            }
        }

        /// <summary>
        /// Toplu dosya dönüştürme
        /// </summary>
        /// <param name="inputDirectory">Giriş dizini</param>
        /// <param name="outputDirectory">Çıkış dizini</param>
        /// <param name="operation">Dönüştürme işlemi</param>
        /// <param name="parameters">İşlem parametreleri</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<ConversionResult> BatchConvertFiles(string inputDirectory, string outputDirectory,
            ConversionOperation operation, Dictionary<string, object> parameters)
        {
            try
            {
                if (!Directory.Exists(inputDirectory))
                    return ServiceResult<ConversionResult>.Error($"Giriş dizini bulunamadı: {inputDirectory}");

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                var files = Directory.GetFiles(inputDirectory, "*.*", SearchOption.TopDirectoryOnly).ToList();

                var result = new ConversionResult
                {
                    TotalFiles = files.Count,
                    ProcessedFiles = 0,
                    FailedFiles = 0,
                    Errors = new List<string>()
                };

                foreach (var inputFile in files)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(inputFile);
                        var outputFile = operation switch
                        {
                            ConversionOperation.ToBase64 => Path.Combine(outputDirectory, $"{fileName}.txt"),
                            ConversionOperation.ToHex => Path.Combine(outputDirectory, $"{fileName}.hex"),
                            _ => Path.Combine(outputDirectory, $"{fileName}_converted")
                        };

                        var success = operation switch
                        {
                            ConversionOperation.ToBase64 => ConvertToBase64File(inputFile, outputFile),
                            ConversionOperation.ToHex => ConvertToHexFile(inputFile, outputFile),
                            _ => ServiceResult<bool>.Error("Desteklenmeyen işlem")
                        };

                        if (success.IsSuccess)
                            result.ProcessedFiles++;
                        else
                        {
                            result.FailedFiles++;
                            result.Errors.Add($"{inputFile}: {success.ErrorMessages.FirstOrDefault()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedFiles++;
                        result.Errors.Add($"{inputFile}: {ex.Message}");
                    }
                }

                return ServiceResult<ConversionResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<ConversionResult>.Error($"Toplu dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosyayı Base64 formatında dosyaya yazar
        /// </summary>
        /// <param name="inputFile">Giriş dosyası</param>
        /// <param name="outputFile">Çıkış dosyası</param>
        /// <returns>ServiceResult</returns>
        private static ServiceResult<bool> ConvertToBase64File(string inputFile, string outputFile)
        {
            var base64Result = ConvertFileToBase64(inputFile);
            if (!base64Result.IsSuccess)
                return ServiceResult<bool>.Error(base64Result.ErrorMessages.FirstOrDefault() ?? "Base64 dönüştürme hatası");

            File.WriteAllText(outputFile, base64Result.Data);
            return ServiceResult<bool>.Success(true);
        }

        /// <summary>
        /// Dosyayı Hex formatında dosyaya yazar
        /// </summary>
        /// <param name="inputFile">Giriş dosyası</param>
        /// <param name="outputFile">Çıkış dosyası</param>
        /// <returns>ServiceResult</returns>
        private static ServiceResult<bool> ConvertToHexFile(string inputFile, string outputFile)
        {
            var hexResult = ConvertFileToHex(inputFile);
            if (!hexResult.IsSuccess)
                return ServiceResult<bool>.Error(hexResult.ErrorMessages.FirstOrDefault() ?? "Hex dönüştürme hatası");

            File.WriteAllText(outputFile, hexResult.Data);
            return ServiceResult<bool>.Success(true);
        }
    }

    /// <summary>
    /// Hex formatları
    /// </summary>
    public enum HexFormat
    {
        Plain,        // ABCDEF123456
        WithDashes,   // AB-CD-EF-12-34-56
        WithSpaces,   // AB CD EF 12 34 56
        WithPrefix    // 0xABCDEF123456
    }

    /// <summary>
    /// Dönüştürme işlemleri
    /// </summary>
    public enum ConversionOperation
    {
        ToBase64,
        ToHex
    }

    /// <summary>
    /// Dosya bilgisi
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Dosya adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tam yol
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Dosya boyutu (byte)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Dosya uzantısı
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Değiştirilme tarihi
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// MIME type
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Salt okunur mu?
        /// </summary>
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Dönüştürme sonucu
    /// </summary>
    public class ConversionResult
    {
        /// <summary>
        /// Toplam dosya sayısı
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// İşlenen dosya sayısı
        /// </summary>
        public int ProcessedFiles { get; set; }

        /// <summary>
        /// Başarısız dosya sayısı
        /// </summary>
        public int FailedFiles { get; set; }

        /// <summary>
        /// Hata mesajları
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Başarı oranı
        /// </summary>
        public double SuccessRate => TotalFiles > 0 ? (double)ProcessedFiles / TotalFiles * 100 : 0;
    }
}
