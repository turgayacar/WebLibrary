using OfficeOpenXml;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.DataProcessing
{
    /// <summary>
    /// Excel işlemleri için helper sınıfı
    /// </summary>
    public static class ExcelDataHelper
    {
            static ExcelDataHelper()
    {
        // EPPlus 8+ için lisans ayarı gerekmez
        // EPPlus artık ücretsiz ve açık kaynak
    }

        /// <summary>
        /// Excel dosyasından veri okur
        /// </summary>
        /// <typeparam name="T">Okunacak model tipi</typeparam>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="sheetName">Sayfa adı (null ise ilk sayfa)</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <param name="startRow">Başlangıç satırı (1'den başlar)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<T>> ReadFromExcel<T>(
            string filePath,
            string? sheetName = null,
            bool hasHeader = true,
            int startRow = 1) where T : class, new()
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<List<T>>.Error($"Dosya bulunamadı: {filePath}");

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = sheetName != null 
                    ? package.Workbook.Worksheets[sheetName]
                    : package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return ServiceResult<List<T>>.Error($"Sayfa bulunamadı: {sheetName ?? "ilk sayfa"}");

                var records = new List<T>();
                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanWrite).ToList();

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                var colCount = worksheet.Dimension?.Columns ?? 0;

                if (rowCount == 0 || colCount == 0)
                    return ServiceResult<List<T>>.Success(records);

                var actualStartRow = hasHeader ? startRow + 1 : startRow;

                for (int row = actualStartRow; row <= rowCount; row++)
                {
                    var record = new T();
                    var hasData = false;

                    for (int col = 1; col <= colCount; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value;
                        if (cellValue != null)
                        {
                            hasData = true;
                            var propertyIndex = col - 1;
                            if (propertyIndex < properties.Count)
                            {
                                var property = properties[propertyIndex];
                                try
                                {
                                    var convertedValue = Convert.ChangeType(cellValue, property.PropertyType);
                                    property.SetValue(record, convertedValue);
                                }
                                catch
                                {
                                    // Dönüştürme hatası, devam et
                                }
                            }
                        }
                    }

                    if (hasData)
                        records.Add(record);
                }

                return ServiceResult<List<T>>.Success(records);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<T>>.Error($"Excel okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Excel dosyasından dinamik olarak veri okur
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="sheetName">Sayfa adı (null ise ilk sayfa)</param>
        /// <param name="hasHeader">Başlık satırı var mı?</param>
        /// <param name="startRow">Başlangıç satırı (1'den başlar)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<Dictionary<string, object>>> ReadFromExcelDynamic(
            string filePath,
            string? sheetName = null,
            bool hasHeader = true,
            int startRow = 1)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<List<Dictionary<string, object>>>.Error($"Dosya bulunamadı: {filePath}");

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = sheetName != null 
                    ? package.Workbook.Worksheets[sheetName]
                    : package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return ServiceResult<List<Dictionary<string, object>>>.Error($"Sayfa bulunamadı: {sheetName ?? "ilk sayfa"}");

                var records = new List<Dictionary<string, object>>();
                var rowCount = worksheet.Dimension?.Rows ?? 0;
                var colCount = worksheet.Dimension?.Columns ?? 0;

                if (rowCount == 0 || colCount == 0)
                    return ServiceResult<List<Dictionary<string, object>>>.Success(records);

                var actualStartRow = hasHeader ? startRow + 1 : startRow;
                var headers = new List<string>();

                // Başlıkları al
                if (hasHeader)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        var headerValue = worksheet.Cells[startRow, col].Value;
                        headers.Add(headerValue?.ToString() ?? $"Column{col}");
                    }
                }
                else
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        headers.Add($"Column{col}");
                    }
                }

                // Verileri oku
                for (int row = actualStartRow; row <= rowCount; row++)
                {
                    var record = new Dictionary<string, object>();
                    var hasData = false;

                    for (int col = 1; col <= colCount; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value;
                        if (cellValue != null)
                        {
                            hasData = true;
                            var header = headers[col - 1];
                            record[header] = cellValue;
                        }
                    }

                    if (hasData)
                        records.Add(record);
                }

                return ServiceResult<List<Dictionary<string, object>>>.Success(records);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Dictionary<string, object>>>.Error($"Excel dinamik okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Veriyi Excel dosyasına yazar
        /// </summary>
        /// <typeparam name="T">Yazılacak model tipi</typeparam>
        /// <param name="data">Yazılacak veri</param>
        /// <param name="filePath">Dosya yolu</param>
        /// <param name="sheetName">Sayfa adı</param>
        /// <param name="hasHeader">Başlık satırı eklensin mi?</param>
        /// <param name="startRow">Başlangıç satırı (1'den başlar)</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> WriteToExcel<T>(
            IEnumerable<T> data,
            string filePath,
            string sheetName = "Sheet1",
            bool hasHeader = true,
            int startRow = 1) where T : class
        {
            try
            {
                if (data == null)
                    return ServiceResult<bool>.Error("Veri null olamaz");

                if (string.IsNullOrWhiteSpace(filePath))
                    return ServiceResult<bool>.Error("Dosya yolu boş olamaz");

                // Dizin yoksa oluştur
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanRead).ToList();

                var currentRow = startRow;

                // Başlık satırı
                if (hasHeader)
                {
                    for (int col = 1; col <= properties.Count; col++)
                    {
                        var property = properties[col - 1];
                        worksheet.Cells[currentRow, col].Value = property.Name;
                        worksheet.Cells[currentRow, col].Style.Font.Bold = true;
                    }
                    currentRow++;
                }

                // Veri satırları
                foreach (var item in data)
                {
                    for (int col = 1; col <= properties.Count; col++)
                    {
                        var property = properties[col - 1];
                        var value = property.GetValue(item);
                        worksheet.Cells[currentRow, col].Value = value;
                    }
                    currentRow++;
                }

                // Sütun genişliklerini otomatik ayarla
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                package.SaveAs(new FileInfo(filePath));
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Excel yazma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Excel dosyasına birden fazla sayfa ekler
        /// </summary>
        /// <param name="dataSheets">Sayfa verileri</param>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> WriteToExcelMultiSheet(
            Dictionary<string, IEnumerable<Dictionary<string, object>>> dataSheets,
            string filePath)
        {
            try
            {
                if (dataSheets == null || !dataSheets.Any())
                    return ServiceResult<bool>.Error("Sayfa verileri boş olamaz");

                if (string.IsNullOrWhiteSpace(filePath))
                    return ServiceResult<bool>.Error("Dosya yolu boş olamaz");

                // Dizin yoksa oluştur
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var package = new ExcelPackage();

                foreach (var sheetData in dataSheets)
                {
                    var sheetName = sheetData.Key;
                    var data = sheetData.Value;

                    if (data == null || !data.Any())
                        continue;

                    var worksheet = package.Workbook.Worksheets.Add(sheetName);
                    var headers = data.First().Keys.ToList();

                    // Başlık satırı
                    for (int col = 1; col <= headers.Count; col++)
                    {
                        worksheet.Cells[1, col].Value = headers[col - 1];
                        worksheet.Cells[1, col].Style.Font.Bold = true;
                    }

                    // Veri satırları
                    var row = 2;
                    foreach (var record in data)
                    {
                        for (int col = 1; col <= headers.Count; col++)
                        {
                            var header = headers[col - 1];
                            var value = record.ContainsKey(header) ? record[header] : null;
                            worksheet.Cells[row, col].Value = value;
                        }
                        row++;
                    }

                    // Sütun genişliklerini otomatik ayarla
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                package.SaveAs(new FileInfo(filePath));
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Excel çoklu sayfa yazma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Excel dosyasındaki sayfa adlarını alır
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<string>> GetExcelSheetNames(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<List<string>>.Error($"Dosya bulunamadı: {filePath}");

                using var package = new ExcelPackage(new FileInfo(filePath));
                var sheetNames = package.Workbook.Worksheets.Select(ws => ws.Name).ToList();

                return ServiceResult<List<string>>.Success(sheetNames);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<string>>.Error($"Excel sayfa adları okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Excel dosyasındaki sayfa bilgilerini alır
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<ExcelSheetInfo>> GetExcelSheetInfo(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<List<ExcelSheetInfo>>.Error($"Dosya bulunamadı: {filePath}");

                using var package = new ExcelPackage(new FileInfo(filePath));
                var sheetInfos = new List<ExcelSheetInfo>();

                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    var sheetInfo = new ExcelSheetInfo
                    {
                        Name = worksheet.Name,
                        Index = worksheet.Index,
                        RowCount = worksheet.Dimension?.Rows ?? 0,
                        ColumnCount = worksheet.Dimension?.Columns ?? 0,
                        HasData = worksheet.Dimension != null
                    };

                    sheetInfos.Add(sheetInfo);
                }

                return ServiceResult<List<ExcelSheetInfo>>.Success(sheetInfos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ExcelSheetInfo>>.Error($"Excel sayfa bilgileri okuma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Excel dosyasını doğrular
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<ExcelValidationResult> ValidateExcel(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return ServiceResult<ExcelValidationResult>.Error($"Dosya bulunamadı: {filePath}");

                var validationResult = new ExcelValidationResult
                {
                    IsValid = true,
                    Errors = new List<string>()
                };

                using var package = new ExcelPackage(new FileInfo(filePath));

                if (package.Workbook.Worksheets.Count == 0)
                {
                    validationResult.IsValid = false;
                    validationResult.Errors.Add("Excel dosyası hiç sayfa içermiyor");
                }

                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    if (worksheet.Dimension == null)
                    {
                        validationResult.Errors.Add($"Sayfa '{worksheet.Name}' veri içermiyor");
                        continue;
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    if (rowCount == 0 || colCount == 0)
                    {
                        validationResult.Errors.Add($"Sayfa '{worksheet.Name}' boş");
                    }
                }

                if (validationResult.Errors.Any())
                    validationResult.IsValid = false;

                return ServiceResult<ExcelValidationResult>.Success(validationResult);
            }
            catch (Exception ex)
            {
                return ServiceResult<ExcelValidationResult>.Error($"Excel doğrulama hatası: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Excel sayfa bilgisi
    /// </summary>
    public class ExcelSheetInfo
    {
        /// <summary>
        /// Sayfa adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sayfa indeksi
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Satır sayısı
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Sütun sayısı
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Veri içeriyor mu?
        /// </summary>
        public bool HasData { get; set; }
    }

    /// <summary>
    /// Excel doğrulama sonucu
    /// </summary>
    public class ExcelValidationResult
    {
        /// <summary>
        /// Excel geçerli mi?
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Hata mesajları
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }
}
