using Bogus;
using System.Reflection;
using WebLibrary.Models;

namespace WebLibrary.Testing
{
    /// <summary>
    /// Test verisi oluşturma yardımcısı - Bogus kütüphanesi kullanarak gerçekçi test verileri üretir
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Belirtilen türde fake veri oluşturur
        /// </summary>
        /// <typeparam name="T">Oluşturulacak veri türü</typeparam>
        /// <param name="count">Oluşturulacak veri sayısı</param>
        /// <param name="customRules">Özel kurallar</param>
        /// <returns>Fake veri listesi</returns>
        public static List<T> GenerateFakeData<T>(int count = 1, Action<Faker<T>>? customRules = null) where T : class
        {
            try
            {
                var faker = new Faker<T>();
                
                // Özel kurallar varsa uygula
                customRules?.Invoke(faker);
                
                return faker.Generate(count);
            }
            catch (Exception ex)
            {
                // Bogus desteklemeyen türler için fallback
                return GenerateFallbackData<T>(count);
            }
        }

        /// <summary>
        /// Belirtilen türde tek bir fake veri oluşturur
        /// </summary>
        /// <typeparam name="T">Oluşturulacak veri türü</typeparam>
        /// <param name="customRules">Özel kurallar</param>
        /// <returns>Fake veri</returns>
        public static T GenerateFakeData<T>(Action<Faker<T>>? customRules = null) where T : class
        {
            return GenerateFakeData(1, customRules).First();
        }

        /// <summary>
        /// Kullanıcı fake verisi oluşturur
        /// </summary>
        /// <param name="count">Oluşturulacak veri sayısı</param>
        /// <returns>Kullanıcı fake verisi listesi</returns>
        public static List<TestUser> GenerateUsers(int count = 1)
        {
            var faker = new Faker<TestUser>()
                .RuleFor(u => u.Id, f => f.Random.Guid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
                .RuleFor(u => u.IsActive, f => f.Random.Bool())
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
                .RuleFor(u => u.UpdatedAt, f => f.Date.Recent(1));

            return faker.Generate(count);
        }

        /// <summary>
        /// Ürün fake verisi oluşturur
        /// </summary>
        /// <param name="count">Oluşturulacak veri sayısı</param>
        /// <returns>Ürün fake verisi listesi</returns>
        public static List<TestProduct> GenerateProducts(int count = 1)
        {
            var faker = new Faker<TestProduct>()
                .RuleFor(p => p.Id, f => f.Random.Guid())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => f.Random.Decimal(1.0m, 1000.0m))
                .RuleFor(p => p.Category, f => f.Commerce.Categories(1).First())
                .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 1000))
                .RuleFor(p => p.IsAvailable, f => f.Random.Bool())
                .RuleFor(p => p.CreatedAt, f => f.Date.Past(1));

            return faker.Generate(count);
        }

        /// <summary>
        /// Sipariş fake verisi oluşturur
        /// </summary>
        /// <param name="count">Oluşturulacak veri sayısı</param>
        /// <returns>Sipariş fake verisi listesi</returns>
        public static List<TestOrder> GenerateOrders(int count = 1)
        {
            var faker = new Faker<TestOrder>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.OrderNumber, f => f.Random.Replace("ORD-####-####"))
                .RuleFor(o => o.CustomerId, f => f.Random.Guid())
                .RuleFor(o => o.TotalAmount, f => f.Random.Decimal(10.0m, 500.0m))
                .RuleFor(o => o.Status, f => f.PickRandom<TestOrderStatus>())
                .RuleFor(o => o.OrderDate, f => f.Date.Past(1))
                .RuleFor(o => o.ShippingAddress, f => f.Address.FullAddress());

            return faker.Generate(count);
        }

        /// <summary>
        /// Random string oluşturur
        /// </summary>
        /// <param name="length">String uzunluğu</param>
        /// <param name="includeSpecialChars">Özel karakterler dahil edilsin mi</param>
        /// <returns>Random string</returns>
        public static string GenerateRandomString(int length = 10, bool includeSpecialChars = false)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            
            var allowedChars = includeSpecialChars ? chars + specialChars : chars;
            
            return new string(Enumerable.Repeat(allowedChars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Random email oluşturur
        /// </summary>
        /// <returns>Random email</returns>
        public static string GenerateRandomEmail()
        {
            var domains = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "example.com" };
            var username = GenerateRandomString(8);
            var domain = domains[_random.Next(domains.Length)];
            
            return $"{username}@{domain}";
        }

        /// <summary>
        /// Random telefon numarası oluşturur
        /// </summary>
        /// <returns>Random telefon numarası</returns>
        public static string GenerateRandomPhoneNumber()
        {
            return $"+90{_random.Next(500, 600)}{_random.Next(100, 999)}{_random.Next(1000, 9999)}";
        }

        /// <summary>
        /// Random tarih oluşturur
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Random tarih</returns>
        public static DateTime GenerateRandomDate(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.AddYears(-10);
            var end = endDate ?? DateTime.Now;
            
            var timeSpan = end - start;
            var randomDays = _random.Next(0, (int)timeSpan.TotalDays);
            
            return start.AddDays(randomDays);
        }

        /// <summary>
        /// Bogus desteklemeyen türler için fallback veri oluşturur
        /// </summary>
        /// <typeparam name="T">Oluşturulacak veri türü</typeparam>
        /// <param name="count">Oluşturulacak veri sayısı</param>
        /// <returns>Fallback veri listesi</returns>
        private static List<T> GenerateFallbackData<T>(int count) where T : class
        {
            var result = new List<T>();
            
            for (int i = 0; i < count; i++)
            {
                var instance = Activator.CreateInstance<T>();
                
                // Property'leri random değerlerle doldur
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var property in properties)
                {
                    if (property.CanWrite)
                    {
                        var value = GenerateRandomValueForType(property.PropertyType);
                        if (value != null)
                        {
                            property.SetValue(instance, value);
                        }
                    }
                }
                
                result.Add(instance);
            }
            
            return result;
        }

        /// <summary>
        /// Veri türüne göre random değer oluşturur
        /// </summary>
        /// <param name="type">Veri türü</param>
        /// <returns>Random değer</returns>
        private static object? GenerateRandomValueForType(Type type)
        {
            if (type == typeof(string))
                return GenerateRandomString();
            else if (type == typeof(int))
                return _random.Next(1, 1000);
            else if (type == typeof(long))
                return _random.Next(1, 1000000);
            else if (type == typeof(double))
                return _random.NextDouble() * 1000;
            else if (type == typeof(decimal))
                return _random.Next(1, 1000);
            else if (type == typeof(bool))
                return _random.Next(2) == 1;
            else if (type == typeof(DateTime))
                return GenerateRandomDate();
            else if (type == typeof(Guid))
                return Guid.NewGuid();
            else if (type.IsEnum)
                return Enum.GetValues(type).GetValue(_random.Next(Enum.GetValues(type).Length));
            else if (type == typeof(byte[]))
                return new byte[_random.Next(10, 100)];
            
            return null;
        }
    }

    #region Test Models

    /// <summary>
    /// Test kullanıcı modeli
    /// </summary>
    public class TestUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Test ürün modeli
    /// </summary>
    public class TestProduct
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Test sipariş modeli
    /// </summary>
    public class TestOrder
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public TestOrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test sipariş durumu enum'u
    /// </summary>
    public enum TestOrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    #endregion
}
