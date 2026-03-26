# ERDMCore - Enterprise MongoDB Shared Kernel

## 📋 Overview

**ERDMCore** is a production-ready, enterprise-grade shared kernel for building MongoDB-based microservices, specifically designed for credit management and financial systems. It provides a comprehensive foundation following Domain-Driven Design (DDD) principles with robust infrastructure, repository patterns, and best practices for MongoDB.

## 🎯 Key Features

### Core Capabilities
- ✅ **Domain-Driven Design (DDD)**: Rich domain models with aggregates, entities, and value objects
- ✅ **Repository Pattern**: Generic and type-safe repository implementation for MongoDB
- ✅ **Unit of Work**: Transaction management with MongoDB sessions
- ✅ **Audit Trail**: Automatic tracking of creation, modification, and soft delete
- ✅ **Domain Events**: Built-in event sourcing support
- ✅ **Soft Delete**: Non-destructive deletion with IsActive flag
- ✅ **Pagination**: Standardized pagination support for all queries
- ✅ **Health Checks**: Built-in MongoDB health monitoring

### MongoDB Optimizations
- ✅ **Connection Pooling**: Configurable connection pool settings
- ✅ **Read Preferences**: Support for Primary, Secondary, Nearest, and Preferred modes
- ✅ **Write Concerns**: Configurable write concerns (Majority, Acknowledged, Custom)
- ✅ **Tag Sets**: Support for MongoDB tag-based read preferences
- ✅ **Max Staleness**: Configurable replication lag tolerance
- ✅ **Retry Logic**: Automatic retry for reads and writes
- ✅ **SSL/TLS Support**: Secure connections with certificate validation

### Enterprise Features
- ✅ **Correlation IDs**: End-to-end request tracking
- ✅ **Structured Logging**: Built-in logging with correlation
- ✅ **Exception Handling**: Comprehensive exception hierarchy
- ✅ **Validation**: Guard clauses and input validation
- ✅ **DTO Support**: Data transfer objects with pagination and filtering
- ✅ **Bulk Operations**: Efficient batch operations

## 📦 Installation

### Using NuGet Package Manager
```bash
Install-Package ERDMCore
```

### Using .NET CLI
```bash
dotnet add package ERDMCore
```

### Package Reference
```xml
<PackageReference Include="ERDMCore" Version="1.0.0" />
```

## 🚀 Quick Start

### 1. Configure MongoDB in appsettings.json
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "credit_management",
    "CollectionPrefix": "credit",
    "MinPoolSize": 10,
    "MaxPoolSize": 100,
    "ConnectionTimeoutSeconds": 30,
    "SocketTimeoutSeconds": 30,
    "WriteConcern": "majority",
    "JournalEnabled": true,
    "ReadPreferenceMode": "Primary",
    "RetryWrites": true,
    "RetryReads": true
  }
}
```

### 2. Register ERDMCore in Program.cs
```csharp
using ERDMCore.Infrastructure.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add MongoDB infrastructure
builder.Services.AddMongoDB(builder.Configuration);

// Register your repositories
builder.Services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();

var app = builder.Build();

// Map health checks
app.MapHealthChecks("/health");

app.Run();
```

### 3. Create Your Entity
```csharp
using ERDMCore.Core.Entities;

public class CreditApplication : BaseEntity
{
    public string CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; }
    public int CreditScore { get; private set; }
    
    public CreditApplication(string customerId, decimal amount)
    {
        CustomerId = customerId;
        Amount = amount;
        Status = "Draft";
    }
    
    public void Submit()
    {
        if (Status != "Draft")
            throw new BusinessRuleException("Submit", "Only draft applications can be submitted");
            
        Status = "Submitted";
        AddDomainEvent(new CreditApplicationSubmittedEvent(this));
    }
    
    public void Approve(int creditScore)
    {
        if (Status != "UnderReview")
            throw new BusinessRuleException("Approve", "Application must be under review");
            
        CreditScore = creditScore;
        Status = "Approved";
        AddDomainEvent(new CreditApplicationApprovedEvent(this));
    }
}
```

### 4. Create Repository Interface
```csharp
using ERDMCore.Core.Interfaces;

public interface ICreditApplicationRepository : IRepository<CreditApplication>
{
    Task<IEnumerable<CreditApplication>> GetByCustomerIdAsync(string customerId);
    Task<IEnumerable<CreditApplication>> GetPendingApplicationsAsync();
    Task<decimal> GetTotalApprovedAmountAsync(string customerId);
}
```

### 5. Implement Repository
```csharp
using ERDMCore.Infrastructure.MongoDB.Infrastructure;
using MongoDB.Driver;

public class CreditApplicationRepository : MongoRepository<CreditApplication>, ICreditApplicationRepository
{
    public CreditApplicationRepository(
        IMongoDatabase database,
        IOptions<MongoDbSettings> settings,
        ILogger<CreditApplicationRepository> logger)
        : base(database, settings, logger)
    {
    }
    
    public async Task<IEnumerable<CreditApplication>> GetByCustomerIdAsync(string customerId)
    {
        var filter = Builders<CreditApplication>.Filter.Eq(x => x.CustomerId, customerId);
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<IEnumerable<CreditApplication>> GetPendingApplicationsAsync()
    {
        var filter = Builders<CreditApplication>.Filter.Eq(x => x.Status, "Pending");
        return await _collection.Find(filter).ToListAsync();
    }
    
    public async Task<decimal> GetTotalApprovedAmountAsync(string customerId)
    {
        var filter = Builders<CreditApplication>.Filter.And(
            Builders<CreditApplication>.Filter.Eq(x => x.CustomerId, customerId),
            Builders<CreditApplication>.Filter.Eq(x => x.Status, "Approved")
        );
        
        var sum = await _collection.Aggregate()
            .Match(filter)
            .Group(x => x.CustomerId, g => new { Total = g.Sum(x => x.Amount) })
            .FirstOrDefaultAsync();
            
        return sum?.Total ?? 0;
    }
}
```

### 6. Use Repository in Service
```csharp
public class CreditService
{
    private readonly ICreditApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreditService(ICreditApplicationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<CreditApplication> SubmitApplicationAsync(SubmitApplicationCommand command)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var application = new CreditApplication(command.CustomerId, command.Amount);
            application.Submit();
            
            await _repository.AddAsync(application);
            await _unitOfWork.CommitTransactionAsync();
            
            return application;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    
    public async Task<PaginatedResult<CreditApplication>> GetApplicationsAsync(int page, int pageSize)
    {
        return await _repository.GetPaginatedAsync(
            page, 
            pageSize, 
            x => x.IsActive,
            x => x.CreatedAt,
            sortDescending: true
        );
    }
}
```

## 🏗️ Architecture

### Project Structure
```
ERDMCore/
├── Core/                           # Domain layer
│   ├── Entities/                   # Base entity, value objects
│   ├── Interfaces/                 # Repository, Unit of Work contracts
│   ├── DTOs/                       # Data transfer objects
│   ├── Exceptions/                 # Domain exceptions
│   └── PaginatedResult.cs          # Pagination wrapper
│
├── Infrastructure.MongoDB/         # MongoDB infrastructure
│   ├── Settings/                   # MongoDB configuration
│   ├── Infrastructure/             # Repository implementations
│   └── Extensions/                 # Service registration
│
└── Middleware/                     # Cross-cutting concerns
    └── RequestLoggingMiddleware.cs # Request logging
```

### Design Patterns
- **Repository Pattern**: Abstraction over data access
- **Unit of Work**: Transaction management
- **Domain Events**: Event-driven architecture support
- **Value Objects**: Immutable domain concepts
- **Aggregate Root**: Consistency boundaries
- **Specification Pattern**: Query encapsulation

## ⚙️ Advanced Configuration

### Read Preference with Tag Sets
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://replica-set:27017",
    "ReadPreferenceMode": "SecondaryPreferred",
    "MaxStaleness": "00:00:90",
    "TagSets": [
      {
        "Tags": [
          { "Name": "dc", "Value": "east" },
          { "Name": "usage", "Value": "analytics" }
        ]
      }
    ]
  }
}
```

### Write Concern Configuration
```json
{
  "MongoDB": {
    "WriteConcern": "majority",  // Options: "majority", "1", "2", "3", "0"
    "JournalEnabled": true        // Wait for journal commit
  }
}
```

### Connection Pool Optimization
```json
{
  "MongoDB": {
    "MinPoolSize": 20,           // Minimum connections
    "MaxPoolSize": 200,          // Maximum connections
    "ConnectionTimeoutSeconds": 30,
    "SocketTimeoutSeconds": 60
  }
}
```

### SSL/TLS Configuration
```json
{
  "MongoDB": {
    "UseSsl": true,
    "AllowInsecureTls": false,   // For development only
    "ConnectionString": "mongodb://user:pass@host:27017/?ssl=true"
  }
}
```

## 📊 Features Matrix

| Feature | Description | Status |
|---------|-------------|--------|
| Basic CRUD | Create, Read, Update, Delete operations | ✅ |
| Soft Delete | Non-destructive deletion with IsActive | ✅ |
| Pagination | Skip/Take with total count | ✅ |
| Filtering | LINQ expression support | ✅ |
| Bulk Operations | InsertMany, UpdateMany, DeleteMany | ✅ |
| Transactions | Multi-document ACID transactions | ✅ |
| Domain Events | Event sourcing support | ✅ |
| Audit Trail | CreatedBy, ModifiedBy tracking | ✅ |
| Health Checks | MongoDB connection monitoring | ✅ |
| Connection Pooling | Configurable pool settings | ✅ |
| Read Preferences | Primary, Secondary, Nearest, etc. | ✅ |
| Write Concerns | Majority, Acknowledged, Custom | ✅ |
| Tag Sets | Server selection based on tags | ✅ |
| Max Staleness | Replication lag tolerance | ✅ |
| Retry Logic | Automatic operation retry | ✅ |
| SSL/TLS | Encrypted connections | ✅ |

## 🔧 Performance Tuning

### Connection Pool Sizing
```csharp
// For high-throughput applications
settings.MinPoolSize = 50;
settings.MaxPoolSize = 500;

// For low-latency requirements
settings.ConnectionTimeoutSeconds = 10;
settings.SocketTimeoutSeconds = 15;
```

### Read Preference Strategy
- **Primary**: Strong consistency (default)
- **Secondary**: Read scalability, eventual consistency
- **Nearest**: Lowest latency, geographic distribution
- **SecondaryPreferred**: Availability with consistency

### Write Concern Strategy
- **Majority**: Strong durability (default)
- **w=1**: Lower latency, risk of rollback
- **w=0**: Fastest, no acknowledgment

## 🧪 Testing

### Unit Testing Example
```csharp
[Fact]
public async Task AddAsync_ShouldInsertEntity()
{
    // Arrange
    var repository = new CreditApplicationRepository(database, settings, logger);
    var application = new CreditApplication("CUST-123", 5000);
    
    // Act
    var result = await repository.AddAsync(application);
    
    // Assert
    Assert.NotNull(result.Id);
    Assert.NotNull(result.CreatedAt);
    Assert.Equal("CUST-123", result.CustomerId);
}
```

### Integration Testing with Testcontainers
```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnEntity()
{
    // Arrange
    var mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:6.0")
        .Build();
        
    await mongoContainer.StartAsync();
    
    var settings = new MongoDbSettings 
    { 
        ConnectionString = mongoContainer.GetConnectionString(),
        DatabaseName = "test_db"
    };
    
    // Act & Assert
    // ... test implementation
}
```

## 🚦 Health Checks

```csharp
// Check health status
GET /health

// Response
{
    "status": "Healthy",
    "checks": [
        {
            "name": "mongodb",
            "status": "Healthy",
            "description": "MongoDB is healthy",
            "duration": "00:00:00.1234567"
        }
    ]
}
```

## 📝 Best Practices

### 1. **Entity Design**
```csharp
// ✅ Good - Rich domain model
public class CreditApplication : BaseEntity
{
    private List<Document> _documents = new();
    
    public void Submit() { /* business logic */ }
    public void Approve() { /* business logic */ }
}

// ❌ Bad - Anemic model
public class CreditApplication
{
    public string Status { get; set; }
    // No business logic
}
```

### 2. **Repository Usage**
```csharp
// ✅ Good - Use unit of work for multiple operations
await _unitOfWork.BeginTransactionAsync();
await _repository.AddAsync(entity1);
await _repository.AddAsync(entity2);
await _unitOfWork.CommitTransactionAsync();

// ❌ Bad - Each operation separate
await _repository.AddAsync(entity1);
await _repository.AddAsync(entity2);
```

### 3. **Query Optimization**
```csharp
// ✅ Good - Use indexes
var filter = Builders<T>.Filter.Eq(x => x.CustomerId, customerId);
await _collection.Find(filter).ToListAsync();

// ❌ Bad - No index support
await _collection.Find("{}").ToListAsync();
```

## 🔒 Security Considerations

- Always use SSL/TLS in production
- Store connection strings in secure vault
- Use principle of least privilege for database users
- Enable authentication and authorization
- Implement request validation
- Sanitize user inputs

## 📚 API Reference

### BaseEntity Properties
| Property | Type | Description |
|----------|------|-------------|
| Id | string | Unique identifier (ObjectId) |
| CreatedAt | DateTime? | Creation timestamp |
| CreatedBy | string | User who created |
| UpdatedAt | DateTime? | Last update timestamp |
| UpdatedBy | string | User who last updated |
| IsActive | bool | Soft delete flag |
| Version | int | Optimistic concurrency version |

### IRepository Methods
| Method | Description |
|--------|-------------|
| GetByIdAsync | Retrieve by ID |
| GetAllAsync | Retrieve all records |
| AddAsync | Insert single entity |
| UpdateAsync | Update existing entity |
| DeleteAsync | Soft delete entity |
| FindAsync | Query with LINQ |
| GetPaginatedAsync | Paginated results |
| ExecuteQueryAsync | Raw MongoDB query |

## 🐛 Troubleshooting

### Common Issues and Solutions

**Connection Timeout**
```json
// Increase timeout values
{
  "ConnectionTimeoutSeconds": 60,
  "SocketTimeoutSeconds": 120
}
```

**Write Concern Timeout**
```json
// Reduce write concern or increase timeout
{
  "WriteConcern": "1",  // Instead of "majority"
  "JournalEnabled": false  // For better performance
}
```

**Connection Pool Exhaustion**
```json
// Increase pool size
{
  "MinPoolSize": 50,
  "MaxPoolSize": 500
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- MongoDB .NET Driver team
- Contributors and maintainers
- Community feedback and support

## 📞 Support

- **GitHub Issues**: [https://github.com/yourcompany/ERDMCore/issues](https://github.com/yourcompany/ERDMCore/issues)
- **Documentation**: [https://docs.erdmcore.com](https://docs.erdmcore.com)
- **Examples**: [https://github.com/yourcompany/ERDMCore-Examples](https://github.com/yourcompany/ERDMCore-Examples)

---

**Version**: 1.0.0  
**Release Date**: March 2026  
**Compatible With**: .NET 8.0, MongoDB 6.0+  
**Maintainer**: ERDM Team
