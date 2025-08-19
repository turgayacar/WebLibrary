using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using WebLibrary.Models;

namespace WebLibrary.FileMedia
{
    /// <summary>
    /// Image processing için helper sınıfı
    /// </summary>
    public static class ImageProcessingHelper
    {
        /// <summary>
        /// Resim boyutunu değiştirir
        /// </summary>
        /// <param name="inputPath">Giriş dosya yolu</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <param name="width">Yeni genişlik</param>
        /// <param name="height">Yeni yükseklik</param>
        /// <param name="maintainAspectRatio">En-boy oranını koru</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ResizeImage(string inputPath, string outputPath, int width, int height, bool maintainAspectRatio = true)
        {
            try
            {
                if (!File.Exists(inputPath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {inputPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using var image = Image.Load(inputPath);
                
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = maintainAspectRatio ? ResizeMode.Max : ResizeMode.Stretch
                };

                image.Mutate(x => x.Resize(resizeOptions));
                image.Save(outputPath);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Resim boyutlandırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Resim formatını dönüştürür
        /// </summary>
        /// <param name="inputPath">Giriş dosya yolu</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <param name="format">Çıkış formatı</param>
        /// <param name="quality">Kalite (JPEG için 1-100)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertImageFormat(string inputPath, string outputPath, ImageFormat format, int quality = 85)
        {
            try
            {
                if (!File.Exists(inputPath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {inputPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using var image = Image.Load(inputPath);

                switch (format)
                {
                    case ImageFormat.JPEG:
                        var jpegEncoder = new JpegEncoder { Quality = quality };
                        image.SaveAsJpeg(outputPath, jpegEncoder);
                        break;
                    case ImageFormat.PNG:
                        image.SaveAsPng(outputPath);
                        break;
                    case ImageFormat.WEBP:
                        var webpEncoder = new WebpEncoder { Quality = quality };
                        image.SaveAsWebp(outputPath, webpEncoder);
                        break;
                    default:
                        return ServiceResult<bool>.Error($"Desteklenmeyen format: {format}");
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Format dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Resmi döndürür
        /// </summary>
        /// <param name="inputPath">Giriş dosya yolu</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <param name="degrees">Derece (90, 180, 270)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> RotateImage(string inputPath, string outputPath, float degrees)
        {
            try
            {
                if (!File.Exists(inputPath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {inputPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using var image = Image.Load(inputPath);
                image.Mutate(x => x.Rotate(degrees));
                image.Save(outputPath);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Resim döndürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Resmi kırpar
        /// </summary>
        /// <param name="inputPath">Giriş dosya yolu</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <param name="x">X koordinatı</param>
        /// <param name="y">Y koordinatı</param>
        /// <param name="width">Genişlik</param>
        /// <param name="height">Yükseklik</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CropImage(string inputPath, string outputPath, int x, int y, int width, int height)
        {
            try
            {
                if (!File.Exists(inputPath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {inputPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using var image = Image.Load(inputPath);
                var cropRectangle = new Rectangle(x, y, width, height);
                image.Mutate(i => i.Crop(cropRectangle));
                image.Save(outputPath);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Resim kırpma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Resim bilgilerini alır
        /// </summary>
        /// <param name="imagePath">Resim dosya yolu</param>
        /// <returns>Resim bilgileri</returns>
        public static ServiceResult<ImageInfo> GetImageInfo(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                    return ServiceResult<ImageInfo>.Error($"Dosya bulunamadı: {imagePath}");

                using var image = Image.Load(imagePath);
                var fileInfo = new System.IO.FileInfo(imagePath);

                var imageInfo = new ImageInfo
                {
                    Width = image.Width,
                    Height = image.Height,
                    Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                    FileSize = fileInfo.Length,
                    FilePath = imagePath,
                    AspectRatio = (double)image.Width / image.Height
                };

                return ServiceResult<ImageInfo>.Success(imageInfo);
            }
            catch (Exception ex)
            {
                return ServiceResult<ImageInfo>.Error($"Resim bilgisi alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Thumbnail oluşturur
        /// </summary>
        /// <param name="inputPath">Giriş dosya yolu</param>
        /// <param name="outputPath">Çıkış dosya yolu</param>
        /// <param name="maxSize">Maksimum boyut</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CreateThumbnail(string inputPath, string outputPath, int maxSize = 150)
        {
            try
            {
                if (!File.Exists(inputPath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {inputPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using var image = Image.Load(inputPath);
                
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(maxSize, maxSize),
                    Mode = ResizeMode.Max
                };

                image.Mutate(x => x.Resize(resizeOptions));
                image.SaveAsJpeg(outputPath, new JpegEncoder { Quality = 80 });

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Thumbnail oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Toplu resim işleme
        /// </summary>
        /// <param name="inputDirectory">Giriş dizini</param>
        /// <param name="outputDirectory">Çıkış dizini</param>
        /// <param name="operation">İşlem tipi</param>
        /// <param name="parameters">İşlem parametreleri</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<BatchProcessResult> BatchProcessImages(string inputDirectory, string outputDirectory, 
            ImageOperation operation, Dictionary<string, object> parameters)
        {
            try
            {
                if (!Directory.Exists(inputDirectory))
                    return ServiceResult<BatchProcessResult>.Error($"Giriş dizini bulunamadı: {inputDirectory}");

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                var imageFiles = Directory.GetFiles(inputDirectory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => IsImageFile(file)).ToList();

                var result = new BatchProcessResult
                {
                    TotalFiles = imageFiles.Count,
                    ProcessedFiles = 0,
                    FailedFiles = 0,
                    Errors = new List<string>()
                };

                foreach (var inputFile in imageFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(inputFile);
                        var extension = Path.GetExtension(inputFile);
                        var outputFile = Path.Combine(outputDirectory, $"{fileName}_processed{extension}");

                        var success = operation switch
                        {
                            ImageOperation.Resize => ResizeImage(inputFile, outputFile, 
                                (int)parameters["width"], (int)parameters["height"], 
                                parameters.ContainsKey("maintainAspectRatio") && (bool)parameters["maintainAspectRatio"]),
                            ImageOperation.Thumbnail => CreateThumbnail(inputFile, outputFile, 
                                parameters.ContainsKey("maxSize") ? (int)parameters["maxSize"] : 150),
                            ImageOperation.Convert => ConvertImageFormat(inputFile, outputFile,
                                (ImageFormat)parameters["format"], 
                                parameters.ContainsKey("quality") ? (int)parameters["quality"] : 85),
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

                return ServiceResult<BatchProcessResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<BatchProcessResult>.Error($"Toplu işlem hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosyanın resim dosyası olup olmadığını kontrol eder
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>True eğer resim dosyasıysa</returns>
        private static bool IsImageFile(string filePath)
        {
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extensions.Contains(extension);
        }
    }

    /// <summary>
    /// Resim formatları
    /// </summary>
    public enum ImageFormat
    {
        JPEG,
        PNG,
        WEBP
    }

    /// <summary>
    /// Resim işlem tipleri
    /// </summary>
    public enum ImageOperation
    {
        Resize,
        Thumbnail,
        Convert
    }

    /// <summary>
    /// Resim bilgisi
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// Genişlik
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Yükseklik
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Dosya boyutu (byte)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Dosya yolu
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// En-boy oranı
        /// </summary>
        public double AspectRatio { get; set; }
    }

    /// <summary>
    /// Toplu işlem sonucu
    /// </summary>
    public class BatchProcessResult
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
