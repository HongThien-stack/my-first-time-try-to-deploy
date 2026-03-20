# Docker Deployment Guide - Identity Service

## Overview
Hướng dẫn này giúp bạn triển khai Identity Service bằng Docker với SQL Server, kết nối được với cơ sở dữ liệu local của bạn.

## Yêu cầu tiên quyết

### Cài đặt bắt buộc:
1. **Docker Desktop** - https://www.docker.com/products/docker-desktop
2. **.NET 9.0 SDK** - https://dotnet.microsoft.com/download
3. **SQL Server** (tuỳ chọn - nếu muốn chạy trên local thay vì Docker)

### Kiểm tra cài đặt:
```powershell
docker --version
docker-compose --version
dotnet --version
```

## Cấu trúc File

```
OJT-Backend/
├── docker-compose.yml          # Orchestration cho containers
├── .env                         # Environment variables
├── .env.example                 # Template environment variables
├── IdentityService/
│   ├── Dockerfile              # Build image cho IdentityService
│   ├── appsettings.Docker.json  # Config cho Docker environment
│   ├── appsettings.json         # Config cho Development
│   └── src/
│       ├── IdentityService.API/
│       ├── IdentityService.Application/
│       ├── IdentityService.Domain/
│       └── IdentityService.Infrastructure/
```

## Cách 1: Chạy với Docker Compose (Khuyến nghị)

### Bước 1: Thiết lập Environment Variables

Sửa file `.env` trong thư mục root:

```bash
# Mật khẩu SQL Server (đặt mật khẩu mạnh)
DB_PASSWORD=YourStrongPassword123!

# Thay đổi JWT Secret Key (tối thiểu 32 ký tự)
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm

# SMTP Configuration
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your-email@gmail.com
SMTP_PASSWORD=your-app-password

# CORS Origins
CORS_ORIGIN_1=http://localhost:3000
CORS_ORIGIN_2=http://localhost:4200
CORS_ORIGIN_3=http://localhost:5173
```

### Bước 2: Xây dựng và Khởi chạy Containers

```powershell
# Di chuyển đến thư mục root OJT-Backend
cd c:\GAME\deploy\OJT-Backend

# Build Docker image
docker-compose build

# Khởi chạy các services
docker-compose up -d

# Kiểm tra logs của Identity Service
docker-compose logs -f identity-api

# Kiểm tra logs của SQL Server
docker-compose logs -f sqlserver
```

### Bước 3: Kiểm tra Health

```powershell
# Kiểm tra health check endpoint
curl http://localhost:8080/health

# Phải trả về JSON như sau:
# {"status":"Healthy","database":"Connected"}
```

### Bước 4: Truy cập API

- **Swagger UI**: http://localhost:8080/
- **Health Check**: http://localhost:8080/health
- **API Base URL**: http://localhost:8080/api

## Cách 2: Kết nối với SQL Server Local (Windows)

### Bước 1: Cấu hình Connection String

Nếu bạn muốn sử dụng SQL Server local (đã cài đặt trên máy), sửa `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "IdentityDB": "Server=localhost;Database=IdentityDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### Bước 2: Chạy migrations từ folder IdentityService

```powershell
cd c:\GAME\deploy\OJT-Backend\IdentityService

# Cạo migration (nếu có thay đổi schema)
dotnet ef migrations add InitialMigration -p src/IdentityService.Infrastructure -s src/IdentityService.API

# Áp dụng migration vào database
dotnet ef database update -p src/IdentityService.Infrastructure -s src/IdentityService.API
```

### Bước 3: Chạy ứng dụng

```powershell
cd src/IdentityService.API
dotnet run
```

## Cách 3: Sử dụng Docker nhưng kết nối SQL Server Local (Không khuyến nghị)

Nếu muốn Docker chạy Identity Service nhưng kết nối SQL Server local:

### Bước 1: Cấu hình docker-compose.yml

Sửa connection string trong `docker-compose.yml`:

```yaml
identity-api:
  environment:
    ConnectionStrings__IdentityDB: "Server=host.docker.internal,1433;Database=IdentityDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

**Lưu ý**: Sử dụng `host.docker.internal` thay vì `localhost` để Docker container kết nối với host

### Bước 2: Đảm bảo SQL Server cho phép remote connection

```sql
-- Chạy trên SQL Server Management Studio
sp_configure 'remote access', 1
RECONFIGURE
```

### Bước 3: Khởi chạy

```powershell
docker-compose build
docker-compose up -d
```

## Quản lý Docker Containers

### Xem danh sách containers đang chạy
```powershell
docker-compose ps
```

### Dừng containers
```powershell
docker-compose down
```

### Dừng và xóa volumes (xóa dữ liệu database)
```powershell
docker-compose down -v
```

### Xem logs
```powershell
# Xem tất cả logs
docker-compose logs

# Xem logs Identity Service
docker-compose logs identity-api

# Xem logs SQL Server
docker-compose logs sqlserver

# Theo dõi logs theo thời gian thực
docker-compose logs -f
```

### Truy cập shell container
```powershell
# Vào shell của Identity Service
docker-compose exec identity-api sh

# Vào shell của SQL Server
docker-compose exec sqlserver bash
```

## Kết nối SQL Server từ SSMS (SQL Server Management Studio)

**Khi chạy Docker Compose:**

1. Mở SQL Server Management Studio
2. Server name: `localhost,1433`
3. Authentication: SQL Server Authentication
4. Login: `sa`
5. Password: Giá trị của `DB_PASSWORD` trong `.env`
6. Click "Connect"

## Troubleshooting

### 1. Database không kết nối được

```powershell
# Kiểm tra health check
curl http://localhost:8080/health

# Kiểm tra logs
docker-compose logs identity-api
docker-compose logs sqlserver

# Kiểm tra connection
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
```

### 2. Container không khởi động được

```powershell
# Xóa containers và volumes cũ
docker-compose down -v

# Rebuild từ đầu
docker-compose build --no-cache

# Khởi chạy lại
docker-compose up -d
```

### 3. Port 1433 hoặc 8080 đã được sử dụng

```powershell
# Kiểm tra process sử dụng port
netstat -ano | findstr :1433
netstat -ano | findstr :8080

# Thay đổi port trong docker-compose.yml
# sqlserver: ports: - "1433:1433"  -> "1434:1433"
# identity-api: ports: - "8080:8080" -> "8081:8080"
```

### 4. Migration không chạy tự động

Nếu migration không chạy khi container khởi động:

```powershell
# Chạy migrations thủ công
docker-compose exec identity-api dotnet ef database update -p IdentityService.Infrastructure -s IdentityService.API

# Hoặc truy cập shell và chạy
docker-compose exec identity-api sh
# Trong shell:
cd src/IdentityService.API
dotnet ef database update -p ../IdentityService.Infrastructure
```

## Environment Variables Được Hỗ Trợ

| Variable | Mô tả | Giá trị mặc định |
|----------|-------|-----------------|
| `DB_PASSWORD` | Mật khẩu SA của SQL Server | `YourStrongPassword123!` |
| `JWT_SECRET_KEY` | Khóa bí mật JWT (tối thiểu 32 ký tự) | Giá trị dài |
| `JWT_ISSUER` | JWT Issuer | `IdentityService` |
| `JWT_AUDIENCE` | JWT Audience | `IdentityServiceClient` |
| `JWT_EXPIRY_MINUTES` | Thời gian hết hạn token (phút) | `60` |
| `JWT_REFRESH_TOKEN_EXPIRY_DAYS` | Thời gian hết hạn refresh token (ngày) | `7` |
| `SMTP_HOST` | SMTP Server | `smtp.gmail.com` |
| `SMTP_PORT` | SMTP Port | `587` |
| `SMTP_USER` | SMTP Username | Email của bạn |
| `SMTP_PASSWORD` | SMTP Password | App password |
| `CORS_ORIGIN_*` | CORS Origins được phép | `http://localhost:*` |

## API Endpoints Chính

```
POST   /api/auth/register           - Đăng ký tài khoản
POST   /api/auth/login              - Đăng nhập
POST   /api/auth/refresh-token      - Làm mới token
POST   /api/auth/change-password    - Thay đổi mật khẩu
GET    /api/users/{id}              - Lấy thông tin user
GET    /api/roles                   - Lấy danh sách roles
GET    /health                      - Health check endpoint
```

## Gợi ý Bảo mật

1. **Thay đổi tất cả mật khẩu mặc định** trước khi deploy lên production
2. **Sử dụng secrets management** cho sensitive data (AWS Secrets Manager, Azure Key Vault, etc.)
3. **Bật HTTPS** trong production
4. **Cấu hình firewall** để chỉ cho phép kết nối cần thiết
5. **Sử dụng environment-specific** configuration files

## Tiếp theo

Sau khi Identity Service chạy thành công, bạn có thể:
- Cấu hình các services khác (Product, Inventory, POS, Promotion)
- Thiết lập API Gateway
- Cấu hình logging tập trung
- Thiết lập CI/CD pipeline

---

**Last Updated**: March 20, 2026
