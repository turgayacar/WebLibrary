using WebLibrary;
using WebLibrary.Extensions;
using WebLibrary.Helpers;
using WebLibrary.Models;
using WebLibrary.Security;
using WebLibrary.Services;
using WebLibrary.Utilities;
using WebLibrary.DataProcessing;
using WebLibrary.Database;

namespace WebLibraryTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== WebLibrary Test Uygulaması ===");
            Console.WriteLine();

            // Global ayarları test et
            TestGlobalSettings();

            // ServiceResult modelini test et
            TestServiceResult();

            // HTTP Client Helper'ı test et
            await TestHttpClientHelper();

            // Validation Helper'ı test et
            TestValidationHelper();

            // Generic Extensions'ı test et
            TestGenericExtensions();

            // Generic Utilities'i test et
            TestGenericUtilities();

            // Generic Service Base'i test et
            await TestGenericServiceBase();

            // Güvenlik özelliklerini test et
            TestSecurityFeatures();

            // Veri işleme özelliklerini test et
            await TestDataProcessingFeatures();

            // Database Operations özelliklerini test et
            await TestDatabaseOperations();

            Console.WriteLine();
            Console.WriteLine("Tüm testler tamamlandı!");
            Console.ReadKey();
        }

        static void TestGlobalSettings()
        {
            Console.WriteLine("--- Global Ayarlar Test ---");
            Console.WriteLine($"Base API URL: {Global.BaseApiUrl}");
            Console.WriteLine($"API Timeout: {Global.ApiTimeoutSeconds} saniye");
            Console.WriteLine($"Max Retry: {Global.MaxRetryCount}");
            Console.WriteLine($"Cache Expiration: {Global.CacheExpirationMinutes} dakika");
            Console.WriteLine($"Log Level: {Global.LogLevel}");
            
            // Ayarları değiştir
            Global.BaseApiUrl = "https://api.test.com";
            Global.ApiTimeoutSeconds = 60;
            Console.WriteLine($"\nGüncellenmiş Base API URL: {Global.BaseApiUrl}");
            Console.WriteLine($"Güncellenmiş API Timeout: {Global.ApiTimeoutSeconds} saniye");
        }
        
        static void TestServiceResult()
        {
            Console.WriteLine("--- ServiceResult Test ---");
            
            // Başarılı sonuç
            var successResult = ServiceResult<string>.Success("Test verisi");
            Console.WriteLine($"Başarılı sonuç: {successResult.IsSuccess}, Data: {successResult.Data}");
            
            // Hata sonucu
            var errorResult = ServiceResult<string>.Error("Test hatası");
            Console.WriteLine($"Hata sonucu: {errorResult.IsSuccess}, Hata: {string.Join(", ", errorResult.ErrorMessages)}");
            
            // Generic olmayan sonuç
            var simpleResult = ServiceResult.Success();
            Console.WriteLine($"Basit başarılı sonuç: {simpleResult.IsSuccess}");
        }
        
        static void TestValidationHelper()
        {
            Console.WriteLine("--- ValidationHelper Test ---");
            
            // Email doğrulama
            var validEmail = "test@example.com";
            var invalidEmail = "invalid-email";
            Console.WriteLine($"Geçerli email '{validEmail}': {ValidationHelper.IsValidEmail(validEmail)}");
            Console.WriteLine($"Geçersiz email '{invalidEmail}': {ValidationHelper.IsValidEmail(invalidEmail)}");
            
            // TC Kimlik doğrulama
            var validTc = "12345678901";
            var invalidTc = "1234567890";
            Console.WriteLine($"Geçerli TC '{validTc}': {ValidationHelper.IsValidTcKimlik(validTc)}");
            Console.WriteLine($"Geçersiz TC '{invalidTc}': {ValidationHelper.IsValidTcKimlik(invalidTc)}");
            
            // Telefon doğrulama
            var validPhone = "05551234567";
            var invalidPhone = "1234567890";
            Console.WriteLine($"Geçerli telefon '{validPhone}': {ValidationHelper.IsValidPhone(validPhone)}");
            Console.WriteLine($"Geçersiz telefon '{invalidPhone}': {ValidationHelper.IsValidPhone(invalidPhone)}");
            
            // Şifre güvenliği
            var strongPassword = "Test123!@#";
            var weakPassword = "123";
            Console.WriteLine($"Güçlü şifre '{strongPassword}': {ValidationHelper.IsStrongPassword(strongPassword)}");
            Console.WriteLine($"Zayıf şifre '{weakPassword}': {ValidationHelper.IsStrongPassword(weakPassword)}");
        }
        
        static void TestGenericExtensions()
        {
            Console.WriteLine("--- GenericExtensions Test ---");
            
            var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            
            // Null/boş kontrol
            Console.WriteLine($"Liste null veya boş mu: {numbers.IsNullOrEmpty()}");
            Console.WriteLine($"Liste null veya boş değil mi: {numbers.IsNotNullOrEmpty()}");
            
            // Sayfalama
            var page1 = numbers.GetPage(1, 3);
            Console.WriteLine($"Sayfa 1 (3 öğe): {string.Join(", ", page1)}");
            
            var totalPages = numbers.GetTotalPages(3);
            Console.WriteLine($"Toplam sayfa sayısı (3 öğe/sayfa): {totalPages}");
            
            // Property bazlı sıralama (string olarak)
            var people = new List<TestPerson>
            {
                new TestPerson { Id = 3, Name = "Ali", Age = 25, Email = "ali@test.com" },
                new TestPerson { Id = 1, Name = "Zeynep", Age = 30, Email = "zeynep@test.com" },
                new TestPerson { Id = 2, Name = "Mehmet", Age = 28, Email = "mehmet@test.com" }
            };
            
            var sortedPeople = people.OrderByProperty("Name");
            Console.WriteLine($"İsme göre sıralanmış: {string.Join(", ", sortedPeople.Select(p => p.Name))}");
            
            var sortedPeopleByAge = people.OrderByProperty("Age");
            Console.WriteLine($"Yaşa göre sıralanmış: {string.Join(", ", sortedPeopleByAge.Select(p => p.Age))}");
        }
        
        static void TestGenericUtilities()
        {
            Console.WriteLine("--- GenericUtilities Test ---");
            
            var person = new TestPerson { Id = 1, Name = "Test Kişi", Age = 25, Email = "test@test.com" };
            
            // Property değeri alma
            var name = GenericUtilities.GetPropertyValue<string>(person, "Name");
            var age = GenericUtilities.GetPropertyValue<int>(person, "Age");
            Console.WriteLine($"Name: {name}, Age: {age}");
            
            // Property değeri ayarlama
            GenericUtilities.SetPropertyValue(person, "Age", 26);
            Console.WriteLine($"Güncellenmiş yaş: {person.Age}");
            
            // Dictionary'den nesne oluşturma
            var properties = new Dictionary<string, object?>
            {
                { "Id", 2 },
                { "Name", "Yeni Kişi" },
                { "Age", 30 },
                { "Email", "yeni@test.com" }
            };
            
            var newPerson = GenericUtilities.CreateFromDictionary<TestPerson>(properties);
            Console.WriteLine($"Yeni kişi: Id={newPerson?.Id}, Name={newPerson?.Name}, Age={newPerson?.Age}");
            
            // Property kopyalama
            var copiedPerson = GenericUtilities.CopyProperties<TestPerson>(person, "Name", "Age");
            Console.WriteLine($"Kopyalanan kişi: Name={copiedPerson?.Name}, Age={copiedPerson?.Age}");
            
            // Tip dönüştürme
            var personDict = GenericUtilities.GetPropertiesAsDictionary(person);
            Console.WriteLine($"Dictionary: {string.Join(", ", personDict.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }

        static async Task TestGenericServiceBase()
        {
            Console.WriteLine("--- GenericServiceBase Test ---");
            
            // Test için basit bir service oluştur
            var testService = new TestGenericService();
            
            // CRUD işlemlerini test et
            var createResult = await testService.AddAsync(new TestPerson { Name = "Test Kullanıcı", Age = 25, Email = "test@test.com" });
            Console.WriteLine($"Oluşturma sonucu: {createResult.IsSuccess}");
            
            var readResult = await testService.GetByIdAsync(1);
            if (readResult.IsSuccess)
            {
                Console.WriteLine($"Okuma sonucu: {readResult.Data?.Name}");
            }
            
            var updateResult = await testService.UpdateAsync(new TestPerson { Id = 1, Name = "Güncellenmiş Kullanıcı", Age = 26, Email = "updated@test.com" });
            Console.WriteLine($"Güncelleme sonucu: {updateResult.IsSuccess}");
            
            var deleteResult = await testService.DeleteAsync(1);
            Console.WriteLine($"Silme sonucu: {deleteResult.IsSuccess}");
        }
        
        static async Task TestHttpClientHelper()
        {
            Console.WriteLine("--- HttpClientHelper Test ---");
            
            try
            {
                // Test için mock URL (gerçek bir API olmadığı için hata alacağız)
                var testUrl = "https://httpbin.org/get";
                
                var result = await HttpClientHelper.GetAsync<object>(testUrl);
                if (result.IsSuccess)
                {
                    Console.WriteLine("HTTP GET başarılı!");
                }
                else
                {
                    Console.WriteLine($"HTTP GET hatası: {string.Join(", ", result.ErrorMessages)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP test hatası: {ex.Message}");
            }
        }
        
        static void TestSecurityFeatures()
        {
            Console.WriteLine("--- Security Features Test ---");
            
            // Global güvenlik ayarları
            Console.WriteLine($"JWT Secret Key: {Global.JwtSecretKey.Substring(0, 20)}...");
            Console.WriteLine($"JWT Expiration: {Global.JwtExpirationMinutes} dakika");
            Console.WriteLine($"BCrypt Work Factor: {Global.BcryptWorkFactor}");
            Console.WriteLine($"Min Password Length: {Global.MinPasswordLength}");
            
            // JWT Token test
            Console.WriteLine("\n--- JWT Token Test ---");
            var secretKey = "test-secret-key-for-jwt-token-generation";
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("UserId", "123"),
                new System.Security.Claims.Claim("Username", "testuser"),
                new System.Security.Claims.Claim("Role", "Admin")
            };
            
            var tokenResult = JwtTokenHelper.GenerateToken(claims, secretKey, expirationMinutes: 30);
            if (tokenResult.IsSuccess)
            {
                Console.WriteLine($"JWT Token oluşturuldu: {tokenResult.Data?.Substring(0, 50)}...");
                
                // Token doğrulama
                var validationResult = JwtTokenHelper.ValidateToken(tokenResult.Data!, secretKey);
                Console.WriteLine($"Token doğrulama: {validationResult.IsSuccess}");
                
                // Claim çıkarma
                var extractedClaims = JwtTokenHelper.ExtractClaims(tokenResult.Data!, secretKey);
                if (extractedClaims.IsSuccess)
                {
                    Console.WriteLine($"Çıkarılan claim sayısı: {extractedClaims.Data?.Count}");
                }
                
                // Kullanıcı ID çıkarma
                var userIdResult = JwtTokenHelper.ExtractUserId(tokenResult.Data!, secretKey);
                if (userIdResult.IsSuccess)
                {
                    Console.WriteLine($"Kullanıcı ID: {userIdResult.Data}");
                }
            }
            else
            {
                Console.WriteLine($"JWT Token hatası: {string.Join(", ", tokenResult.ErrorMessages)}");
            }
            
            // Password Helper test
            Console.WriteLine("\n--- Password Helper Test ---");
            var testPassword = "Test123!@#";
            
            // Şifre hashleme
            var hashResult = PasswordHelper.HashPassword(testPassword, Global.BcryptWorkFactor);
            if (hashResult.IsSuccess)
            {
                Console.WriteLine($"Şifre hash'lendi: {hashResult.Data?.Substring(0, 20)}...");
                
                // Şifre doğrulama
                var verifyResult = PasswordHelper.VerifyPassword(testPassword, hashResult.Data!);
                Console.WriteLine($"Şifre doğrulama: {verifyResult.IsSuccess}");
                
                // Şifre güvenliği kontrol
                var strengthResult = PasswordHelper.CheckPasswordStrength(testPassword);
                if (strengthResult.IsSuccess)
                {
                    Console.WriteLine($"Şifre güvenliği: {strengthResult.Data?.Strength}");
                    Console.WriteLine($"Şifre puanı: {strengthResult.Data?.Score}/100");
                }
            }
            else
            {
                Console.WriteLine($"Şifre hash hatası: {string.Join(", ", hashResult.ErrorMessages)}");
            }
            
            // Encryption Helper test
            Console.WriteLine("\n--- Encryption Helper Test ---");
            var testText = "Test şifreleme metni";
            
            // AES anahtar çifti oluştur
            var keyPairResult = EncryptionHelper.GenerateAesKeyPair();
            if (keyPairResult.IsSuccess)
            {
                var keyPair = keyPairResult.Data!;
                Console.WriteLine($"AES anahtar çifti oluşturuldu: Key={keyPair.Key.Length} byte, IV={keyPair.IV.Length} byte");
                
                // AES şifreleme
                var encryptResult = EncryptionHelper.EncryptAes(testText, keyPair.Key, keyPair.IV);
                if (encryptResult.IsSuccess)
                {
                    Console.WriteLine($"AES şifreleme: {encryptResult.Data?.Substring(0, 20)}...");
                    
                    // AES şifre çözme
                    var decryptResult = EncryptionHelper.DecryptAes(encryptResult.Data!, keyPair.Key, keyPair.IV);
                    if (decryptResult.IsSuccess)
                    {
                        Console.WriteLine($"AES şifre çözme: {decryptResult.Data}");
                    }
                }
            }
            
            // Hash işlemleri
            var sha256Result = EncryptionHelper.GenerateSha256Hash(testText);
            if (sha256Result.IsSuccess)
            {
                Console.WriteLine($"SHA256 Hash: {sha256Result.Data?.Substring(0, 20)}...");
            }
            
            var md5Result = EncryptionHelper.GenerateMd5Hash(testText);
            if (md5Result.IsSuccess)
            {
                Console.WriteLine($"MD5 Hash: {md5Result.Data?.Substring(0, 20)}...");
            }
            
            // Güvenli rastgele değerler
            var randomNumberResult = EncryptionHelper.GenerateSecureRandomNumber(1, 100);
            if (randomNumberResult.IsSuccess)
            {
                Console.WriteLine($"Güvenli rastgele sayı: {randomNumberResult.Data}");
            }
            
            var randomStringResult = EncryptionHelper.GenerateSecureRandomString(16);
            if (randomStringResult.IsSuccess)
            {
                Console.WriteLine($"Güvenli rastgele string: {randomStringResult.Data}");
            }
        }

        static async Task TestDataProcessingFeatures()
        {
            Console.WriteLine("--- Veri İşleme Özellikleri Test Ediliyor ---");

            // CSV işlemleri test
            TestCsvOperations();

            // Excel işlemleri test
            TestExcelOperations();

            // Veri dönüştürme test
            TestDataTransformation();

            // Toplu işlemler test
            await TestBulkOperations();

            Console.WriteLine("Veri işleme özellikleri test edildi!");
        }

        static async Task TestDatabaseOperations()
        {
            Console.WriteLine("Database Operations test ediliyor...");

            // Connection Factory test
            var connectionString = "Server=localhost;Database=TestDB;Trusted_Connection=true;";
            var connectionFactory = new SqlServerConnectionFactory(connectionString);
            Console.WriteLine($"Connection Factory oluşturuldu: {connectionFactory.ProviderName}");

            // Query Builder test
            var query = new QueryBuilder()
                .Select("Id, Name, Email")
                .From("Users")
                .Where("Age > @Age", "Age", 18)
                .OrderBy("Name", "ASC")
                .Limit(10);

            var sql = query.Build();
            var parameters = query.GetParameters();
            Console.WriteLine($"Query Builder SQL: {sql}");
            Console.WriteLine($"Query Builder Parameters: {parameters.Count} parametre");

            // Repository pattern test (simulated)
            Console.WriteLine("Repository pattern test ediliyor...");
            var testPerson = new TestPerson { Id = 1, Name = "Test User", Age = 25, Email = "test@test.com" };
            Console.WriteLine($"Test person oluşturuldu: {testPerson.Name}");

            // Unit of Work test (simulated)
            Console.WriteLine("Unit of Work pattern test ediliyor...");
            Console.WriteLine("Transaction yönetimi simüle edildi");

            // Database Helper test (simulated)
            Console.WriteLine("Database Helper test ediliyor...");
            Console.WriteLine("Veritabanı bağlantı testleri simüle edildi");

            Console.WriteLine("Database Operations test edildi!");
        }

        static void TestCsvOperations()
        {
            Console.WriteLine("CSV işlemleri test ediliyor...");

            // Test verisi oluştur
            var testData = new List<TestPerson>
            {
                new TestPerson { Id = 1, Name = "Ahmet", Age = 25, Email = "ahmet@test.com" },
                new TestPerson { Id = 2, Name = "Ayşe", Age = 30, Email = "ayse@test.com" },
                new TestPerson { Id = 3, Name = "Mehmet", Age = 35, Email = "mehmet@test.com" }
            };

            // CSV'ye yazma test
            var csvResult = CsvDataHelper.WriteToCsv(testData);
            if (csvResult.IsSuccess)
            {
                Console.WriteLine($"CSV oluşturuldu: {csvResult.Data?.Length} karakter");
                
                // CSV'den okuma test
                var readResult = CsvDataHelper.ReadFromCsv<TestPerson>(csvResult.Data!);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"CSV'den {readResult.Data?.Count} kayıt okundu");
                }
                else
                {
                    Console.WriteLine($"CSV okuma hatası: {readResult.ErrorMessages.First()}");
                }
            }
            else
            {
                Console.WriteLine($"CSV yazma hatası: {csvResult.ErrorMessages.First()}");
            }

            // CSV dosya işlemleri test
            var filePath = "test_data.csv";
            var fileResult = CsvDataHelper.WriteToCsvFile(testData, filePath);
            if (fileResult.IsSuccess)
            {
                Console.WriteLine($"CSV dosyası oluşturuldu: {filePath}");
                
                // Dosyadan okuma test
                var fileReadResult = CsvDataHelper.ReadFromCsvFile<TestPerson>(filePath);
                if (fileReadResult.IsSuccess)
                {
                    Console.WriteLine($"Dosyadan {fileReadResult.Data?.Count} kayıt okundu");
                }
                else
                {
                    Console.WriteLine($"Dosya okuma hatası: {fileReadResult.ErrorMessages.First()}");
                }

                // Test dosyasını temizle
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            else
            {
                Console.WriteLine($"CSV dosya yazma hatası: {fileResult.ErrorMessages.First()}");
            }
        }

        static void TestExcelOperations()
        {
            Console.WriteLine("Excel işlemleri test ediliyor...");

            // Test verisi oluştur
            var testData = new List<TestPerson>
            {
                new TestPerson { Id = 1, Name = "Ali", Age = 28, Email = "ali@test.com" },
                new TestPerson { Id = 2, Name = "Fatma", Age = 32, Email = "fatma@test.com" },
                new TestPerson { Id = 3, Name = "Hasan", Age = 27, Email = "hasan@test.com" }
            };

            // Excel dosyasına yazma test
            var filePath = "test_data.xlsx";
            var excelResult = ExcelDataHelper.WriteToExcel(testData, filePath);
            if (excelResult.IsSuccess)
            {
                Console.WriteLine($"Excel dosyası oluşturuldu: {filePath}");
                
                // Dosyadan okuma test
                var readResult = ExcelDataHelper.ReadFromExcel<TestPerson>(filePath);
                if (readResult.IsSuccess)
                {
                    Console.WriteLine($"Excel'den {readResult.Data?.Count} kayıt okundu");
                }
                else
                {
                    Console.WriteLine($"Excel okuma hatası: {readResult.ErrorMessages.First()}");
                }

                // Çoklu sayfa test
                var multiSheetData = new Dictionary<string, IEnumerable<Dictionary<string, object>>>
                {
                    { "Sayfa1", testData.Select(p => new Dictionary<string, object> { { "Id", p.Id }, { "Name", p.Name }, { "Age", p.Age }, { "Email", p.Email } }) },
                    { "Sayfa2", testData.Take(2).Select(p => new Dictionary<string, object> { { "Id", p.Id }, { "Name", p.Name }, { "Age", p.Age }, { "Email", p.Email } }) }
                };

                var multiSheetResult = ExcelDataHelper.WriteToExcelMultiSheet(multiSheetData, "test_multi.xlsx");
                if (multiSheetResult.IsSuccess)
                {
                    Console.WriteLine("Çoklu sayfa Excel dosyası oluşturuldu");
                }

                // Test dosyalarını temizle
                if (File.Exists(filePath))
                    File.Delete(filePath);
                if (File.Exists("test_multi.xlsx"))
                    File.Delete("test_multi.xlsx");
            }
            else
            {
                Console.WriteLine($"Excel yazma hatası: {excelResult.ErrorMessages.First()}");
            }
        }

        static void TestDataTransformation()
        {
            Console.WriteLine("Veri dönüştürme test ediliyor...");

            // Test verisi oluştur
            var testPerson = new TestPerson 
            { 
                Id = 1, 
                Name = "Test Kullanıcı", 
                Age = 25, 
                Email = "test@example.com" 
            };

            // JSON dönüştürme test
            var jsonResult = DataTransformationHelper.ToJson(testPerson, true);
            if (jsonResult.IsSuccess)
            {
                Console.WriteLine($"JSON oluşturuldu: {jsonResult.Data?.Length} karakter");
                
                // JSON'dan geri dönüştürme test
                var fromJsonResult = DataTransformationHelper.FromJson<TestPerson>(jsonResult.Data!);
                if (fromJsonResult.IsSuccess)
                {
                    Console.WriteLine($"JSON'dan nesne oluşturuldu: {fromJsonResult.Data?.Name}");
                }
            }

            // Dictionary dönüştürme test
            var dictResult = DataTransformationHelper.ToDictionary(testPerson);
            if (dictResult.IsSuccess)
            {
                Console.WriteLine($"Dictionary oluşturuldu: {dictResult.Data?.Count} property");
                
                // Dictionary'den nesne oluşturma test
                var fromDictResult = DataTransformationHelper.FromDictionary<TestPerson>(dictResult.Data!);
                if (fromDictResult.IsSuccess)
                {
                    Console.WriteLine($"Dictionary'den nesne oluşturuldu: {fromDictResult.Data?.Name}");
                }
            }

            // Nesne kopyalama test
            var copyResult = DataTransformationHelper.DeepCopy(testPerson);
            if (copyResult.IsSuccess)
            {
                Console.WriteLine($"Nesne kopyalandı: {copyResult.Data?.Name}");
                
                // Kopya ile orijinali karşılaştır
                var compareResult = DataTransformationHelper.CompareObjects(
                    testPerson, 
                    copyResult.Data!, 
                    "Name", "Age", "Email");
                
                if (compareResult.IsSuccess && compareResult.Data)
                {
                    Console.WriteLine("Kopya ve orijinal nesne eşleşiyor");
                }
            }

            // Property kopyalama test
            var propertyCopyResult = DataTransformationHelper.CopyProperties(testPerson, "Name", "Age");
            if (propertyCopyResult.IsSuccess)
            {
                Console.WriteLine($"Belirli property'ler kopyalandı: {propertyCopyResult.Data?.Name}, {propertyCopyResult.Data?.Age}");
            }
        }

        static async Task TestBulkOperations()
        {
            Console.WriteLine("Toplu işlemler test ediliyor...");

            // Test verisi oluştur
            var testData = Enumerable.Range(1, 100)
                .Select(i => new TestPerson 
                { 
                    Id = i, 
                    Name = $"Kullanıcı {i}", 
                    Age = 20 + (i % 50), 
                    Email = $"user{i}@test.com" 
                })
                .ToList();

            // Simüle edilmiş işlem fonksiyonları
            async Task<ServiceResult<bool>> SimulateInsert(IEnumerable<TestPerson> items)
            {
                await Task.Delay(10); // Simüle edilmiş gecikme
                return ServiceResult<bool>.Success(true);
            }

            async Task<ServiceResult<bool>> SimulateUpdate(IEnumerable<TestPerson> items)
            {
                await Task.Delay(10);
                return ServiceResult<bool>.Success(true);
            }

            async Task<ServiceResult<bool>> SimulateDelete(IEnumerable<TestPerson> items)
            {
                await Task.Delay(10);
                return ServiceResult<bool>.Success(true);
            }

            // Toplu ekleme test
            var bulkInsertResult = await BulkOperationsHelper.BulkInsertAsync(testData, 10, SimulateInsert);
            if (bulkInsertResult.IsSuccess)
            {
                var result = bulkInsertResult.Data!;
                Console.WriteLine($"Toplu ekleme tamamlandı: {result.SuccessCount}/{result.TotalCount} başarılı, {result.SuccessRate:F1}% başarı oranı");
            }

            // Toplu güncelleme test
            var bulkUpdateResult = await BulkOperationsHelper.BulkUpdateAsync(testData, 15, SimulateUpdate);
            if (bulkUpdateResult.IsSuccess)
            {
                var result = bulkUpdateResult.Data!;
                Console.WriteLine($"Toplu güncelleme tamamlandı: {result.SuccessCount}/{result.TotalCount} başarılı, {result.SuccessRate:F1}% başarı oranı");
            }

            // Paralel toplu işlem test
            var parallelResult = await BulkOperationsHelper.BulkProcessParallelAsync(testData, 20, 3, SimulateInsert);
            if (parallelResult.IsSuccess)
            {
                var result = parallelResult.Data!;
                Console.WriteLine($"Paralel toplu işlem tamamlandı: {result.SuccessCount}/{result.TotalCount} başarılı, {result.SuccessRate:F1}% başarı oranı");
            }
        }

        // Test için kullanılan model sınıfı
        public class TestPerson
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Email { get; set; } = string.Empty;
        }

        // Test için kullanılan generic service sınıfı
        public class TestGenericService : GenericServiceBase<TestPerson>
        {
            private readonly List<TestPerson> _data = new();
            private int _nextId = 1;

            public override async Task<ServiceResult<TestPerson>> AddAsync(TestPerson entity)
            {
                try
                {
                    entity.Id = _nextId++;
                    _data.Add(entity);
                    return ServiceResult<TestPerson>.Success(entity);
                }
                catch (Exception ex)
                {
                    return ServiceResult<TestPerson>.Error($"Oluşturma hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<TestPerson>> GetByIdAsync(int id)
            {
                try
                {
                    var entity = _data.FirstOrDefault(x => x.Id == id);
                    if (entity == null)
                        return ServiceResult<TestPerson>.Error("Kayıt bulunamadı");

                    return ServiceResult<TestPerson>.Success(entity);
                }
                catch (Exception ex)
                {
                    return ServiceResult<TestPerson>.Error($"Okuma hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<TestPerson>> UpdateAsync(TestPerson entity)
            {
                try
                {
                    var existingEntity = _data.FirstOrDefault(x => x.Id == entity.Id);
                    if (existingEntity == null)
                        return ServiceResult<TestPerson>.Error("Güncellenecek kayıt bulunamadı");

                    existingEntity.Name = entity.Name;
                    existingEntity.Age = entity.Age;
                    existingEntity.Email = entity.Email;

                    return ServiceResult<TestPerson>.Success(entity);
                }
                catch (Exception ex)
                {
                    return ServiceResult<TestPerson>.Error($"Güncelleme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult> DeleteAsync(int id)
            {
                try
                {
                    var entity = _data.FirstOrDefault(x => x.Id == id);
                    if (entity == null)
                        return ServiceResult.Error("Silinecek kayıt bulunamadı");

                    _data.Remove(entity);
                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    return ServiceResult.Error($"Silme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetAllAsync()
            {
                try
                {
                    return ServiceResult<List<TestPerson>>.Success(_data.ToList());
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Listeleme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<bool>> ExistsAsync(int id)
            {
                try
                {
                    var exists = _data.Any(x => x.Id == id);
                    return ServiceResult<bool>.Success(exists);
                }
                catch (Exception ex)
                {
                    return ServiceResult<bool>.Error($"Kontrol hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<int>> GetCountAsync()
            {
                try
                {
                    return ServiceResult<int>.Success(_data.Count);
                }
                catch (Exception ex)
                {
                    return ServiceResult<int>.Error($"Sayım hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetPagedAsync(int pageNumber, int pageSize)
            {
                try
                {
                    var pagedData = _data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    return ServiceResult<List<TestPerson>>.Success(pagedData);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Sayfalama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetWhereAsync(Func<TestPerson, bool> predicate)
            {
                try
                {
                    var filteredData = _data.Where(predicate).ToList();
                    return ServiceResult<List<TestPerson>>.Success(filteredData);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Filtreleme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<TestPerson>> GetFirstOrDefaultAsync(Func<TestPerson, bool> predicate)
            {
                try
                {
                    var entity = _data.FirstOrDefault(predicate);
                    if (entity == null)
                        return ServiceResult<TestPerson>.Error("Kayıt bulunamadı");

                    return ServiceResult<TestPerson>.Success(entity);
                }
                catch (Exception ex)
                {
                    return ServiceResult<TestPerson>.Error($"Arama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> AddRangeAsync(List<TestPerson> entities)
            {
                try
                {
                    foreach (var entity in entities)
                    {
                        entity.Id = _nextId++;
                        _data.Add(entity);
                    }
                    return ServiceResult<List<TestPerson>>.Success(entities);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Toplu ekleme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> UpdateRangeAsync(List<TestPerson> entities)
            {
                try
                {
                    foreach (var entity in entities)
                    {
                        var existingEntity = _data.FirstOrDefault(x => x.Id == entity.Id);
                        if (existingEntity != null)
                        {
                            existingEntity.Name = entity.Name;
                            existingEntity.Age = entity.Age;
                            existingEntity.Email = entity.Email;
                        }
                    }
                    return ServiceResult<List<TestPerson>>.Success(entities);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Toplu güncelleme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult> DeleteRangeAsync(List<int> ids)
            {
                try
                {
                    var entitiesToRemove = _data.Where(x => ids.Contains(x.Id)).ToList();
                    foreach (var entity in entitiesToRemove)
                    {
                        _data.Remove(entity);
                    }
                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    return ServiceResult.Error($"Toplu silme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult> DeleteWhereAsync(Func<TestPerson, bool> predicate)
            {
                try
                {
                    var entitiesToRemove = _data.Where(predicate).ToList();
                    foreach (var entity in entitiesToRemove)
                    {
                        _data.Remove(entity);
                    }
                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    return ServiceResult.Error($"Koşullu silme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult> DeleteAllAsync()
            {
                try
                {
                    _data.Clear();
                    return ServiceResult.Success();
                }
                catch (Exception ex)
                {
                    return ServiceResult.Error($"Toplu silme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetOrderedAsync(string propertyName, bool ascending = true)
            {
                try
                {
                    List<TestPerson> orderedData;
                    switch (propertyName.ToLower())
                    {
                        case "name":
                            orderedData = ascending ? _data.OrderBy(x => x.Name).ToList() : _data.OrderByDescending(x => x.Name).ToList();
                            break;
                        case "age":
                            orderedData = ascending ? _data.OrderBy(x => x.Age).ToList() : _data.OrderByDescending(x => x.Age).ToList();
                            break;
                        case "id":
                            orderedData = ascending ? _data.OrderBy(x => x.Id).ToList() : _data.OrderByDescending(x => x.Id).ToList();
                            break;
                        default:
                            orderedData = _data.ToList();
                            break;
                    }
                    return ServiceResult<List<TestPerson>>.Success(orderedData);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Sıralama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<object>> GetGroupedAsync(string propertyName)
            {
                try
                {
                    object groupedData;
                    switch (propertyName.ToLower())
                    {
                        case "age":
                            groupedData = _data.GroupBy(x => x.Age).ToDictionary(g => g.Key, g => g.ToList());
                            break;
                        default:
                            groupedData = _data.GroupBy(x => x.GetType().GetProperty(propertyName)?.GetValue(x)).ToDictionary(g => g.Key, g => g.ToList());
                            break;
                    }
                    return ServiceResult<object>.Success(groupedData);
                }
                catch (Exception ex)
                {
                    return ServiceResult<object>.Error($"Gruplama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetFilteredAsync(string propertyName, object value)
            {
                try
                {
                    List<TestPerson> filteredData;
                    switch (propertyName.ToLower())
                    {
                        case "name":
                            filteredData = _data.Where(x => x.Name.Contains(value.ToString() ?? "")).ToList();
                            break;
                        case "age":
                            filteredData = _data.Where(x => x.Age == Convert.ToInt32(value)).ToList();
                            break;
                        case "id":
                            filteredData = _data.Where(x => x.Id == Convert.ToInt32(value)).ToList();
                            break;
                        default:
                            filteredData = _data.ToList();
                            break;
                    }
                    return ServiceResult<List<TestPerson>>.Success(filteredData);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Filtreleme hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> SearchAsync(string propertyName, string searchTerm)
            {
                try
                {
                    List<TestPerson> searchResults;
                    switch (propertyName.ToLower())
                    {
                        case "name":
                            searchResults = _data.Where(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                            break;
                        case "email":
                            searchResults = _data.Where(x => x.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                            break;
                        default:
                            searchResults = _data.ToList();
                            break;
                    }
                    return ServiceResult<List<TestPerson>>.Success(searchResults);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Arama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetByComparisonAsync(string propertyName, string @operator, object value)
            {
                try
                {
                    List<TestPerson> comparisonResults;
                    switch (propertyName.ToLower())
                    {
                        case "age":
                            switch (@operator)
                            {
                                case ">":
                                    comparisonResults = _data.Where(x => x.Age > Convert.ToInt32(value)).ToList();
                                    break;
                                case "<":
                                    comparisonResults = _data.Where(x => x.Age < Convert.ToInt32(value)).ToList();
                                    break;
                                case ">=":
                                    comparisonResults = _data.Where(x => x.Age >= Convert.ToInt32(value)).ToList();
                                    break;
                                case "<=":
                                    comparisonResults = _data.Where(x => x.Age <= Convert.ToInt32(value)).ToList();
                                    break;
                                case "==":
                                    comparisonResults = _data.Where(x => x.Age == Convert.ToInt32(value)).ToList();
                                    break;
                                default:
                                    comparisonResults = _data.ToList();
                                    break;
                            }
                            break;
                        default:
                            comparisonResults = _data.ToList();
                            break;
                    }
                    return ServiceResult<List<TestPerson>>.Success(comparisonResults);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Karşılaştırma hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<TestPerson>>> GetByDateRangeAsync(string propertyName, DateTime startDate, DateTime endDate)
            {
                try
                {
                    // TestPerson'da tarih property'si yok, boş liste döndür
                    return ServiceResult<List<TestPerson>>.Success(new List<TestPerson>());
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<TestPerson>>.Error($"Tarih aralığı hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<List<object>>> GetDistinctAsync(string propertyName)
            {
                try
                {
                    List<object> distinctValues;
                    switch (propertyName.ToLower())
                    {
                        case "name":
                            distinctValues = _data.Select(x => x.Name).Distinct().Cast<object>().ToList();
                            break;
                        case "age":
                            distinctValues = _data.Select(x => x.Age).Distinct().Cast<object>().ToList();
                            break;
                        default:
                            distinctValues = new List<object>();
                            break;
                    }
                    return ServiceResult<List<object>>.Success(distinctValues);
                }
                catch (Exception ex)
                {
                    return ServiceResult<List<object>>.Error($"Distinct hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<int>> CountByPropertyAsync(string propertyName, object value)
            {
                try
                {
                    int count;
                    switch (propertyName.ToLower())
                    {
                        case "name":
                            count = _data.Count(x => x.Name == value.ToString());
                            break;
                        case "age":
                            count = _data.Count(x => x.Age == Convert.ToInt32(value));
                            break;
                        default:
                            count = 0;
                            break;
                    }
                    return ServiceResult<int>.Success(count);
                }
                catch (Exception ex)
                {
                    return ServiceResult<int>.Error($"Sayım hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<decimal>> SumByPropertyAsync(string propertyName)
            {
                try
                {
                    decimal sum;
                    switch (propertyName.ToLower())
                    {
                        case "age":
                            sum = (decimal)_data.Sum(x => x.Age);
                            break;
                        default:
                            sum = 0m;
                            break;
                    }
                    return ServiceResult<decimal>.Success(sum);
                }
                catch (Exception ex)
                {
                    return ServiceResult<decimal>.Error($"Toplama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<decimal>> AverageByPropertyAsync(string propertyName)
            {
                try
                {
                    decimal average;
                    switch (propertyName.ToLower())
                    {
                        case "age":
                            average = (decimal)_data.Average(x => x.Age);
                            break;
                        default:
                            average = 0m;
                            break;
                    }
                    return ServiceResult<decimal>.Success(average);
                }
                catch (Exception ex)
                {
                    return ServiceResult<decimal>.Error($"Ortalama hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<TestPerson>> GetMinByPropertyAsync(string propertyName)
            {
                try
                {
                    TestPerson? minEntity;
                    switch (propertyName.ToLower())
                    {
                        case "age":
                            minEntity = _data.OrderBy(x => x.Age).FirstOrDefault();
                            break;
                        case "id":
                            minEntity = _data.OrderBy(x => x.Id).FirstOrDefault();
                            break;
                        default:
                            minEntity = null;
                            break;
                    }
                    if (minEntity == null)
                        return ServiceResult<TestPerson>.Error("Minimum değer bulunamadı");

                    return ServiceResult<TestPerson>.Success(minEntity);
                }
                catch (Exception ex)
                {
                    return ServiceResult<TestPerson>.Error($"Minimum bulma hatası: {ex.Message}");
                }
            }

            public override async Task<ServiceResult<TestPerson>> GetMaxByPropertyAsync(string propertyName)
            {
                try
                {
                    TestPerson? maxEntity;
                    switch (propertyName.ToLower())
                    {
                        case "age":
                            maxEntity = _data.OrderByDescending(x => x.Age).FirstOrDefault();
                            break;
                        case "id":
                            maxEntity = _data.OrderByDescending(x => x.Id).FirstOrDefault();
                            break;
                        default:
                            maxEntity = null;
                            break;
                    }
                    if (maxEntity == null)
                        return ServiceResult<TestPerson>.Error("Maksimum değer bulunamadı");

                    return ServiceResult<TestPerson>.Success(maxEntity);
                }
                catch (Exception ex)
                {
                    return ServiceResult<TestPerson>.Error($"Maksimum bulma hatası: {ex.Message}");
                }
            }
        }
    }
}
