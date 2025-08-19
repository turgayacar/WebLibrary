using System.Collections.Generic;
using System.Linq;
using WebLibrary.Models;

namespace WebLibrary.Extensions
{
    /// <summary>
    /// Generic extension metodları
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        /// Koleksiyonun null veya boş olup olmadığını kontrol eder
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <returns>Null veya boş mu?</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }
        
        /// <summary>
        /// Koleksiyonun null veya boş olmadığını kontrol eder
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <returns>Null veya boş değil mi?</returns>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? collection)
        {
            return !collection.IsNullOrEmpty();
        }
        
        /// <summary>
        /// Koleksiyonu sayfalara böler
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Bölünecek koleksiyon</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>Sayfalanmış koleksiyon</returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> collection, int pageSize)
        {
            return collection.Select((item, index) => new { item, index })
                           .GroupBy(x => x.index / pageSize)
                           .Select(g => g.Select(x => x.item));
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen sayfa ve boyuta göre sayfalar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Sayfalanacak koleksiyon</param>
        /// <param name="pageNumber">Sayfa numarası (1'den başlar)</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>Belirtilen sayfa</returns>
        public static IEnumerable<T> GetPage<T>(this IEnumerable<T> collection, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            
            return collection.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
        
        /// <summary>
        /// Koleksiyonun toplam sayfa sayısını hesaplar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Koleksiyon</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>Toplam sayfa sayısı</returns>
        public static int GetTotalPages<T>(this IEnumerable<T> collection, int pageSize)
        {
            if (pageSize < 1) pageSize = 10;
            var count = collection.Count();
            return (int)Math.Ceiling((double)count / pageSize);
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre sıralar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Sıralanacak koleksiyon</param>
        /// <param name="propertyName">Sıralama property'si</param>
        /// <param name="ascending">Artan sıralama mı?</param>
        /// <returns>Sıralanmış koleksiyon</returns>
        public static IEnumerable<T> OrderByProperty<T>(this IEnumerable<T> collection, string propertyName, bool ascending = true)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection;
            
            if (ascending)
                return collection.OrderBy(x => property.GetValue(x));
            else
                return collection.OrderByDescending(x => property.GetValue(x));
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre filtreler
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Filtrelenecek koleksiyon</param>
        /// <param name="propertyName">Filtreleme property'si</param>
        /// <param name="value">Filtreleme değeri</param>
        /// <returns>Filtrelenmiş koleksiyon</returns>
        public static IEnumerable<T> WherePropertyEquals<T>(this IEnumerable<T> collection, string propertyName, object? value)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection;
            
            return collection.Where(x => Equals(property.GetValue(x), value));
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre filtreler (contains)
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Filtrelenecek koleksiyon</param>
        /// <param name="propertyName">Filtreleme property'si</param>
        /// <param name="value">Filtreleme değeri</param>
        /// <returns>Filtrelenmiş koleksiyon</returns>
        public static IEnumerable<T> WherePropertyContains<T>(this IEnumerable<T> collection, string propertyName, string value)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection;
            
            return collection.Where(x =>
            {
                var propertyValue = property.GetValue(x)?.ToString();
                return !string.IsNullOrEmpty(propertyValue) && propertyValue.Contains(value, StringComparison.OrdinalIgnoreCase);
            });
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre gruplar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Gruplanacak koleksiyon</param>
        /// <param name="propertyName">Gruplama property'si</param>
        /// <returns>Gruplanmış koleksiyon</returns>
        public static IEnumerable<IGrouping<object?, T>> GroupByProperty<T>(this IEnumerable<T> collection, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection.GroupBy(x => (object?)null);
            
            return collection.GroupBy(x => property.GetValue(x));
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre distinct yapar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Distinct yapılacak koleksiyon</param>
        /// <param name="propertyName">Distinct property'si</param>
        /// <returns>Distinct koleksiyon</returns>
        public static IEnumerable<T> DistinctByProperty<T>(this IEnumerable<T> collection, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection;
            
            return collection.GroupBy(x => property.GetValue(x)).Select(g => g.First());
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre sum yapar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Koleksiyon</param>
        /// <param name="propertyName">Sum property'si</param>
        /// <returns>Toplam değer</returns>
        public static decimal SumByProperty<T>(this IEnumerable<T> collection, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return 0;
            
            return collection.Sum(x =>
            {
                var value = property.GetValue(x);
                return Convert.ToDecimal(value ?? 0);
            });
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre average yapar
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Koleksiyon</param>
        /// <param name="propertyName">Average property'si</param>
        /// <returns>Ortalama değer</returns>
        public static decimal AverageByProperty<T>(this IEnumerable<T> collection, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return 0;
            
            var values = collection.Select(x =>
            {
                var value = property.GetValue(x);
                return Convert.ToDecimal(value ?? 0);
            }).ToList();
            
            return values.Any() ? values.Average() : 0;
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre min değerini bulur
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Koleksiyon</param>
        /// <param name="propertyName">Min property'si</param>
        /// <returns>Minimum değer</returns>
        public static T? MinByProperty<T>(this IEnumerable<T> collection, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection.FirstOrDefault();
            
            return collection.OrderBy(x => property.GetValue(x)).FirstOrDefault();
        }
        
        /// <summary>
        /// Koleksiyonu belirtilen property'ye göre max değerini bulur
        /// </summary>
        /// <typeparam name="T">Koleksiyon tipi</typeparam>
        /// <param name="collection">Koleksiyon</param>
        /// <param name="propertyName">Max property'si</param>
        /// <returns>Maksimum değer</returns>
        public static T? MaxByProperty<T>(this IEnumerable<T> collection, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return collection.FirstOrDefault();
            
            return collection.OrderByDescending(x => property.GetValue(x)).FirstOrDefault();
        }
    }
}
