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

## Support

For issues and questions, please open an issue on GitHub.
