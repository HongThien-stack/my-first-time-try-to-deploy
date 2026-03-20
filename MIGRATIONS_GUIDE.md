# Database Migrations Guide

## Tổng Quan

Entity Framework Core Migrations giúp bạn:
- Quản lý thay đổi schema database
- Theo dõi lịch sử thay đổi
- Tự động cập nhật database
- Làm việc theo team với database schema

## Kiến Trúc Hiện Tại

```
IdentityService.Infrastructure/
├── Data/
│   └── IdentityDbContext.cs      # DbContext definition
├── Migrations/                    # Folder contains migrations
│   ├── 20260320000000_Initial.cs  # Example migration
│   └── IdentityDbContextModelSnapshot.cs
└── IdentityService.Infrastructure.csproj
```

## Migrations hiện có

### Initial Migration
- **Tạo**: Tables (`users`, `roles`, `user_login_logs`, `user_audit_logs`)
- **Indexes**: Email unique, phone index
- **Relationships**: User -> Role relationship

## Cách Sử Dụng

### 1. Tự động chạy (Khuyến nghị)

#### Docker
```powershell
# Migrations tự động chạy khi container khởi động
# (Cấu hình trong Program.cs)

# Xem logs để kiểm tra
.\docker-start.ps1 logs-api | grep -i migration
```

#### Local
```powershell
# Migrations tự động chạy khi app start
# (Cấu hình trong Program.cs)

.\local-start.ps1 run
# Xem logs để kiểm tra
```

### 2. Tạo Migration Mới

Khi bạn thay đổi database model:

```powershell
cd c:\GAME\deploy\OJT-Backend\IdentityService

# Cú pháp
dotnet ef migrations add <MigrationName> `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API

# Ví dụ: Thêm cột mới
dotnet ef migrations add AddPhoneNumberVerified `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API
```

**Output**:
```
A migration file: src/IdentityService.Infrastructure/Migrations/20260320_AddPhoneNumberVerified.cs
```

### 3. Áp dụng Migration

```powershell
# Cú pháp
dotnet ef database update `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API

# Hoặc sử dụng script
.\local-start.ps1 migrate
```

**Output**:
```
Applying migration '20260320_AddPhoneNumberVerified'.
Done.
```

## Quy Trình Thêm Cột Mới

### Step 1: Định nghĩa Model

Sửa `IdentityService.Domain/Entities/User.cs`:

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    // ... existing properties ...
    
    // Thêm property mới
    public string? PhoneNumberVerified { get; set; }
}
```

### Step 2: Cập nhật DbContext

Sửa `IdentityService.Infrastructure/Data/IdentityDbContext.cs`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>(entity =>
    {
        // ... existing configs ...
        
        // Thêm config cho property mới
        entity.Property(e => e.PhoneNumberVerified)
            .HasColumnName("phone_number_verified")
            .HasMaxLength(50);
    });
}
```

### Step 3: Tạo Migration

```powershell
dotnet ef migrations add AddPhoneNumberVerified `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API
```

### Step 4: Kiểm tra Migration File

Xem file `src/IdentityService.Infrastructure/Migrations/20260320_AddPhoneNumberVerified.cs`:

```csharp
public partial class AddPhoneNumberVerified : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "phone_number_verified",
            table: "users",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "phone_number_verified",
            table: "users");
    }
}
```

### Step 5: Áp dụng Migration

```powershell
# Docker
.\docker-start.ps1 restart
# Migrations tự động chạy

# Local
.\local-start.ps1 migrate
# Hoặc khởi động lại app
```

### Step 6: Kiểm tra

```sql
-- SSMS
USE IdentityDB;
EXEC sp_help 'users';
-- Kiểm tra column 'phone_number_verified' có tồn tại
```

## Các Lệnh Phổ Biến

### List Migrations
```powershell
dotnet ef migrations list `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API
```

**Output**:
```
20260320000000_Initial
20260320000001_AddPhoneNumberVerified
Done.
```

### Rollback to Previous Migration

```powershell
dotnet ef database update <PreviousMigrationName> `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API

# Ví dụ: Quay lại Initial
dotnet ef database update 20260320000000_Initial `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API
```

### Remove Last Migration

```powershell
# Nếu chưa apply
dotnet ef migrations remove `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API

# Nếu đã apply, phải rollback trước
dotnet ef database update <PreviousMigration>
dotnet ef migrations remove
```

### Generate SQL Script

```powershell
# Generate SQL cho tất cả migrations từ đầu
dotnet ef migrations script `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API `
    -o migrations.sql

# Generate SQL cho một migration cụ thể
dotnet ef migrations script <FromMigration> <ToMigration> `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API
```

## Best Practices

### 1. Naming Conventions

✅ **Good**:
- `AddUserPhoneNumber`
- `MakeEmailUnique`
- `RenamePasswordHashToPasswordEncrypted`

❌ **Bad**:
- `Update1`
- `Fix`
- `Changes`

### 2. Một Migration = Một Thay Đổi Logically

```powershell
# ✅ Tốt - Mỗi migration một mục đích
dotnet ef migrations add AddEmailVerifiedColumn
dotnet ef migrations add AddPhoneColumn

# ❌ Tệ - Quá nhiều thay đổi trong 1 migration
dotnet ef migrations add UpdateUserTable
```

### 3. Review Migration File

Luôn kiểm tra file migration trước khi apply:

```powershell
# Mở file và kiểm tra Up() và Down() methods
# Đảm bảo SQL logic đúng
# Kiểm tra tên column, types, constraints
```

### 4. Team Collaboration

```
Nếu có conflict migrations:
1. Rename migrations để tránh duplicate
2. Thường dùng timestamp prefix (auto)
3. Update DbContext snapshot file
4. Test merge kỹ lưỡng
```

### 5. Production Considerations

```powershell
# Trước khi deploy production:

# 1. Kiểm tra backup database
# 2. Test migration trên dev environment
# 3. Test migration trên staging environment
# 4. Kiểm tra Down() method (rollback plan)
# 5. Thông báo với team
# 6. Có plan B nếu xảy ra vấn đề
```

## Troubleshooting

### Issue: "No entities of type X are tracked"

**Nguyên nhân**: Entity chưa được add vào DbContext

**Giải pháp**:
```csharp
// Trong OnModelCreating
public DbSet<YourEntity> YourEntities { get; set; }
```

### Issue: "The migration 'X' has not been applied to the database"

**Giải pháp**:
```powershell
# Apply migrations
dotnet ef database update
```

### Issue: "Unable to resolve service for type"

**Nguyên nhân**: DbContext không được registered

**Giải pháp** (trong Program.cs):
```csharp
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### Issue: Migration file messed up

**Giải pháp**:
```powershell
# Remove bad migration (nếu chưa apply)
dotnet ef migrations remove

# Hoặc rollback
dotnet ef database update <PreviousMigration>
dotnet ef migrations remove
```

## Xem SQL được tạo

```powershell
# Xem SQL mà migrationtạo ra
dotnet ef migrations script 20260320000000_Initial 20260320000001_AddPhoneNumber

# Output: SQL statements
```

## Integration với Docker

Migrations tự động chạy khi container khởi động:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    dbContext.Database.Migrate();  // Tự động chạy pending migrations
}
```

## Kiểm tra Migration Status

```powershell
# List applied migrations
SELECT * FROM __EFMigrationsHistory

# Hoặc từ SSMS
USE IdentityDB
SELECT * FROM __EFMigrationsHistory
ORDER BY MigrationId DESC
```

## Tài liệu Tham Khảo

- [Entity Framework Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Migrations and Reverse Engineering](https://docs.microsoft.com/en-us/ef/core/backwards-compatibility)
- [Common Mistakes in EF Core](https://docs.microsoft.com/en-us/ef/core/modelconfiguration/)

---

**Tip**: Luôn test migrations trên dev environment trước khi dùng trên production!

---

Last Updated: March 20, 2026
