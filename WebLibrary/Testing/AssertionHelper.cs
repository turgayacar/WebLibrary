using System.Collections;
using System.Reflection;
using WebLibrary.Models;

namespace WebLibrary.Testing
{
    /// <summary>
    /// Gelişmiş assertion yardımcısı - Test'lerde kullanılmak üzere çeşitli assertion metodları sağlar
    /// </summary>
    public static class AssertionHelper
    {
        /// <summary>
        /// İki nesnenin eşit olup olmadığını kontrol eder (deep comparison)
        /// </summary>
        /// <param name="expected">Beklenen değer</param>
        /// <param name="actual">Gerçek değer</param>
        /// <param name="message">Hata mesajı</param>
        public static void AreEqual(object? expected, object? actual, string? message = null)
        {
            if (!AreObjectsEqual(expected, actual))
            {
                var errorMessage = message ?? $"Expected: {expected}, Actual: {actual}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// İki nesnenin eşit olmadığını kontrol eder
        /// </summary>
        /// <param name="expected">Beklenen değer</param>
        /// <param name="actual">Gerçek değer</param>
        /// <param name="message">Hata mesajı</param>
        public static void AreNotEqual(object? expected, object? actual, string? message = null)
        {
            if (AreObjectsEqual(expected, actual))
            {
                var errorMessage = message ?? $"Expected not equal, but both values are: {expected}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Nesnenin null olup olmadığını kontrol eder
        /// </summary>
        /// <param name="obj">Kontrol edilecek nesne</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsNull(object? obj, string? message = null)
        {
            if (obj != null)
            {
                var errorMessage = message ?? $"Expected null, but got: {obj}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Nesnenin null olmadığını kontrol eder
        /// </summary>
        /// <param name="obj">Kontrol edilecek nesne</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsNotNull(object? obj, string? message = null)
        {
            if (obj == null)
            {
                var errorMessage = message ?? "Expected not null, but got null";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koşulun true olduğunu kontrol eder
        /// </summary>
        /// <param name="condition">Kontrol edilecek koşul</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsTrue(bool condition, string? message = null)
        {
            if (!condition)
            {
                var errorMessage = message ?? "Expected true, but got false";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koşulun false olduğunu kontrol eder
        /// </summary>
        /// <param name="condition">Kontrol edilecek koşul</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsFalse(bool condition, string? message = null)
        {
            if (condition)
            {
                var errorMessage = message ?? "Expected false, but got true";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koleksiyonun belirli sayıda eleman içerdiğini kontrol eder
        /// </summary>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <param name="expectedCount">Beklenen eleman sayısı</param>
        /// <param name="message">Hata mesajı</param>
        public static void HasCount(IEnumerable collection, int expectedCount, string? message = null)
        {
            var actualCount = collection.Cast<object>().Count();
            if (actualCount != expectedCount)
            {
                var errorMessage = message ?? $"Expected count: {expectedCount}, Actual count: {actualCount}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koleksiyonun boş olduğunu kontrol eder
        /// </summary>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsEmpty(IEnumerable collection, string? message = null)
        {
            if (collection.Cast<object>().Any())
            {
                var errorMessage = message ?? "Expected empty collection, but it contains elements";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koleksiyonun boş olmadığını kontrol eder
        /// </summary>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsNotEmpty(IEnumerable collection, string? message = null)
        {
            if (!collection.Cast<object>().Any())
            {
                var errorMessage = message ?? "Expected non-empty collection, but it is empty";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koleksiyonun belirli bir elemanı içerdiğini kontrol eder
        /// </summary>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <param name="item">Aranan eleman</param>
        /// <param name="message">Hata mesajı</param>
        public static void Contains(IEnumerable collection, object item, string? message = null)
        {
            if (!collection.Cast<object>().Contains(item))
            {
                var errorMessage = message ?? $"Collection does not contain expected item: {item}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Koleksiyonun belirli bir elemanı içermediğini kontrol eder
        /// </summary>
        /// <param name="collection">Kontrol edilecek koleksiyon</param>
        /// <param name="item">Aranan eleman</param>
        /// <param name="message">Hata mesajı</param>
        public static void DoesNotContain(IEnumerable collection, object item, string? message = null)
        {
            if (collection.Cast<object>().Contains(item))
            {
                var errorMessage = message ?? $"Collection contains unexpected item: {item}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// String'in belirli bir alt string'i içerdiğini kontrol eder
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <param name="substring">Aranan alt string</param>
        /// <param name="message">Hata mesajı</param>
        public static void StringContains(string text, string substring, string? message = null)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains(substring))
            {
                var errorMessage = message ?? $"String '{text}' does not contain substring '{substring}'";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// String'in belirli bir alt string'i içermediğini kontrol eder
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <param name="substring">Aranan alt string</param>
        /// <param name="message">Hata mesajı</param>
        public static void StringDoesNotContain(string text, string substring, string? message = null)
        {
            if (!string.IsNullOrEmpty(text) && text.Contains(substring))
            {
                var errorMessage = message ?? $"String '{text}' contains unexpected substring '{substring}'";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// String'in belirli bir pattern ile eşleştiğini kontrol eder (regex)
        /// </summary>
        /// <param name="text">Kontrol edilecek string</param>
        /// <param name="pattern">Regex pattern</param>
        /// <param name="message">Hata mesajı</param>
        public static void StringMatches(string text, string pattern, string? message = null)
        {
            if (string.IsNullOrEmpty(text) || !System.Text.RegularExpressions.Regex.IsMatch(text, pattern))
            {
                var errorMessage = message ?? $"String '{text}' does not match pattern '{pattern}'";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Exception'ın fırlatıldığını kontrol eder
        /// </summary>
        /// <typeparam name="T">Beklenen exception türü</typeparam>
        /// <param name="action">Test edilecek aksiyon</param>
        /// <param name="message">Hata mesajı</param>
        public static void ThrowsException<T>(Action action, string? message = null) where T : Exception
        {
            try
            {
                action();
                var errorMessage = message ?? $"Expected exception of type {typeof(T).Name}, but no exception was thrown";
                throw new AssertionException(errorMessage);
            }
            catch (T)
            {
                // Beklenen exception fırlatıldı
            }
            catch (Exception ex)
            {
                var errorMessage = message ?? $"Expected exception of type {typeof(T).Name}, but got {ex.GetType().Name}: {ex.Message}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Async metodun exception fırlattığını kontrol eder
        /// </summary>
        /// <typeparam name="T">Beklenen exception türü</typeparam>
        /// <param name="asyncAction">Test edilecek async aksiyon</param>
        /// <param name="message">Hata mesajı</param>
        public static async Task ThrowsExceptionAsync<T>(Func<Task> asyncAction, string? message = null) where T : Exception
        {
            try
            {
                await asyncAction();
                var errorMessage = message ?? $"Expected exception of type {typeof(T).Name}, but no exception was thrown";
                throw new AssertionException(errorMessage);
            }
            catch (T)
            {
                // Beklenen exception fırlatıldı
            }
            catch (Exception ex)
            {
                var errorMessage = message ?? $"Expected exception of type {typeof(T).Name}, but got {ex.GetType().Name}: {ex.Message}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Exception fırlatılmadığını kontrol eder
        /// </summary>
        /// <param name="action">Test edilecek aksiyon</param>
        /// <param name="message">Hata mesajı</param>
        public static void DoesNotThrowException(Action action, string? message = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var errorMessage = message ?? $"Expected no exception, but got {ex.GetType().Name}: {ex.Message}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// Async metodun exception fırlatmadığını kontrol eder
        /// </summary>
        /// <param name="asyncAction">Test edilecek async aksiyon</param>
        /// <param name="message">Hata mesajı</param>
        public static async Task DoesNotThrowExceptionAsync(Func<Task> asyncAction, string? message = null)
        {
            try
            {
                await asyncAction();
            }
            catch (Exception ex)
            {
                var errorMessage = message ?? $"Expected no exception, but got {ex.GetType().Name}: {ex.Message}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// İki nesnenin deep comparison ile eşit olup olmadığını kontrol eder
        /// </summary>
        /// <param name="obj1">İlk nesne</param>
        /// <param name="obj2">İkinci nesne</param>
        /// <returns>Eşitse true, değilse false</returns>
        private static bool AreObjectsEqual(object? obj1, object? obj2)
        {
            // Null kontrolü
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // Referans eşitliği
            if (ReferenceEquals(obj1, obj2)) return true;

            // Tür kontrolü
            if (obj1.GetType() != obj2.GetType()) return false;

            // Primitive türler
            if (obj1 is string str1 && obj2 is string str2)
                return string.Equals(str1, str2, StringComparison.Ordinal);

            if (obj1 is IEquatable<object> equatable1)
                return equatable1.Equals(obj2);

            if (obj1.Equals(obj2)) return true;

            // Koleksiyonlar
            if (obj1 is IEnumerable enumerable1 && obj2 is IEnumerable enumerable2)
            {
                var list1 = enumerable1.Cast<object>().ToList();
                var list2 = enumerable2.Cast<object>().ToList();

                if (list1.Count != list2.Count) return false;

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!AreObjectsEqual(list1[i], list2[i])) return false;
                }
                return true;
            }

            // Complex objects - reflection ile property'leri karşılaştır
            var properties = obj1.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var value1 = property.GetValue(obj1);
                    var value2 = property.GetValue(obj2);

                    if (!AreObjectsEqual(value1, value2)) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ServiceResult'ın başarılı olduğunu kontrol eder
        /// </summary>
        /// <typeparam name="T">ServiceResult türü</typeparam>
        /// <param name="result">Kontrol edilecek ServiceResult</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsSuccess<T>(ServiceResult<T> result, string? message = null)
        {
            if (!result.IsSuccess)
            {
                var errorMessage = message ?? $"Expected success, but got error: {string.Join(", ", result.ErrorMessages)}";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// ServiceResult'ın başarısız olduğunu kontrol eder
        /// </summary>
        /// <typeparam name="T">ServiceResult türü</typeparam>
        /// <param name="result">Kontrol edilecek ServiceResult</param>
        /// <param name="message">Hata mesajı</param>
        public static void IsFailure<T>(ServiceResult<T> result, string? message = null)
        {
            if (result.IsSuccess)
            {
                var errorMessage = message ?? "Expected failure, but got success";
                throw new AssertionException(errorMessage);
            }
        }

        /// <summary>
        /// ServiceResult'ın belirli hata mesajını içerdiğini kontrol eder
        /// </summary>
        /// <typeparam name="T">ServiceResult türü</typeparam>
        /// <param name="result">Kontrol edilecek ServiceResult</param>
        /// <param name="expectedErrorMessage">Beklenen hata mesajı</param>
        /// <param name="message">Hata mesajı</param>
        public static void ContainsErrorMessage<T>(ServiceResult<T> result, string expectedErrorMessage, string? message = null)
        {
            if (!result.ErrorMessages.Any(e => e.Contains(expectedErrorMessage)))
            {
                var errorMessage = message ?? $"Expected error message containing '{expectedErrorMessage}', but got: {string.Join(", ", result.ErrorMessages)}";
                throw new AssertionException(errorMessage);
            }
        }
    }

    /// <summary>
    /// Assertion exception sınıfı
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
        public AssertionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
