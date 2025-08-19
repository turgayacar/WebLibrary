using System.Reflection;
using System.Text.Json;
using WebLibrary.Models;

namespace WebLibrary.Utilities
{
    /// <summary>
    /// Generic utility metodları
    /// </summary>
    public static class GenericUtilities
    {
        /// <summary>
        /// Nesneyi başka bir tipe dönüştürür
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <returns>Dönüştürülmüş nesne</returns>
        public static T? ConvertTo<T>(object? source) where T : class
        {
            if (source == null)
                return null;
            
            try
            {
                if (source is T target)
                    return target;
                
                var json = JsonSerializer.Serialize(source);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Nesneyi başka bir tipe dönüştürür (value type için)
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <returns>Dönüştürülmüş değer</returns>
        public static T ConvertToValue<T>(object? source) where T : struct
        {
            if (source == null)
                return default;
            
            try
            {
                if (source is T target)
                    return target;
                
                return (T)Convert.ChangeType(source, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        
        /// <summary>
        /// Nesnenin belirtilen property değerini alır
        /// </summary>
        /// <typeparam name="T">Property tipi</typeparam>
        /// <param name="obj">Nesne</param>
        /// <param name="propertyName">Property adı</param>
        /// <returns>Property değeri</returns>
        public static T? GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return default;
            
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property == null)
                    return default;
                
                var value = property.GetValue(obj);
                if (value == null)
                    return default;
                
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        
        /// <summary>
        /// Nesnenin belirtilen property değerini ayarlar
        /// </summary>
        /// <param name="obj">Nesne</param>
        /// <param name="propertyName">Property adı</param>
        /// <param name="value">Yeni değer</param>
        /// <returns>Başarılı mı?</returns>
        public static bool SetPropertyValue(object obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return false;
            
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property == null || !property.CanWrite)
                    return false;
                
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(obj, convertedValue);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Nesnenin tüm property'lerini dictionary olarak döndürür
        /// </summary>
        /// <param name="obj">Nesne</param>
        /// <returns>Property dictionary'si</returns>
        public static Dictionary<string, object?> GetPropertiesAsDictionary(object obj)
        {
            if (obj == null)
                return new Dictionary<string, object?>();
            
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new Dictionary<string, object?>();
            
            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    result[property.Name] = property.GetValue(obj);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Dictionary'den nesne oluşturur
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="properties">Property dictionary'si</param>
        /// <returns>Oluşturulan nesne</returns>
        public static T? CreateFromDictionary<T>(Dictionary<string, object?> properties) where T : class, new()
        {
            if (properties == null)
                return null;
            
            try
            {
                var obj = new T();
                
                foreach (var property in properties)
                {
                    SetPropertyValue(obj, property.Key, property.Value);
                }
                
                return obj;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Nesnenin belirtilen property'lerini kopyalar
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <param name="propertyNames">Kopyalanacak property'ler</param>
        /// <returns>Kopyalanmış nesne</returns>
        public static T? CopyProperties<T>(object source, params string[] propertyNames) where T : class, new()
        {
            if (source == null || propertyNames == null || propertyNames.Length == 0)
                return null;
            
            try
            {
                var target = new T();
                
                foreach (var propertyName in propertyNames)
                {
                    var value = GetPropertyValue<object>(source, propertyName);
                    SetPropertyValue(target, propertyName, value);
                }
                
                return target;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Nesnenin tüm property'lerini kopyalar
        /// </summary>
        /// <typeparam name="T">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <returns>Kopyalanmış nesne</returns>
        public static T? CopyAllProperties<T>(object source) where T : class, new()
        {
            if (source == null)
                return null;
            
            try
            {
                var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var propertyNames = properties.Where(p => p.CanRead).Select(p => p.Name).ToArray();
                
                return CopyProperties<T>(source, propertyNames);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Nesnenin belirtilen property'lerini null yapar
        /// </summary>
        /// <param name="obj">Nesne</param>
        /// <param name="propertyNames">Null yapılacak property'ler</param>
        public static void SetPropertiesToNull(object obj, params string[] propertyNames)
        {
            if (obj == null || propertyNames == null)
                return;
            
            foreach (var propertyName in propertyNames)
            {
                SetPropertyValue(obj, propertyName, null);
            }
        }
        
        /// <summary>
        /// Nesnenin belirtilen property'lerini varsayılan değerlerine ayarlar
        /// </summary>
        /// <param name="obj">Nesne</param>
        /// <param name="propertyNames">Varsayılan değere ayarlanacak property'ler</param>
        public static void SetPropertiesToDefault(object obj, params string[] propertyNames)
        {
            if (obj == null || propertyNames == null)
                return;
            
            foreach (var propertyName in propertyNames)
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    var defaultValue = property.PropertyType.IsValueType ? 
                        Activator.CreateInstance(property.PropertyType) : null;
                    property.SetValue(obj, defaultValue);
                }
            }
        }
        
        /// <summary>
        /// Nesnenin belirtilen property'lerinin değerlerini karşılaştırır
        /// </summary>
        /// <param name="obj1">İlk nesne</param>
        /// <param name="obj2">İkinci nesne</param>
        /// <param name="propertyNames">Karşılaştırılacak property'ler</param>
        /// <returns>Eşit mi?</returns>
        public static bool CompareProperties(object obj1, object obj2, params string[] propertyNames)
        {
            if (obj1 == null || obj2 == null || propertyNames == null)
                return false;
            
            foreach (var propertyName in propertyNames)
            {
                var value1 = GetPropertyValue<object>(obj1, propertyName);
                var value2 = GetPropertyValue<object>(obj2, propertyName);
                
                if (!Equals(value1, value2))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Nesnenin belirtilen property'lerinin değişip değişmediğini kontrol eder
        /// </summary>
        /// <param name="original">Orijinal nesne</param>
        /// <param name="current">Mevcut nesne</param>
        /// <param name="propertyNames">Kontrol edilecek property'ler</param>
        /// <returns>Değişen property'ler</returns>
        public static List<string> GetChangedProperties(object original, object current, params string[] propertyNames)
        {
            var changedProperties = new List<string>();
            
            if (original == null || current == null || propertyNames == null)
                return changedProperties;
            
            foreach (var propertyName in propertyNames)
            {
                var originalValue = GetPropertyValue<object>(original, propertyName);
                var currentValue = GetPropertyValue<object>(current, propertyName);
                
                if (!Equals(originalValue, currentValue))
                {
                    changedProperties.Add(propertyName);
                }
            }
            
            return changedProperties;
        }
    }
}
