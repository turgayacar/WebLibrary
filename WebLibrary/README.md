# WebLibrary

.NET 9.0 ile geliştirilmiş, generic method ve utility işlemleri içeren modern web kütüphanesi.

## 🚀 Özellikler

- **Generic ServiceResult**: Başarı ve hata durumlarını yöneten generic sınıf
- **HTTP Client Helper**: Generic HTTP istekleri için yardımcı sınıf
- **Validation Helper**: Veri doğrulama işlemleri için yardımcı metodlar
- **Generic Extensions**: Koleksiyonlar için extension metodları
- **Generic Utilities**: Nesne işlemleri için utility metodları
- **Generic Service Base**: CRUD işlemleri için temel servis sınıfı
- **Global Configuration**: Merkezi yapılandırma yönetimi
- **🔐 Security & Authentication**:
  - **JWT Token Helper**: JWT token oluşturma, doğrulama ve yönetim
  - **Password Helper**: BCrypt ile şifre hashleme, doğrulama ve güvenlik kontrolü
  - **Encryption Helper**: AES şifreleme, hash işlemleri ve güvenli rastgele değer üretimi
- **📊 Data Processing**:
  - **CSV Data Helper**: CSV import/export işlemleri
  - **Excel Data Helper**: Excel import/export işlemleri
  - **Data Transformation Helper**: Veri dönüştürme işlemleri
  - **Bulk Operations Helper**: Toplu işlemler
- **🗄️ Database Operations**:
  - **Generic Repository Pattern**: Dapper ile generic repository
  - **Query Builder**: SQL sorgularını dinamik olarak oluşturma
  - **Connection Management**: Veritabanı bağlantı yönetimi
  - **Unit of Work**: Transaction yönetimi

## 📦 Kurulum

```bash
dotnet add package WebLibrary
```

## 🏗️ Proje Yapısı

```
WebLibrary/
├── Global.cs                 # Global yapılandırma
├── Models/
│   └── ServiceResult.cs     # Generic servis sonuç sınıfı
├── Helpers/
│   ├── HttpClientHelper.cs  # HTTP client yardımcısı
│   └── ValidationHelper.cs  # Veri doğrulama yardımcısı
├── Extensions/
│   └── GenericExtensions.cs # Generic extension metodları
├── Utilities/
│   └── GenericUtilities.cs  # Generic utility metodları
├── Services/
│   └── GenericServiceBase.cs # Generic servis base sınıfı
├── Security/                 # 🔐 Güvenlik ve kimlik doğrulama
│   ├── JwtTokenHelper.cs    # JWT token işlemleri
│   ├── PasswordHelper.cs    # Şifre işlemleri
│   └── EncryptionHelper.cs  # Şifreleme işlemleri
├── DataProcessing/           # 📊 Veri işleme
│   ├── CsvDataHelper.cs     # CSV import/export işlemleri
│   ├── ExcelDataHelper.cs   # Excel import/export işlemleri
│   ├── DataTransformationHelper.cs # Veri dönüştürme işlemleri
│   └── BulkOperationsHelper.cs # Toplu işlemler
└── Database/                 # 🗄️ Veritabanı işlemleri
    ├── IDbConnectionFactory.cs       # Veritabanı bağlantı fabrikası interface'i
    ├── SqlServerConnectionFactory.cs # SQL Server bağlantı fabrikası
    ├── QueryBuilder.cs               # SQL sorgu oluşturucu
    ├── IGenericRepository.cs         # Generic repository interface'i
    ├── DapperGenericRepository.cs    # Dapper ile generic repository base class'ı
    ├── IUnitOfWork.cs                # Unit of Work interface'i
    ├── UnitOfWork.cs                 # Unit of Work base class'ı
    └── DatabaseHelper.cs             # Veritabanı yardımcı metodları
```

## 🔧 Kullanım Örnekleri

### Global Ayarlar

```csharp
using WebLibrary;

// Global ayarları kullan
Global.BaseApiUrl = "https://api.example.com";
Global.ApiTimeoutSeconds = 30;
Global.MaxRetryCount = 3;
```

### ServiceResult

```csharp
using WebLibrary.Models;

// Başarılı sonuç
var successResult = ServiceResult<string>.Success("Veri başarıyla alındı");

// Hata sonucu
var errorResult = ServiceResult<string>.Error("Bir hata oluştu");

// Sonucu kontrol et
if (successResult.IsSuccess)
{
    var data = successResult.Data;
}
else
{
    var errors = successResult.ErrorMessages;
}
```

### HTTP Client Helper

```csharp
using WebLibrary.Helpers;

// GET isteği
var result = await HttpClientHelper.GetAsync<User>("https://api.example.com/users/1");

// POST isteği
var newUser = new User { Name = "John", Email = "john@example.com" };
var postResult = await HttpClientHelper.PostAsync<User>("https://api.example.com/users", newUser);

// PUT isteği
var updateResult = await HttpClientHelper.PutAsync<User>("https://api.example.com/users/1", updatedUser);

// DELETE isteği
var deleteResult = await HttpClientHelper.DeleteAsync("https://api.example.com/users/1");
```

### Validation Helper

```csharp
using WebLibrary.Helpers;

// Email doğrulama
bool isValidEmail = ValidationHelper.IsValidEmail("test@example.com");

// TC Kimlik doğrulama
bool isValidTc = ValidationHelper.IsValidTcKimlik("12345678901");

// Telefon doğrulama
bool isValidPhone = ValidationHelper.IsValidPhone("05551234567");

// Şifre güvenliği
bool isStrongPassword = ValidationHelper.IsStrongPassword("Test123!@#");

// Model doğrulama
var validationResult = ValidationHelper.ValidateModel(userModel);
```

### Security & Authentication

```csharp
using WebLibrary.Security;

// JWT Token işlemleri
var secretKey = "your-secret-key";
var claims = new List<Claim>
{
    new Claim("UserId", "123"),
    new Claim("Username", "john"),
    new Claim("Role", "Admin")
};

// Token oluştur
var tokenResult = JwtTokenHelper.GenerateToken(claims, secretKey);
if (tokenResult.IsSuccess)
{
    var token = tokenResult.Data;
    
    // Token doğrula
    var isValid = JwtTokenHelper.ValidateToken(token, secretKey);
    
    // Claim'leri çıkar
    var extractedClaims = JwtTokenHelper.ExtractClaims(token, secretKey);
    var userId = JwtTokenHelper.ExtractUserId(token, secretKey);
}

// Şifre işlemleri
var password = "MySecurePassword123!";

// Şifre hashle
var hashedPassword = PasswordHelper.HashPassword(password);

// Şifre doğrula
var isValidPassword = PasswordHelper.VerifyPassword(password, hashedPassword);

// Şifre güvenliği kontrol et
var strengthResult = PasswordHelper.CheckPasswordStrength(password);
Console.WriteLine($"Güvenlik skoru: {strengthResult.Data?.Score}/7");

// Güçlü şifre oluştur
var strongPassword = PasswordHelper.GenerateStrongPassword(16, true);

// Şifreleme işlemleri
var plainText = "Gizli veri";

// AES anahtar çifti oluştur
var keyPair = EncryptionHelper.GenerateStringKeyPair();

// Veri şifrele
var encrypted = EncryptionHelper.EncryptAesString(plainText, keyPair.Data.Key, keyPair.Data.IV);

// Veri çöz
var decrypted = EncryptionHelper.DecryptAesString(encrypted.Data, keyPair.Data.Key, keyPair.Data.IV);

// Hash işlemleri
var sha256Hash = EncryptionHelper.GenerateSha256Hash(plainText);
var sha512Hash = EncryptionHelper.GenerateSha512Hash(plainText);

// Güvenli rastgele değerler
var randomNumber = EncryptionHelper.GenerateSecureRandomNumber(1, 100);
var randomString = EncryptionHelper.GenerateSecureRandomString(20, true);

// HMAC oluştur ve doğrula
var hmac = EncryptionHelper.GenerateHmac(plainText, "secret-key");
var isValidHmac = EncryptionHelper.VerifyHmac(plainText, "secret-key", hmac.Data);
```

### Generic Extensions

```csharp
using WebLibrary.Extensions;

var users = new List<User>();

// Null/boş kontrol
if (users.IsNotNullOrEmpty())
{
    // Sayfalama
    var page1 = users.GetPage(1, 10);
    var totalPages = users.GetTotalPages(10);
    
    // Property bazlı sıralama
    var orderedByName = users.OrderByProperty("Name", true);
    
    // Property bazlı filtreleme
    var filteredByAge = users.WherePropertyEquals("Age", 25);
    
    // Property bazlı gruplama
    var groupedByCity = users.GroupByProperty("City");
}
```

### Generic Utilities

```csharp
using WebLibrary.Utilities;

var user = new User();

// Property değeri alma
var name = GenericUtilities.GetPropertyValue<string>(user, "Name");

// Property değeri ayarlama
GenericUtilities.SetPropertyValue(user, "Age", 25);

// Dictionary'den nesne oluşturma
var properties = new Dictionary<string, object?>
{
    { "Name", "John" },
    { "Email", "john@example.com" }
};
var newUser = GenericUtilities.CreateFromDictionary<User>(properties);

// Property kopyalama
var copiedUser = GenericUtilities.CopyProperties<User>(user, "Name", "Email");
```

### Generic Service Base

```csharp
using WebLibrary.Services;

public class UserService : GenericServiceBase<User>
{
    public override async Task<ServiceResult<List<User>>> GetAllAsync()
    {
        // Implementasyon
        return ServiceResult<List<User>>.Success(users);
    }
    
    public override async Task<ServiceResult<User>> GetByIdAsync(int id)
    {
        // Implementasyon
        var user = users.FirstOrDefault(u => u.Id == id);
        return user != null 
            ? ServiceResult<User>.Success(user)
            : ServiceResult<User>.Error("Kullanıcı bulunamadı");
    }
    
    // Diğer metodların implementasyonu...
}
```

### Database Operations

```csharp
using WebLibrary.Database;

// Connection Factory
var connectionFactory = new SqlServerConnectionFactory("your-connection-string");

// Query Builder
var query = new QueryBuilder()
    .Select("Id, Name, Email")
    .From("Users")
    .Where("Age > @Age", "Age", 18)
    .OrderBy("Name", "ASC")
    .Limit(10);

var sql = query.Build();
var parameters = query.GetParameters();

// Generic Repository
public class UserRepository : DapperGenericRepository<User, int>
{
    public UserRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory, "Users", "Id")
    {
    }
}

var userRepo = new UserRepository(connectionFactory);

// CRUD işlemleri
var users = await userRepo.GetAllAsync();
var user = await userRepo.GetByIdAsync(1);
var newUser = await userRepo.AddAsync(user);
var updated = await userRepo.UpdateAsync(user);
var deleted = await userRepo.DeleteAsync(1);

// Unit of Work
using var unitOfWork = new UnitOfWork(connectionFactory);
var userRepo = unitOfWork.GetRepository<User, int>();

try
{
    unitOfWork.BeginTransaction();
    
    // Birden fazla işlem
    await userRepo.AddAsync(user1);
    await userRepo.AddAsync(user2);
    
    unitOfWork.Commit();
}
catch
{
    unitOfWork.Rollback();
    throw;
}

// Database Helper
var isValid = DatabaseHelper.ValidateConnectionString(connectionString);
var isConnected = DatabaseHelper.TestConnection(connectionString);
var dbInfo = DatabaseHelper.GetDatabaseInfo(connectionString);
var tables = DatabaseHelper.GetTables(connectionString);
var columns = DatabaseHelper.GetTableStructure(connectionString, "Users");

// Özel SQL sorgusu
var results = DatabaseHelper.ExecuteQuery(connectionString, 
    "SELECT * FROM Users WHERE Age > @Age", 
    new Dictionary<string, object> { { "Age", 18 } });

// Backup
var backupResult = DatabaseHelper.CreateBackup(connectionString, @"C:\backup.bak");
```

## 🧪 Test

Test projesini çalıştırmak için:

```bash
cd WebLibraryTest
dotnet run
```

## 📋 Gereksinimler

- .NET 9.0
- Microsoft.AspNetCore.Http
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging

## 🔄 Güncellemeler

### v1.3.0
- Database Operations eklendi
  - Generic Repository Pattern (Dapper ile)
  - Query Builder
  - Connection Management
  - Unit of Work Pattern
  - Database Helper

### v1.2.0
- Data Processing özellikleri eklendi
  - CSV import/export
  - Excel import/export
  - Data Transformation
  - Bulk Operations

### v1.1.0
- Security & Authentication özellikleri eklendi
  - JWT Token Helper
  - Password Helper (BCrypt)
  - Encryption Helper

### v1.0.0
- İlk sürüm
- Generic ServiceResult sınıfı
- HTTP Client Helper
- Validation Helper
- Generic Extensions
- Generic Utilities
- Generic Service Base

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📞 İletişim

Proje ile ilgili sorularınız için issue açabilirsiniz.
