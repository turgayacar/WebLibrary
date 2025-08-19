using WebLibrary.Models;
using System.Text;

namespace WebLibrary.FileMedia
{
    /// <summary>
    /// Document processing için helper sınıfı
    /// </summary>
    public static class DocumentProcessingHelper
    {
        /// <summary>
        /// PDF dosyasından metin çıkarır (basit metin dosyası olarak)
        /// </summary>
        /// <param name="pdfPath">PDF dosya yolu</param>
        /// <returns>Çıkarılan metin</returns>
        public static ServiceResult<string> ExtractTextFromPdf(string pdfPath)
        {
            try
            {
                if (!File.Exists(pdfPath))
                    return ServiceResult<string>.Error($"PDF dosyası bulunamadı: {pdfPath}");

                // Basit metin dosyası olarak oku
                var text = File.ReadAllText(pdfPath, Encoding.UTF8);
                return ServiceResult<string>.Success(text);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"PDF metin çıkarma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Metin dosyasından PDF oluşturur (basit metin dosyası olarak)
        /// </summary>
        /// <param name="textContent">Metin içeriği</param>
        /// <param name="outputPath">Çıkış PDF yolu</param>
        /// <param name="title">Belge başlığı</param>
        /// <param name="author">Yazar</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CreatePdfFromText(string textContent, string outputPath, string? title = null, string? author = null)
        {
            try
            {
                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Basit metin dosyası olarak kaydet
                var content = new StringBuilder();
                if (!string.IsNullOrEmpty(title))
                    content.AppendLine($"Title: {title}");
                if (!string.IsNullOrEmpty(author))
                    content.AppendLine($"Author: {author}");
                content.AppendLine($"Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                content.AppendLine();
                content.AppendLine(textContent);

                File.WriteAllText(outputPath, content.ToString(), Encoding.UTF8);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"PDF oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// PDF dosyalarını birleştirir
        /// </summary>
        /// <param name="inputPdfs">Birleştirilecek PDF dosyaları</param>
        /// <param name="outputPath">Çıkış PDF yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> MergePdfs(List<string> inputPdfs, string outputPath)
        {
            try
            {
                if (!inputPdfs.Any())
                    return ServiceResult<bool>.Error("Birleştirilecek PDF dosyası belirtilmedi");

                // Tüm dosyaların var olduğunu kontrol et
                foreach (var pdf in inputPdfs)
                {
                    if (!File.Exists(pdf))
                        return ServiceResult<bool>.Error($"PDF dosyası bulunamadı: {pdf}");
                }

                // Çıkış dizini yoksa oluştur
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                // Basit metin dosyası olarak birleştir
                var mergedContent = new StringBuilder();
                mergedContent.AppendLine($"Merged PDF - Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                mergedContent.AppendLine($"Total files: {inputPdfs.Count}");
                mergedContent.AppendLine();

                foreach (var pdfPath in inputPdfs)
                {
                    mergedContent.AppendLine($"--- File: {Path.GetFileName(pdfPath)} ---");
                    try
                    {
                        var fileContent = File.ReadAllText(pdfPath, Encoding.UTF8);
                        mergedContent.AppendLine(fileContent);
                    }
                    catch
                    {
                        mergedContent.AppendLine("[Binary content - cannot read as text]");
                    }
                    mergedContent.AppendLine();
                }

                File.WriteAllText(outputPath, mergedContent.ToString(), Encoding.UTF8);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"PDF birleştirme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// PDF dosyasını böler
        /// </summary>
        /// <param name="inputPdf">Bölünecek PDF dosyası</param>
        /// <param name="outputDirectory">Çıkış dizini</param>
        /// <param name="pageRanges">Sayfa aralıkları (örn: "1-5,10-15")</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<string>> SplitPdf(string inputPdf, string outputDirectory, string? pageRanges = null)
        {
            try
            {
                if (!File.Exists(inputPdf))
                    return ServiceResult<List<string>>.Error($"PDF dosyası bulunamadı: {inputPdf}");

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                var outputFiles = new List<string>();
                var fileContent = File.ReadAllText(inputPdf, Encoding.UTF8);
                var lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (string.IsNullOrEmpty(pageRanges))
                {
                    // Her 50 satırı ayrı dosya olarak kaydet
                    var linesPerFile = 50;
                    var fileCount = (int)Math.Ceiling((double)lines.Length / linesPerFile);
                    
                    for (int i = 0; i < fileCount; i++)
                    {
                        var outputPath = Path.Combine(outputDirectory, $"part_{i + 1}.txt");
                        var startIndex = i * linesPerFile;
                        var endIndex = Math.Min(startIndex + linesPerFile, lines.Length);
                        var partLines = lines.Skip(startIndex).Take(endIndex - startIndex);
                        
                        File.WriteAllText(outputPath, string.Join(Environment.NewLine, partLines), Encoding.UTF8);
                        outputFiles.Add(outputPath);
                    }
                }
                else
                {
                    // Belirtilen aralıkları kaydet
                    var ranges = ParsePageRanges(pageRanges);
                    int fileIndex = 1;

                    foreach (var range in ranges)
                    {
                        var outputPath = Path.Combine(outputDirectory, $"range_{range.Start}-{range.End}.txt");
                        var startIndex = Math.Max(0, (range.Start - 1) * 50);
                        var endIndex = Math.Min(lines.Length, range.End * 50);
                        var rangeLines = lines.Skip(startIndex).Take(endIndex - startIndex);
                        
                        File.WriteAllText(outputPath, string.Join(Environment.NewLine, rangeLines), Encoding.UTF8);
                        outputFiles.Add(outputPath);
                    }
                }

                return ServiceResult<List<string>>.Success(outputFiles);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<string>>.Error($"PDF bölme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// PDF bilgilerini alır (basit dosya bilgileri)
        /// </summary>
        /// <param name="pdfPath">PDF dosya yolu</param>
        /// <returns>PDF bilgileri</returns>
        public static ServiceResult<PdfInfo> GetPdfInfo(string pdfPath)
        {
            try
            {
                if (!File.Exists(pdfPath))
                    return ServiceResult<PdfInfo>.Error($"PDF dosyası bulunamadı: {pdfPath}");

                var fileInfo = new System.IO.FileInfo(pdfPath);

                var pdfInfo = new PdfInfo
                {
                    FilePath = pdfPath,
                    PageCount = 1, // Basit metin dosyası olarak kabul ediyoruz
                    FileSize = fileInfo.Length,
                    Title = Path.GetFileNameWithoutExtension(pdfPath),
                    Author = "Unknown",
                    Subject = "Text Document",
                    Creator = "WebLibrary",
                    Producer = "WebLibrary",
                    CreationDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime,
                    IsEncrypted = false
                };

                return ServiceResult<PdfInfo>.Success(pdfInfo);
            }
            catch (Exception ex)
            {
                return ServiceResult<PdfInfo>.Error($"PDF bilgi alma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Word belgesini PDF'e dönüştürür (basit metin tabanlı)
        /// </summary>
        /// <param name="textContent">Word belgesi içeriği (metin olarak)</param>
        /// <param name="outputPath">Çıkış PDF yolu</param>
        /// <param name="title">Belge başlığı</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertTextToPdf(string textContent, string outputPath, string? title = null)
        {
            return CreatePdfFromText(textContent, outputPath, title);
        }

        /// <summary>
        /// Metin dosyasını okur ve PDF'e dönüştürür
        /// </summary>
        /// <param name="textFilePath">Metin dosya yolu</param>
        /// <param name="outputPath">Çıkış PDF yolu</param>
        /// <param name="encoding">Karakter kodlaması</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> ConvertTextFileToPdf(string textFilePath, string outputPath, Encoding? encoding = null)
        {
            try
            {
                if (!File.Exists(textFilePath))
                    return ServiceResult<bool>.Error($"Metin dosyası bulunamadı: {textFilePath}");

                encoding ??= Encoding.UTF8;
                var textContent = File.ReadAllText(textFilePath, encoding);
                var title = Path.GetFileNameWithoutExtension(textFilePath);

                return CreatePdfFromText(textContent, outputPath, title);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Metin dosyası PDF dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Sayfa aralıklarını parse eder
        /// </summary>
        /// <param name="pageRanges">Sayfa aralıkları (örn: "1-5,10-15")</param>
        /// <returns>Sayfa aralığı listesi</returns>
        private static List<PageRange> ParsePageRanges(string pageRanges)
        {
            var ranges = new List<PageRange>();
            var parts = pageRanges.Split(',');

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains('-'))
                {
                    var rangeParts = trimmed.Split('-');
                    if (rangeParts.Length == 2 && 
                        int.TryParse(rangeParts[0].Trim(), out var start) && 
                        int.TryParse(rangeParts[1].Trim(), out var end))
                    {
                        ranges.Add(new PageRange { Start = start, End = end });
                    }
                }
                else if (int.TryParse(trimmed, out var singlePage))
                {
                    ranges.Add(new PageRange { Start = singlePage, End = singlePage });
                }
            }

            return ranges;
        }
    }

    /// <summary>
    /// PDF bilgisi
    /// </summary>
    public class PdfInfo
    {
        /// <summary>
        /// Dosya yolu
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Sayfa sayısı
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Dosya boyutu (byte)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Başlık
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Yazar
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Konu
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturan
        /// </summary>
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// Üretici
        /// </summary>
        public string Producer { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Değiştirilme tarihi
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Şifreli mi?
        /// </summary>
        public bool IsEncrypted { get; set; }
    }

    /// <summary>
    /// Sayfa aralığı
    /// </summary>
    public class PageRange
    {
        /// <summary>
        /// Başlangıç sayfası
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Bitiş sayfası
        /// </summary>
        public int End { get; set; }
    }
}
