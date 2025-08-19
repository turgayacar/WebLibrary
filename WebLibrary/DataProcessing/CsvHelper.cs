using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.DataProcessing
{
    /// <summary>
    /// CSV işlemleri için helper sınıfı
    /// </summary>
    public static class CsvDataHelper
    {
        /// <summary>
        /// CSV'den veri okur
        /// </summary>
        /// <typeparam name="T">Okunacak model tipi</typeparam>
        /// <param name="csvContent">CSV içeriği</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<T>> ReadFromCsv<T>(
            string csvContent,
            bool hasHeader = true,
            string delimiter = ",") where T : class, new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                    return ServiceResult<List<T>>.Error("CSV içeriği boş olamaz");

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = hasHeader,
                    Delimiter = delimiter,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, config);

                var records = csv.GetRecords<T>().ToList();
                return ServiceResult<List<T>>.Success(records);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<T>>.Error($"CSV okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CSV dosyasından veri okur
        /// </summary>
        /// <typeparam name="T">Okunacak model tipi</typeparam>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="encoding">Dosya encoding'i</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<T>> ReadFromCsvFile<T>(
            string filePath,
            Encoding? encoding = null,
            bool hasHeader = true,
            string delimiter = ",") where T : class, new()
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<List<T>>.Error($"Dosya bulunamadı: {filePath}");

                encoding ??= Encoding.UTF8;
                var csvContent = File.ReadAllText(filePath, encoding);

                return ReadFromCsv<T>(csvContent, hasHeader, delimiter);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<T>>.Error($"CSV dosya okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Veriyi CSV formatına dönüştürür
        /// </summary>
        /// <typeparam name="T">Dönüştürülecek model tipi</typeparam>
        /// <param name="data">Dönüştürülecek veri</param>
        /// <param name="hasHeader">Başlık satırı eklensin mi?</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> WriteToCsv<T>(
            IEnumerable<T> data,
            bool hasHeader = true,
            string delimiter = ",") where T : class
        {
            try
            {
                if (data == null)
                    return ServiceResult<string>.Error("Veri null olamaz");

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = hasHeader,
                    Delimiter = delimiter
                };

                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, config);

                csv.WriteRecords(data);
                var csvContent = writer.ToString();

                return ServiceResult<string>.Success(csvContent);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"CSV yazma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Veriyi CSV dosyasına yazar
        /// </summary>
        /// <typeparam name="T">Yazılacak model tipi</typeparam>
        /// <param name="data">Yazılacak veri</param>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="encoding">Dosya encoding'i</param>
        /// <param name="hasHeader">Başlık satırı eklensin mi?</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> WriteToCsvFile<T>(
            IEnumerable<T> data,
            string filePath,
            Encoding? encoding = null,
            bool hasHeader = true,
            string delimiter = ",") where T : class
        {
            try
            {
                if (data == null)
                    return ServiceResult<bool>.Error("Veri null olamaz");

                if (string.IsNullOrWhiteSpace(filePath))
                    return ServiceResult<bool>.Error("Dosya yolu boş olamaz");

                encoding ??= Encoding.UTF8;
                var csvContent = WriteToCsv(data, hasHeader, delimiter);

                if (!csvContent.IsSuccess)
                    return ServiceResult<bool>.Error(csvContent.ErrorMessages.First());

                // Dizin yoksa oluştur
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, csvContent.Data, encoding);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"CSV dosya yazma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CSV'den belirli sütunları okur
        /// </summary>
        /// <typeparam name="T">Okunacak model tipi</typeparam>
        /// <param name="csvContent">CSV içeriği</param>
        /// <param name="columnMappings">Sütun eşleştirmeleri</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<T>> ReadFromCsvWithMapping<T>(
            string csvContent,
            Dictionary<string, string> columnMappings,
            bool hasHeader = true,
            string delimiter = ",") where T : class, new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                    return ServiceResult<List<T>>.Error("CSV içeriği boş olamaz");

                if (columnMappings == null || !columnMappings.Any())
                    return ServiceResult<List<T>>.Error("Sütun eşleştirmeleri boş olamaz");

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = hasHeader,
                    Delimiter = delimiter,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, config);

                var records = new List<T>();

                if (hasHeader)
                {
                    csv.Read();
                    csv.ReadHeader();
                }

                while (csv.Read())
                {
                    var record = new T();
                    var type = typeof(T);

                    foreach (var mapping in columnMappings)
                    {
                        var csvColumnName = mapping.Key;
                        var propertyName = mapping.Value;

                        var property = type.GetProperty(propertyName);
                        if (property != null && property.CanWrite)
                        {
                            try
                            {
                                var value = csv.GetField(csvColumnName);
                                if (value != null)
                                {
                                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                                    property.SetValue(record, convertedValue);
                                }
                            }
                            catch
                            {
                                // Sütun bulunamadı veya dönüştürülemedi, devam et
                            }
                        }
                    }

                    records.Add(record);
                }

                return ServiceResult<List<T>>.Success(records);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<T>>.Error($"CSV mapping okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CSV'den dinamik olarak veri okur
        /// </summary>
        /// <param name="csvContent">CSV içeriği</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<Dictionary<string, string>>> ReadFromCsvDynamic(
            string csvContent,
            bool hasHeader = true,
            string delimiter = ",")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                    return ServiceResult<List<Dictionary<string, string>>>.Error("CSV içeriği boş olamaz");

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = hasHeader,
                    Delimiter = delimiter,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var reader = new StringReader(csvContent);
                using var csv = new CsvReader(reader, config);

                var records = new List<Dictionary<string, string>>();

                if (hasHeader)
                {
                    csv.Read();
                    csv.ReadHeader();
                }

                while (csv.Read())
                {
                    var record = new Dictionary<string, string>();
                    var headers = hasHeader ? csv.HeaderRecord : null;

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            var value = csv.GetField(header);
                            record[header] = value ?? string.Empty;
                        }
                    }
                    else
                    {
                        // Başlık yoksa sütun indekslerini kullan
                        var fieldCount = csv.Parser.Count;
                        for (int i = 0; i < fieldCount; i++)
                        {
                            var value = csv.GetField(i);
                            record[$"Column{i}"] = value ?? string.Empty;
                        }
                    }

                    records.Add(record);
                }

                return ServiceResult<List<Dictionary<string, string>>>.Success(records);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Dictionary<string, string>>>.Error($"CSV dinamik okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CSV'deki sütun başlıklarını alır
        /// </summary>
        /// <param name="csvContent">CSV içeriği</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<string>> GetCsvHeaders(
            string csvContent,
            string delimiter = ",")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                    return ServiceResult<List<string>>.Error("CSV içeriği boş olamaz");

                var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return ServiceResult<List<string>>.Error("CSV içeriği satır içermiyor");

                var firstLine = lines[0].Trim();
                var headers = firstLine.Split(delimiter)
                    .Select(h => h.Trim().Trim('"'))
                    .Where(h => !string.IsNullOrEmpty(h))
                    .ToList();

                return ServiceResult<List<string>>.Success(headers);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<string>>.Error($"CSV başlık okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CSV'deki satır sayısını sayar
        /// </summary>
        /// <param name="csvContent">CSV içeriği</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<int> GetCsvRowCount(
            string csvContent,
            bool hasHeader = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                    return ServiceResult<int>.Error("CSV içeriği boş olamaz");

                var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var rowCount = lines.Length;

                if (hasHeader && rowCount > 0)
                    rowCount--;

                return ServiceResult<int>.Success(rowCount);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Error($"CSV satır sayma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// CSV'yi doğrular
        /// </summary>
        /// <param name="csvContent">CSV içeriği</param>
        /// <param name="expectedColumns">Beklenen sütun sayısı</param>
        /// <param name="delimiter">Ayırıcı karakter</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<CsvValidationResult> ValidateCsv(
            string csvContent,
            int? expectedColumns = null,
            string delimiter = ",")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                    return ServiceResult<CsvValidationResult>.Error("CSV içeriği boş olamaz");

                var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return ServiceResult<CsvValidationResult>.Error("CSV içeriği satır içermiyor");

                var validationResult = new CsvValidationResult
                {
                    IsValid = true,
                    RowCount = lines.Length,
                    ColumnCount = 0,
                    Errors = new List<string>()
                };

                // İlk satırdan sütun sayısını al
                var firstLine = lines[0].Trim();
                var columns = firstLine.Split(delimiter);
                validationResult.ColumnCount = columns.Length;

                // Beklenen sütun sayısı kontrolü
                if (expectedColumns.HasValue && validationResult.ColumnCount != expectedColumns.Value)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add($"Beklenen sütun sayısı: {expectedColumns.Value}, Gerçek: {validationResult.ColumnCount}");
                }

                // Tüm satırlarda sütun sayısı tutarlılığı kontrolü
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var lineColumns = line.Split(delimiter);
                        if (lineColumns.Length != validationResult.ColumnCount)
                        {
                            validationResult.IsValid = false;
                            validationResult.Errors.Add($"Satır {i + 1}: Sütun sayısı tutarsız. Beklenen: {validationResult.ColumnCount}, Gerçek: {lineColumns.Length}");
                        }
                    }
                }

                return ServiceResult<CsvValidationResult>.Success(validationResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<CsvValidationResult>.Error($"CSV doğrulama hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// CSV doğrulama sonucu
    /// </summary>
    public class CsvValidationResult
    {
        /// <summary>
        /// CSV geçerli mi?
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Satır sayısı
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Sütun sayısı
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Hata mesajları
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }
}
