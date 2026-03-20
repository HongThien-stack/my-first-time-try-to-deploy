# Identity Service - Quick Start Guide

## 🚀 Cách 1: Docker Compose (Khuyến nghị)

### Yêu cầu
- Docker Desktop

### Khởi chạy (1 lệnh)
```powershell
cd c:\GAME\deploy\OJT-Backend
.\docker-start.ps1 up
```

### Kết quả
- **Swagger UI**: http://localhost:8080
- **Health Check**: http://localhost:8080/health
- **SQL Server**: localhost,1433 (user: sa)

### Quản lý
```powershell
.\docker-start.ps1 ps          # Xem containers
.\docker-start.ps1 logs-api    # Xem logs
.\docker-start.ps1 down        # Dừng services
```

**Để kết nối SQL Server:**
- Server: `localhost,1433`
- Login: `sa`
- Password: Xem file `.env` (biến `DB_PASSWORD`)

---

## 🖥️ Cách 2: Local Development (Windows)

### Yêu cầu
- .NET 9.0 SDK
- SQL Server (bất kỳ version)

### Setup lần đầu (1 lệnh)
```powershell
cd c:\GAME\deploy\OJT-Backend
.\local-start.ps1 setup
```

### Khởi chạy
```powershell
.\local-start.ps1 run
```

### Kết quả
- **Swagger UI**: http://localhost:5000
- **Health Check**: http://localhost:5000/health

### Quản lý
```powershell
.\local-start.ps1 migrate      # Chạy migrations
.\local-start.ps1 test-api     # Kiểm tra API
.\local-start.ps1 clean        # Xóa build artifacts
```

---

## 📋 So sánh hai cách

| Tiêu chí | Docker | Local |
|----------|--------|-------|
| Cài đặt | Docker Desktop | .NET SDK + SQL Server |
| Khởi chạy | ⚡ Nhanh (~30s) | ⚡ Nhanh (~10s) |
| Độ phức tạp | Rất đơn giản | Trung bình |
| Môi trường | Chuẩn (Dev/Prod giống) | Có thể khác |
| Debug | Khó hơn | Dễ hơn |
| Database | Trong container | Local SQL Server |
| Port | 8080 | 5000 |
| **Khuyến nghị** | ✅ Dùng đầu tiên | ✅ Dùng sau |

---

## ⚙️ Cấu hình Cơ sở dữ liệu

### Docker (Tự động)
Database được tạo và migration chạy tự động khi container khởi động.

### Local (Thủ công)
1. Sửa connection string trong `IdentityService/appsettings.json`:
```json
"ConnectionStrings": {
  "IdentityDB": "Server=YOUR_SERVER;Database=IdentityDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

2. Chạy migration:
```powershell
.\local-start.ps1 migrate
```

---

## 🔍 Kiểm tra Kết nối

### Docker
```powershell
# Kiểm tra health
curl http://localhost:8080/health
# Kết quả: {"status":"Healthy","database":"Connected"}
```

### Local
```powershell
.\local-start.ps1 test-api
# Hoặc visit: http://localhost:5000/health
```

---

## 📝 Thay đổi Cấu hình

### Docker
- Sửa file `.env` rồi khởi động lại:
```powershell
.\docker-start.ps1 down
.\docker-start.ps1 up
```

### Local
- Sửa file `IdentityService/appsettings.json`
- Khởi động lại service:
```powershell
.\local-start.ps1 run
```

---

## 🐛 Xử lý Sự cố

### API không phản hồi

**Docker:**
```powershell
.\docker-start.ps1 logs-api
```

**Local:**
```powershell
# Kiểm tra service có đang chạy
Get-Process dotnet | Where-Object {$_.Name -eq "dotnet"}

# Hoặc xem logs từ console
.\local-start.ps1 run
```

### Database không kết nối

**Docker:**
```powershell
# Kiểm tra SQL Server container
.\docker-start.ps1 logs-db

# Kiểm tra connection
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
```

**Local:**
```powershell
# Kiểm tra SQL Server đang chạy
# SQL Server (MSSQLSERVER)        Running

# Kiểm tra connection string
# Xem file IdentityService/appsettings.json
```

### Port đã bị sử dụng

**Docker:**
```powershell
# Tìm process sử dụng port
netstat -ano | findstr :8080

# Kill process (thay PID bằng giá trị thực tế)
taskkill /PID 1234 /F

# Hoặc sửa port trong docker-compose.yml
```

**Local:**
```powershell
# Tìm process sử dụng port
netstat -ano | findstr :5000

# Kill process (thay PID bằng giá trị thực tế)
taskkill /PID 1234 /F
```

---

## 📚 Tài liệu Chi tiết

- **[DOCKER_SETUP_GUIDE.md](DOCKER_SETUP_GUIDE.md)** - Hướng dẫn Docker toàn diện
- **[DatabaseSchemas/README.md](DatabaseSchemas/README.md)** - Schema Database
- **[README.md](README.md)** - Tổng quan dự án

---

## 🔑 Environment Variables

### Database
- `DB_PASSWORD`: Mật khẩu SQL Server SA (Docker)

### JWT
- `JWT_SECRET_KEY`: Khóa bí mật (tối thiểu 32 ký tự)
- `JWT_ISSUER`: Issuer token
- `JWT_AUDIENCE`: Audience token
- `JWT_EXPIRY_MINUTES`: Thời gian hết hạn (phút)
- `JWT_REFRESH_TOKEN_EXPIRY_DAYS`: Thời gian refresh token (ngày)

### SMTP (Email)
- `SMTP_HOST`: SMTP server
- `SMTP_PORT`: SMTP port
- `SMTP_USER`: Email để gửi
- `SMTP_PASSWORD`: Password/App password

### CORS
- `CORS_ORIGIN_1`, `CORS_ORIGIN_2`, etc: Frontend URLs

---

## 💡 Tips

1. **Đầu tiên, sử dụng Docker** - Dễ nhất, chuẩn nhất
2. **Debug? Sử dụng Local** - Dễ debug hơn với VS Code/Visual Studio
3. **Thay đổi code? Cần restart:**
   - Docker: `.\docker-start.ps1 restart`
   - Local: Nhấn Ctrl+C rồi `.\local-start.ps1 run`
4. **Xem logs trong Docker:** `.\docker-start.ps1 logs`
5. **Reset database trong Docker:** `.\docker-start.ps1 clean`

---

## 🎯 Tiếp Theo

Sau khi Identity Service hoạt động:

1. ✅ **Kiểm tra API:**
   - Truy cập Swagger UI
   - Test các endpoint
   - Kiểm tra database

2. ➡️ **Cấu hình các services khác:**
   - Product Service
   - Inventory Service
   - POS Service
   - Promotion Service

3. 🔗 **Kết nối services:**
   - Thiết lập API Gateway
   - Cấu hình service-to-service communication
   - Thiết lập logging tập trung

---

**Questions or Issues?** Xem [DOCKER_SETUP_GUIDE.md](DOCKER_SETUP_GUIDE.md) để giải pháp chi tiết.

Last Updated: March 20, 2026
