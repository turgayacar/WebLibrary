using System.Text.Json;
using WebLibrary.Models;

namespace WebLibrary.DataProcessing
{
    /// <summary>
    /// Veri dönüştürme işlemleri için helper sınıfı
    /// </summary>
    public static class DataTransformationHelper
    {
        /// <summary>
        /// Nesneyi JSON formatına dönüştürür
        /// </summary>
        /// <typeparam name="T">Dönüştürülecek tip</typeparam>
        /// <param name="obj">Dönüştürülecek nesne</param>
        /// <param name="indented">Girintili format mı?</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<string> ToJson<T>(T obj, bool indented = false)
        {
            try
            {
                if (obj == null)
                    return ServiceResult<string>.Error("Nesne null olamaz");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = indented,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(obj, options);
                return ServiceResult<string>.Success(json);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Error($"JSON dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// JSON'dan nesneye dönüştürür
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> FromJson<T>(string json) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                    return ServiceResult<T>.Error("JSON string boş olamaz");

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var obj = JsonSerializer.Deserialize<T>(json, options);
                if (obj == null)
                    return ServiceResult<T>.Error("JSON deserialize edilemedi");

                return ServiceResult<T>.Success(obj);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"JSON parse hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi Dictionary'e dönüştürür
        /// </summary>
        /// <typeparam name="T">Kaynak tip</typeparam>
        /// <param name="obj">Dönüştürülecek nesne</param>
        /// <param name="includeNulls">Null değerler dahil edilsin mi?</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<Dictionary<string, object?>> ToDictionary<T>(T obj, bool includeNulls = false)
        {
            try
            {
                if (obj == null)
                    return ServiceResult<Dictionary<string, object?>>.Error("Nesne null olamaz");

                var type = typeof(T);
                var properties = type.GetProperties();
                var result = new Dictionary<string, object?>();

                foreach (var property in properties)
                {
                    var value = property.GetValue(obj);
                    if (includeNulls || value != null)
                    {
                        result[property.Name] = value;
                    }
                }

                return ServiceResult<Dictionary<string, object?>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<Dictionary<string, object?>>.Error($"Dictionary dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dictionary'den nesne oluşturur
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="dictionary">Kaynak dictionary</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> FromDictionary<T>(Dictionary<string, object?> dictionary) where T : class, new()
        {
            try
            {
                if (dictionary == null)
                    return ServiceResult<T>.Error("Dictionary null olamaz");

                var result = new T();
                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanWrite).ToList();

                foreach (var kvp in dictionary)
                {
                    var property = properties.FirstOrDefault(p => 
                        string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                    if (property != null && kvp.Value != null)
                    {
                        try
                        {
                            var convertedValue = Convert.ChangeType(kvp.Value, property.PropertyType);
                            property.SetValue(result, convertedValue);
                        }
                        catch
                        {
                            // Dönüştürme hatası, devam et
                        }
                    }
                }

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"Dictionary'den nesne oluşturma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi farklı tipe dönüştürür
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> TransformTo<T>(object source) where T : class, new()
        {
            try
            {
                if (source == null)
                    return ServiceResult<T>.Error("Kaynak nesne null olamaz");

                // Önce JSON üzerinden dönüştürmeyi dene
                var jsonResult = ToJson(source);
                if (jsonResult.IsSuccess)
                {
                    return FromJson<T>(jsonResult.Data!);
                }

                // Dictionary üzerinden dönüştürmeyi dene
                var dictResult = ToDictionary(source);
                if (dictResult.IsSuccess)
                {
                    return FromDictionary<T>(dictResult.Data!);
                }

                return ServiceResult<T>.Error("Dönüştürme başarısız");
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"Tip dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Koleksiyonu farklı tipe dönüştürür
        /// </summary>
        /// <typeparam name="TSource">Kaynak tip</typeparam>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        /// <param name="sourceCollection">Kaynak koleksiyon</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<List<TTarget>> TransformCollection<TSource, TTarget>(
            IEnumerable<TSource> sourceCollection) 
            where TTarget : class, new()
        {
            try
            {
                if (sourceCollection == null)
                    return ServiceResult<List<TTarget>>.Error("Kaynak koleksiyon null olamaz");

                var result = new List<TTarget>();

                foreach (var source in sourceCollection)
                {
                    var transformed = TransformTo<TTarget>(source);
                    if (transformed.IsSuccess)
                    {
                        result.Add(transformed.Data!);
                    }
                }

                return ServiceResult<List<TTarget>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<TTarget>>.Error($"Koleksiyon dönüştürme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi kopyalar
        /// </summary>
        /// <typeparam name="T">Kopyalanacak tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> DeepCopy<T>(T source) where T : class
        {
            try
            {
                if (source == null)
                    return ServiceResult<T>.Error("Kaynak nesne null olamaz");

                var jsonResult = ToJson(source);
                if (!jsonResult.IsSuccess)
                    return ServiceResult<T>.Error(jsonResult.ErrorMessages.First());

                return FromJson<T>(jsonResult.Data!);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"Derin kopyalama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi belirli property'lerle kopyalar
        /// </summary>
        /// <typeparam name="T">Kopyalanacak tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <param name="propertyNames">Kopyalanacak property adları</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> CopyProperties<T>(T source, params string[] propertyNames) where T : class, new()
        {
            try
            {
                if (source == null)
                    return ServiceResult<T>.Error("Kaynak nesne null olamaz");

                if (propertyNames == null || propertyNames.Length == 0)
                    return ServiceResult<T>.Error("Property adları belirtilmedi");

                var result = new T();
                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanRead && p.CanWrite).ToList();

                foreach (var propertyName in propertyNames)
                {
                    var sourceProperty = properties.FirstOrDefault(p => 
                        string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
                    var targetProperty = properties.FirstOrDefault(p => 
                        string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (sourceProperty != null && targetProperty != null)
                    {
                        var value = sourceProperty.GetValue(source);
                        targetProperty.SetValue(result, value);
                    }
                }

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"Property kopyalama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi belirli property'leri hariç kopyalar
        /// </summary>
        /// <typeparam name="T">Kopyalanacak tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <param name="excludePropertyNames">Hariç tutulacak property adları</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<T> CopyPropertiesExcluding<T>(T source, params string[] excludePropertyNames) where T : class, new()
        {
            try
            {
                if (source == null)
                    return ServiceResult<T>.Error("Kaynak nesne null olamaz");

                var result = new T();
                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanRead && p.CanWrite).ToList();

                foreach (var property in properties)
                {
                    if (!excludePropertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        var value = property.GetValue(source);
                        property.SetValue(result, value);
                    }
                }

                return ServiceResult<T>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<T>.Error($"Property kopyalama hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi günceller
        /// </summary>
        /// <typeparam name="T">Güncellenecek tip</typeparam>
        /// <param name="target">Hedef nesne</param>
        /// <param name="updates">Güncelleme verileri</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> UpdateObject<T>(T target, Dictionary<string, object?> updates) where T : class
        {
            try
            {
                if (target == null)
                    return ServiceResult<bool>.Error("Hedef nesne null olamaz");

                if (updates == null || !updates.Any())
                    return ServiceResult<bool>.Error("Güncelleme verileri boş olamaz");

                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanWrite).ToList();

                foreach (var update in updates)
                {
                    var property = properties.FirstOrDefault(p => 
                        string.Equals(p.Name, update.Key, StringComparison.OrdinalIgnoreCase));

                    if (property != null && update.Value != null)
                    {
                        try
                        {
                            var convertedValue = Convert.ChangeType(update.Value, property.PropertyType);
                            property.SetValue(target, convertedValue);
                        }
                        catch
                        {
                            // Dönüştürme hatası, devam et
                        }
                    }
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Nesne güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi belirli property'lerle günceller
        /// </summary>
        /// <typeparam name="T">Güncellenecek tip</typeparam>
        /// <param name="target">Hedef nesne</param>
        /// <param name="updates">Güncelleme verileri</param>
        /// <param name="allowedProperties">İzin verilen property adları</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> UpdateObjectWithAllowedProperties<T>(
            T target, 
            Dictionary<string, object?> updates, 
            params string[] allowedProperties) where T : class
        {
            try
            {
                if (target == null)
                    return ServiceResult<bool>.Error("Hedef nesne null olamaz");

                if (updates == null || !updates.Any())
                    return ServiceResult<bool>.Error("Güncelleme verileri boş olamaz");

                if (allowedProperties == null || allowedProperties.Length == 0)
                    return ServiceResult<bool>.Error("İzin verilen property'ler belirtilmedi");

                var filteredUpdates = updates.Where(u => 
                    allowedProperties.Contains(u.Key, StringComparer.OrdinalIgnoreCase))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return UpdateObject(target, filteredUpdates);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Nesne güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Nesneyi belirli property'lerle karşılaştırır
        /// </summary>
        /// <typeparam name="T">Karşılaştırılacak tip</typeparam>
        /// <param name="obj1">İlk nesne</param>
        /// <param name="obj2">İkinci nesne</param>
        /// <param name="propertyNames">Karşılaştırılacak property adları</param>
        /// <returns>ServiceResult</returns>
        public static ServiceResult<bool> CompareObjects<T>(
            T obj1, 
            T obj2, 
            params string[] propertyNames) where T : class
        {
            try
            {
                if (obj1 == null || obj2 == null)
                    return ServiceResult<bool>.Error("Nesneler null olamaz");

                if (propertyNames == null || propertyNames.Length == 0)
                    return ServiceResult<bool>.Error("Property adları belirtilmedi");

                var type = typeof(T);
                var properties = type.GetProperties().Where(p => p.CanRead).ToList();

                foreach (var propertyName in propertyNames)
                {
                    var property = properties.FirstOrDefault(p => 
                        string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (property != null)
                    {
                        var value1 = property.GetValue(obj1);
                        var value2 = property.GetValue(obj2);

                        if (!Equals(value1, value2))
                        {
                            return ServiceResult<bool>.Success(false);
                        }
                    }
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Error($"Nesne karşılaştırma hatası: {ex.Message}");
            }
        }
    }
}
