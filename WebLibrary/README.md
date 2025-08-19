# WebLibrary - .NET 9 Web Development Library

A comprehensive .NET 9 Class Library that provides generic methods, utilities, and operations for web development.

## Features

### 🔧 Core Components
- **Global Configuration**: Centralized configuration management
- **ServiceResult Model**: Generic result wrapper for service operations
- **HTTP Client Helper**: Generic HTTP client operations
- **Validation Helper**: Data validation utilities
- **Generic Extensions**: Extension methods for common operations
- **Generic Utilities**: General-purpose utility methods
- **Generic Service Base**: Abstract base class for CRUD operations

### 🔐 Security & Authentication
- **JWT Token Management**: Token generation, validation, and claim extraction
- **Password Security**: BCrypt hashing, verification, and strength checking
- **Encryption Utilities**: AES encryption, SHA hashing, HMAC, and secure random generation

### 📊 Data Processing
- **CSV Operations**: Import/export with CsvHelper
- **Excel Operations**: Read/write Excel files with EPPlus
- **Data Transformation**: JSON/Dictionary conversion, object copying, and comparison
- **Bulk Operations**: Batch processing with parallel support

### 🗄️ Database Operations
- **Generic Repository Pattern**: CRUD operations with Dapper
- **Query Builder**: Dynamic SQL query construction
- **Connection Management**: Database connection factory pattern
- **Unit of Work Pattern**: Transaction management
- **Database Helper**: Utility operations for database management

### 📝 Logging & Monitoring
- **Structured Logging**: Serilog integration with console and file sinks
- **Performance Monitoring**: Timer and statistics tracking
- **Health Checks**: Database, API, and disk usage monitoring
- **Metrics Collection**: Counters, gauges, and histograms

### 📁 File & Media Processing
- **Image Processing**: Resize, format conversion, rotation, cropping with SixLabors.ImageSharp
- **File Compression**: ZIP compression and decompression
- **Document Processing**: Text-based document operations
- **Media Conversion**: Base64, Hex, encoding, JSON/XML transformations

### 🤖 CAPTCHA & Anti-Bot
- **Google reCAPTCHA**: v2/v3 integration
- **Math CAPTCHA**: Customizable difficulty levels
- **CAPTCHA Manager**: Unified management system
- **Configurable Options**: Themes, languages, and expiration settings

### 🧪 Testing & Mocking Framework
- **Test Data Generation**: Bogus integration for fake data
- **Mocking Support**: Moq integration for test doubles
- **Assertion Helpers**: Custom assertion methods
- **Test Base Classes**: Common setup/teardown, performance measurement, database testing

## Project Structure

```
WebLibrary/
├── Global.cs                           # Global constants and configuration
├── Models/
│   └── ServiceResult.cs                # Generic result wrapper
├── Helpers/
│   ├── HttpClientHelper.cs             # HTTP client operations
│   └── ValidationHelper.cs             # Data validation
├── Extensions/
│   └── GenericExtensions.cs            # Extension methods
├── Utilities/
│   └── GenericUtilities.cs             # Utility methods
├── Services/
│   └── GenericServiceBase.cs           # Base service class
├── Security/
│   ├── JwtTokenHelper.cs               # JWT operations
│   ├── PasswordHelper.cs               # Password security
│   └── EncryptionHelper.cs             # Encryption utilities
├── DataProcessing/
│   ├── CsvDataHelper.cs                # CSV operations
│   ├── ExcelHelper.cs                  # Excel operations
│   ├── DataTransformationHelper.cs     # Data transformation
│   └── BulkOperationsHelper.cs         # Bulk operations
└── Database/
    ├── IDbConnectionFactory.cs         # Connection factory interface
    ├── SqlServerConnectionFactory.cs   # SQL Server connection factory
    ├── QueryBuilder.cs                 # SQL query builder
    ├── IGenericRepository.cs           # Repository interface
    ├── DapperGenericRepository.cs      # Dapper repository implementation
    ├── IUnitOfWork.cs                  # Unit of work interface
    ├── UnitOfWork.cs                   # Unit of work implementation
    └── DatabaseHelper.cs               # Database utilities
├── Logging/
    ├── StructuredLoggingHelper.cs      # Serilog structured logging
    ├── PerformanceMonitoringHelper.cs  # Performance tracking
    ├── HealthCheckHelper.cs            # Health checks
    └── MetricsCollectionHelper.cs      # Metrics collection
├── FileMedia/
    ├── ImageProcessingHelper.cs        # Image manipulation
    ├── FileCompressionHelper.cs        # File compression
    ├── DocumentProcessingHelper.cs     # Document operations
    └── MediaConversionHelper.cs        # Media conversion
├── Captcha/
    ├── ICaptchaProvider.cs             # CAPTCHA provider interface
    ├── GoogleRecaptchaProvider.cs      # Google reCAPTCHA
    ├── MathCaptchaProvider.cs          # Math CAPTCHA
    ├── CaptchaManager.cs               # CAPTCHA management
    └── CaptchaModels.cs                # CAPTCHA models
└── Testing/
    ├── TestDataGenerator.cs            # Fake data generation
    ├── MockHelper.cs                   # Mocking utilities
    ├── AssertionHelper.cs              # Custom assertions
    └── TestBase.cs                     # Test base classes

WebLibraryTest/                         # Test console application
└── Program.cs                          # Test implementation
```

## Installation

### NuGet Package
```bash
dotnet add package WebLibrary
```

### Manual Installation
1. Clone this repository
2. Build the solution
3. Reference the WebLibrary.dll in your project

## Quick Start

### Basic Usage
```csharp
using WebLibrary.Models;
using WebLibrary.Helpers;

// Use ServiceResult for operation results
var result = ServiceResult<string>.Success("Operation completed");

// HTTP operations
var httpResult = await HttpClientHelper.GetAsync<MyModel>("https://api.example.com/data");

// Validation
bool isValidEmail = ValidationHelper.IsValidEmail("test@example.com");
```

### Security Features
```csharp
using WebLibrary.Security;

// JWT Token operations
var token = JwtTokenHelper.GenerateToken(claims, secretKey);
var isValid = JwtTokenHelper.ValidateToken(token, secretKey);

// Password hashing
var hashedPassword = PasswordHelper.HashPassword("mypassword");
var isValidPassword = PasswordHelper.VerifyPassword("mypassword", hashedPassword);

// Encryption
var encrypted = EncryptionHelper.AesEncrypt("sensitive data", key, iv);
var decrypted = EncryptionHelper.AesDecrypt(encrypted, key, iv);
```

### Data Processing
```csharp
using WebLibrary.DataProcessing;

// CSV operations
var csvResult = CsvDataHelper.WriteToCsv(data, "output.csv");
var readResult = CsvDataHelper.ReadFromCsv<MyModel>("input.csv");

// Excel operations
var excelResult = ExcelDataHelper.WriteToExcel(data, "output.xlsx");
var excelReadResult = ExcelDataHelper.ReadFromExcel<MyModel>("input.xlsx");

// Data transformation
var jsonResult = DataTransformationHelper.ToJson(myObject);
var dictResult = DataTransformationHelper.ToDictionary(myObject);
```

### Database Operations
```csharp
using WebLibrary.Database;

// Connection factory
var connectionFactory = new SqlServerConnectionFactory(connectionString);

// Repository pattern
public class UserRepository : DapperGenericRepository<User>
{
    public UserRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory, "Users") { }
}

// Query builder
var query = new QueryBuilder()
    .Select("Id", "Name", "Email")
    .From("Users")
    .Where("Age > @MinAge")
    .OrderBy("Name")
```

### Logging & Monitoring
```csharp
using WebLibrary.Logging;

// Structured logging
StructuredLoggingHelper.ConfigureLogger(Serilog.Events.LogEventLevel.Information, "logs/app.log");
StructuredLoggingHelper.LogInformation("User logged in: {UserId}", userId);

// Performance monitoring
PerformanceMonitoringHelper.StartTimer("api-call");
var result = await apiService.CallAsync();
PerformanceMonitoringHelper.StopTimer("api-call");

// Health checks
HealthCheckHelper.RecordHealthCheck("database", HealthStatus.Healthy, "Connection OK");
var systemHealth = HealthCheckHelper.GetSystemHealthStatus();

// Metrics collection
MetricsCollectionHelper.IncrementCounter("api-requests", 1);
MetricsCollectionHelper.SetGauge("active-users", 150);
```

### File & Media Processing
```csharp
using WebLibrary.FileMedia;

// Image processing
ImageProcessingHelper.ResizeImage("input.jpg", "output.jpg", 800, 600);
ImageProcessingHelper.ConvertImageFormat("input.jpg", "output.png", ImageFormat.PNG);

// File compression
FileCompressionHelper.CompressDirectory("source-folder", "archive.zip");
FileCompressionHelper.ExtractZip("archive.zip", "extract-folder");

// Media conversion
var base64 = MediaConversionHelper.ConvertFileToBase64("file.txt");
var hex = MediaConversionHelper.ConvertFileToHex("file.txt");
```

### CAPTCHA & Anti-Bot
```csharp
using WebLibrary.Captcha;

// Math CAPTCHA
var mathCaptcha = new MathCaptchaProvider();
var captchaResult = mathCaptcha.GenerateCaptcha(new CaptchaOptions
{
    Type = CaptchaType.MathCaptcha,
    Difficulty = CaptchaDifficulty.Medium,
    Language = "tr"
});

// Google reCAPTCHA
var googleCaptcha = new GoogleRecaptchaProvider(new GoogleRecaptchaSettings
{
    SiteKey = "your-site-key",
    SecretKey = "your-secret-key"
});

// CAPTCHA Manager
var captchaManager = new CaptchaManager();
var result = captchaManager.GenerateCaptcha(CaptchaType.MathCaptcha, options);
```

### Testing & Mocking Framework
```csharp
using WebLibrary.Testing;

// Test data generation
var users = TestDataGenerator.GenerateUsers(10);
var products = TestDataGenerator.GenerateProducts(5);

// Mocking
var mockRepo = MockHelper.CreateRepositoryMock<User>(users);
var mockLogger = MockHelper.CreateLoggerMock<MyService>();

// Assertions
AssertionHelper.IsNotNull(users);
AssertionHelper.HasCount(users, 10);
AssertionHelper.Contains(users, users.First());

// Test base classes
public class MyTestClass : TestBase
{
    protected override void SetupTestData()
    {
        // Test setup
    }
}
    .AddParameter("MinAge", 18)
    .Build();

// Unit of Work
using var unitOfWork = new UnitOfWork(connectionFactory);
var userRepo = unitOfWork.GetRepository<User, UserRepository>();
await unitOfWork.CommitAsync();
```

## Dependencies

- .NET 9.0
- BCrypt.Net-Next 4.0.3
- CsvHelper 33.1.0
- Dapper 2.1.66
- EPPlus 8.1.0
- Microsoft.Data.SqlClient 6.1.1
- System.IdentityModel.Tokens.Jwt 8.14.0

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Updates

- **v1.0.0**: Initial release with core components
- **v1.1.0**: Added Security & Authentication features
- **v1.2.0**: Added Data Processing features
- **v1.3.0**: Added Database Operations with Dapper integration
- **v1.4.0**: Added Logging & Monitoring, File & Media Processing, CAPTCHA & Anti-Bot, and Testing & Mocking Framework

## Support

For issues and questions, please open an issue on GitHub.

## Veritabanı Kullanımı

### Program.cs'de Servisleri Kaydetme

```csharp
using WebLibrary.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// WebLibrary Database Services
builder.Services.AddScoped<IDbConnectionFactory>(provider => 
    new SqlServerConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TestDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Controller'da Kullanım

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var repository = _unitOfWork.GetRepository<User, int>();
        var result = await repository.GetAllAsync();
        
        if (result.IsSuccess)
            return Ok(result.Data);
        
        return BadRequest(result.ErrorMessages);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        var repository = _unitOfWork.GetRepository<User, int>();
        var result = await repository.AddAsync(user);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetAll), result.Data);
        
        return BadRequest(result.ErrorMessages);
    }
}
```

### Veritabanı Tablosu Oluşturma

```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
)
```
