using System.IO.Compression;
using WebLibrary.Models;

namespace WebLibrary.FileMedia
{
    /// <summary>
    /// File compression için helper sınıfı
    /// </summary>
    public static class FileCompressionHelper
    {
        /// <summary>
        /// Dosyayı ZIP olarak sıkıştırır
        /// </summary>
        /// <param name="filePath">Sıkıştırılacak dosya yolu</param>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <param name="compressionLevel">Sıkıştırma seviyesi</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CompressFile(string filePath, string zipPath, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {filePath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(zipPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Mevcut ZIP dosyası varsa sil
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                var fileName = Path.GetFileName(filePath);
                archive.CreateEntryFromFile(filePath, fileName, compressionLevel);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Dosya sıkıştırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dizini ZIP olarak sıkıştırır
        /// </summary>
        /// <param name="directoryPath">Sıkıştırılacak dizin yolu</param>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <param name="compressionLevel">Sıkıştırma seviyesi</param>
        /// <param name="includeBaseDirectory">Ana dizini dahil et</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CompressDirectory(string directoryPath, string zipPath, 
            CompressionLevel compressionLevel = CompressionLevel.Optimal, bool includeBaseDirectory = false)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    return ServiceResult<bool>.Error($"Dizin bulunamadı: {directoryPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(zipPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Mevcut ZIP dosyası varsa sil
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(directoryPath, zipPath, compressionLevel, includeBaseDirectory);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Dizin sıkıştırma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ZIP dosyasını çıkarır
        /// </summary>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <param name="extractPath">Çıkarılacak dizin yolu</param>
        /// <param name="overwrite">Mevcut dosyaları üzerine yaz</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ExtractZip(string zipPath, string extractPath, bool overwrite = true)
        {
            try
            {
                if (!File.Exists(zipPath))
                    return ServiceResult<bool>.Error($"ZIP dosyası bulunamadı: {zipPath}");

                if (!Directory.Exists(extractPath))
                    Directory.CreateDirectory(extractPath);

                ZipFile.ExtractToDirectory(zipPath, extractPath, overwrite);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"ZIP çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ZIP dosyasındaki içeriği listeler
        /// </summary>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <returns>ZIP içeriği</returns>
        public static ServiceResult<List<ZipEntryInfo>> ListZipContents(string zipPath)
        {
            try
            {
                if (!File.Exists(zipPath))
                    return ServiceResult<List<ZipEntryInfo>>.Error($"ZIP dosyası bulunamadı: {zipPath}");

                var entries = new List<ZipEntryInfo>();

                using var archive = ZipFile.OpenRead(zipPath);
                foreach (var entry in archive.Entries)
                {
                    entries.Add(new ZipEntryInfo
                    {
                        Name = entry.Name,
                        FullName = entry.FullName,
                        Length = entry.Length,
                        CompressedLength = entry.CompressedLength,
                        LastWriteTime = entry.LastWriteTime,
                        CompressionRatio = entry.Length > 0 ? (1.0 - (double)entry.CompressedLength / entry.Length) * 100 : 0
                    });
                }

                return ServiceResult<List<ZipEntryInfo>>.Success(entries);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ZipEntryInfo>>.Error($"ZIP içerik listeleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ZIP dosyasına dosya ekler
        /// </summary>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <param name="filePath">Eklenecek dosya yolu</param>
        /// <param name="entryName">ZIP içindeki dosya adı</param>
        /// <param name="compressionLevel">Sıkıştırma seviyesi</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> AddFileToZip(string zipPath, string filePath, string? entryName = null, 
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<bool>.Error($"Dosya bulunamadı: {filePath}");

                entryName ??= Path.GetFileName(filePath);

                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
                
                // Aynı isimde entry varsa sil
                var existingEntry = archive.GetEntry(entryName);
                existingEntry?.Delete();

                archive.CreateEntryFromFile(filePath, entryName, compressionLevel);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"ZIP'e dosya ekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// ZIP dosyasından dosya çıkarır
        /// </summary>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <param name="entryName">Çıkarılacak dosya adı</param>
        /// <param name="extractPath">Çıkarılacak yol</param>
        /// <param name="overwrite">Mevcut dosyayı üzerine yaz</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ExtractFileFromZip(string zipPath, string entryName, string extractPath, bool overwrite = true)
        {
            try
            {
                if (!File.Exists(zipPath))
                    return ServiceResult<bool>.Error($"ZIP dosyası bulunamadı: {zipPath}");

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(extractPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using var archive = ZipFile.OpenRead(zipPath);
                var entry = archive.GetEntry(entryName);

                if (entry == null)
                    return ServiceResult<bool>.Error($"ZIP içinde dosya bulunamadı: {entryName}");

                if (File.Exists(extractPath) && !overwrite)
                    return ServiceResult<bool>.Error($"Dosya zaten mevcut: {extractPath}");

                entry.ExtractToFile(extractPath, overwrite);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"ZIP'den dosya çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosya sıkıştırma oranını hesaplar
        /// </summary>
        /// <param name="originalSize">Orijinal boyut</param>
        /// <param name="compressedSize">Sıkıştırılmış boyut</param>
        /// <returns>Sıkıştırma oranı (%)</returns>
        public static double CalculateCompressionRatio(long originalSize, long compressedSize)
        {
            if (originalSize == 0) return 0;
            return (1.0 - (double)compressedSize / originalSize) * 100;
        }

        /// <summary>
        /// Dosya boyutunu formatlar
        /// </summary>
        /// <param name="bytes">Byte cinsinden boyut</param>
        /// <returns>Formatlanmış boyut</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Toplu dosya sıkıştırma
        /// </summary>
        /// <param name="files">Sıkıştırılacak dosyalar</param>
        /// <param name="zipPath">ZIP dosya yolu</param>
        /// <param name="compressionLevel">Sıkıştırma seviyesi</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<CompressionResult> CompressMultipleFiles(List<string> files, string zipPath, 
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            try
            {
                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(zipPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Mevcut ZIP dosyası varsa sil
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                var result = new CompressionResult
                {
                    TotalFiles = files.Count,
                    ProcessedFiles = 0,
                    FailedFiles = 0,
                    OriginalSize = 0,
                    CompressedSize = 0,
                    Errors = new List<string>()
                };

                using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

                foreach (var filePath in files)
                {
                    try
                    {
                        if (!File.Exists(filePath))
                        {
                            result.FailedFiles++;
                            result.Errors.Add($"Dosya bulunamadı: {filePath}");
                            continue;
                        }

                        var fileInfo = new System.IO.FileInfo(filePath);
                        result.OriginalSize += fileInfo.Length;

                        var fileName = Path.GetFileName(filePath);
                        archive.CreateEntryFromFile(filePath, fileName, compressionLevel);
                        result.ProcessedFiles++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedFiles++;
                        result.Errors.Add($"{filePath}: {ex.Message}");
                    }
                }

                // ZIP dosya boyutunu al
                if (File.Exists(zipPath))
                {
                    var zipInfo = new System.IO.FileInfo(zipPath);
                    result.CompressedSize = zipInfo.Length;
                    result.CompressionRatio = CalculateCompressionRatio(result.OriginalSize, result.CompressedSize);
                }

                return ServiceResult<CompressionResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<CompressionResult>.Error($"Toplu sıkıştırma hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// ZIP entry bilgisi
    /// </summary>
    public class ZipEntryInfo
    {
        /// <summary>
        /// Dosya adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tam yol
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Orijinal boyut
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Sıkıştırılmış boyut
        /// </summary>
        public long CompressedLength { get; set; }

        /// <summary>
        /// Son değiştirilme tarihi
        /// </summary>
        public DateTimeOffset LastWriteTime { get; set; }

        /// <summary>
        /// Sıkıştırma oranı (%)
        /// </summary>
        public double CompressionRatio { get; set; }
    }

    /// <summary>
    /// Sıkıştırma sonucu
    /// </summary>
    public class CompressionResult
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
        /// Orijinal toplam boyut
        /// </summary>
        public long OriginalSize { get; set; }

        /// <summary>
        /// Sıkıştırılmış boyut
        /// </summary>
        public long CompressedSize { get; set; }

        /// <summary>
        /// Sıkıştırma oranı (%)
        /// </summary>
        public double CompressionRatio { get; set; }

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
