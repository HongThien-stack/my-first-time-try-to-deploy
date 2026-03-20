# Docker & Database Connection Troubleshooting Guide

## 🔍 Diagnosis Workflow

```
Issue Occurs
    ↓
Run Health Check: curl http://localhost:8080/health
    ↓
Check Container Status: docker-compose ps
    ↓
Check Logs: docker-compose logs
    ↓
Apply Fix (see relevant section below)
```

---

## 🚨 Common Issues & Solutions

### 1. "Connection refused" / Port không phản hồi

#### Triệu tượng
```
curl : (7) Failed to connect to localhost port 8080: Connection refused
```

#### Nguyên nhân có thể
- [ ] Container chưa khởi động
- [ ] API crash
- [ ] Port sai
- [ ] Firewall chặn

#### Giải pháp
```powershell
# Step 1: Kiểm tra container đang chạy
docker-compose ps

# Kết quả mong đợi:
# NAME                STATUS
# identity-sqlserver  Up (healthy)
# identity-api        Up (healthy)

# Step 2: Nếu không running, khởi chạy
.\docker-start.ps1 up

# Step 3: Nếu vẫn lỗi, xem logs
docker-compose logs identity-api

# Step 4: Kiểm tra port không bị sử dụng
netstat -ano | findstr :8080

# Step 5: Nếu port bị dùng, kill process
taskkill /PID 1234 /F  # Thay 1234 bằng PID thực tế

# Step 6: Khởi chạy lại
.\docker-start.ps1 restart
```

---

### 2. Database Connection Error

#### Triệu tượng
```
Health Status: Unhealthy
Error: Cannot open database "IdentityDB" requested by the login...
Cannot connect to server=sqlserver...
```

#### Nguyên nhân
- [ ] SQL Server container không healthy
- [ ] Connection string sai
- [ ] Database chưa được tạo
- [ ] Credential sai

#### Giải pháp

**Check 1: SQL Server container status**
```powershell
# Xem status
docker-compose ps sqlserver

# Logs
.\docker-start.ps1 logs-db

# Expected: Mặt sau ~40 giây, database sẵn sàng
```

**Check 2: Connection string**
```powershell
# Xem logs API để kiểm tra connection string
docker-compose logs identity-api | grep -i connection

# Docker sử dụng tên service làm hostname
# ✅ Correct: Server=sqlserver,1433
# ❌ Wrong: Server=localhost,1433
```

**Check 3: Database credentials**
```powershell
# Kiểm tra .env file
cat .env | findstr DB_PASSWORD

# Default: DB_PASSWORD=YourStrongPassword123!

# Test kết nối trực tiếp
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost `
    -U sa `
    -P "DB_PASSWORD_VALUE_HERE" `
    -Q "SELECT @@VERSION"
```

**Check 4: Database tồn tại**
```powershell
# Vào shell SQL Server
docker-compose exec sqlserver bash

# Trong bash:
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT name FROM sys.databases WHERE name LIKE 'Identity%'"

# Expected: IdentityDB
```

**Check 5: Tables tồn tại**
```powershell
# Kiểm tra tables
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost `
    -U sa `
    -P "YourPassword" `
    -d IdentityDB `
    -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"

# Expected:
# roles
# user_login_logs
# user_audit_logs
# users
```

**Fix: Reset từ đầu**
```powershell
# Dừng và xóa mọi thứ
.\docker-start.ps1 clean

# Kiểm tra .env file, cấu hình lại
# Update .env nếu cần

# Khởi chạy lại
.\docker-start.ps1 up

# Chờ ~40 giây rồi kiểm tra
.\docker-start.ps1 health
```

---

### 3. Migrations không chạy tự động

#### Triệu tượng
```
API chạy nhưng:
- Database rỗng
- Không có tables
- Health check fails: "Invalid object name 'users'"
```

#### Nguyên nhân
- [ ] Database connection successful nhưng migration bị bỏ qua
- [ ] Lỗi trong migration code
- [ ] Code quá cũ

#### Giải pháp

**Check 1: Log migration output**
```powershell
docker-compose logs identity-api | grep -i migrat

# Expected:
# Checking database migrations...
# Database migrations applied successfully
```

**Check 2: Chạy migrations thủ công**
```powershell
# Cách 1: Vào command trong container
docker-compose exec identity-api sh
cd src/IdentityService.API
dotnet ef database update

# Cách 2: Từ ngoài
docker-compose exec identity-api dotnet ef database update `
    -p IdentityService.Infrastructure `
    -s IdentityService.API
```

**Check 3: Kiểm tra các migrations available**
```powershell
docker-compose exec identity-api dotnet ef migrations list `
    -p IdentityService.Infrastructure `
    -s IdentityService.API

# Expected: Thấy "Initial" hoặc migrations khác
```

**Fix: Force apply migrations**
```powershell
# Vào container shell
docker-compose exec identity-api sh

# Chạy lệnh update
dotnet ef database update `
    -p src/IdentityService.Infrastructure `
    -s src/IdentityService.API --verbose

# Kiểm tra logs chi tiết
```

---

### 4. API crashes sau khởi động

#### Triệu tượng
```
Container started rồi exited
docker-compose logs identity-api shows errors
```

#### Nguyên nhân phổ biến
- [ ] Database connection fails
- [ ] Missing configuration
- [ ] Invalid appsettings
- [ ] Port conflict
- [ ] Environment variable sai

#### Giải pháp
```powershell
# Step 1: Xem full logs
docker-compose logs identity-api

# Tìm dòng error (thường in đỏ)
# Một số lỗi phổ biến:
# - "InvalidOperationException: JWT SecretKey not configured"
# - "Cannot connect to server=sqlserver"
# - "No connection string"

# Step 2: Fix dựa vào error message
# Ví dụ:
#   - Kiểm tra .env có JWT_SECRET_KEY
#   - Kiểm tra SQL Server healthy
#   - Kiểm tra appsettings.Docker.json valid

# Step 3: Rebuild
docker-compose up --build -d

# Step 4: Kiểm tra lại
docker-compose logs identity-api
```

---

### 5. Port 1433 hoặc 8080 đã sử dụng

#### Triệu tượng
```
docker-compose up
Error response from daemon: bind: address already in use
```

#### Giải pháp

**Cách 1: Shutdown process hiện tại**
```powershell
# Kiểm tra process
netstat -ano | findstr :1433      # SQL Server
netstat -ano | findstr :8080      # API

# Kết quả:
# TCP    0.0.0.0:1433      LISTENING    1234

# Kill process
taskkill /PID 1234 /F

# Khởi chạy lại
.\docker-start.ps1 up
```

**Cách 2: Sử dụng cổng khác**

Sửa `docker-compose.yml`:
```yaml
sqlserver:
  ports:
    - "1434:1433"      # External port 1434 -> containerport 1433

identity-api:
  ports:
    - "8081:8080"      # External port 8081 -> container port 8080
```

Sau đó:
```powershell
# Truy cập API: http://localhost:8081 (thay vì 8080)
# Kết nối SQL Server: localhost,1434 (thay vì 1433)

docker-compose up -d
```

---

### 6. "healthcheck failed: x retries exceeded"

#### Triệu tượng
```
Container exits sau vài lần retry
```

#### Nguyên nhân
- [ ] Database startup quá chậm
- [ ] Connection string sai
- [ ] SQL Server không able

#### Giải pháp
```powershell
# Tăng start-period timeout trong docker-compose.yml
# Hiện tại: start-period=40s
# Thay thành: start-period=60s  (hoặc 90s)

# docker-compose.yml
sqlserver:
  healthcheck:
    start_period: 60s   # Tăng từ 40s

identity-api:
  depends_on:
    sqlserver:
      condition: service_healthy

# Rebuild
docker-compose up --build -d

# Kiểm tra logs
docker-compose logs sqlserver
```

---

### 7. Cannot connect from SSMS

#### Triệu tượng
```
SSMS không kết nối được SQL Server trong Docker
Login failed, login timeout, connection refused
```

#### Giải pháp

**Check 1: Container running**
```powershell
docker-compose ps

# Expected: sqlserver - Up (healthy)
```

**Check 2: Port mapping**
```powershell
# Kiểm tra port forward
docker-compose ps sqlserver

# Hoặc:
docker port identity-sqlserver

# Expected: 1433/tcp -> 0.0.0.0:1433
```

**Check 3: Server name syntax**
```
SSMS Server name: localhost,1433
   (NOT just "localhost")
   (NOT "localhost:1433")
   (Dùng dấu phẩy, NOT colon)

Authentication: SQL Server Authentication
Login: sa
Password: (từ .env DB_PASSWORD)
```

**Check 4: Network access**
```powershell
# Kiểm tra SQL Server đang listen
docker-compose exec sqlserver netstat -tuln | grep 1433

# Expected: 0.0.0.0:1433 (LISTEN)

# Kiểm tra firewall
# Windows Defender > Firewall > Allow an app
# Đảm bảo Docker có quyền
```

**Check 5: TrustServerCertificate**
```mssql
-- SSMS connection options:
-- Options > Connection Properties
-- Encrypt connection: Disable (hoặc "No")
-- Trust server certificate: Yes
```

---

### 8. Out of memory / Container killed

#### Triệu tượng
```
Container bị kill for being OOM killer
Or very slow performance
```

#### Cause
- [ ] Docker memory allocation không đủ
- [ ] SQL Server sử dụng quá nhiều memory

#### Solution

**Check current memory**
```powershell
# Windows Task Manager
# Docker Desktop > check memory usage

# Hoặc từ command line
docker stats

# Kết quả: xem %MEM của containers
```

**Increase Docker memory**
```
Docker Desktop > Settings > Resources > Memory
Tăng từ default (thường 2GB) lên 4-8GB

Apply & Restart
```

**Limit SQL Server memory (nếu cần)**
```yaml
# docker-compose.yml
sqlserver:
  environment:
    MSSQL_MEMORY_LIMIT_MB: 2048    # Limit 2GB (từ default 3.25GB)
  deploy:
    resources:
      limits:
        memory: 3G
      reservations:
        memory: 2G
```

---

### 9. "The term 'docker-compose' is not recognized"

#### Nguyên nhân
- [ ] Docker Desktop chưa cài
- [ ] PowerShell chưa restart sau cài
- [ ] Path chưa update

#### Giải pháp
```powershell
# Check Docker installed
docker --version

# Kết quả: Docker version x.x.x

# Kiểm tra docker-compose
docker-compose --version

# Nếu không có:
# 1. Tải Docker Desktop từ https://www.docker.com/products/docker-desktop
# 2. Install
# 3. Restart PowerShell (mở mới)
# 4. Test lại
```

---

### 10. Logs không hiển thị hoặc quá nhiều logs

#### Quá ít logs
```powershell
# Thêm verbose
docker-compose logs --verbose
docker-compose logs -f identity-api
```

#### Quá nhiều logs
```powershell
# Limit logs
docker-compose logs --tail=50 identity-api

# Hoặc filter
docker-compose logs identity-api | Select-String "error"
```

---

## 🔧 Advanced Debugging

### Truy cập Container Shell

```powershell
# SQL Server
docker-compose exec sqlserver bash

# Identity API
docker-compose exec identity-api sh
```

### Kiểm tra Environment Variables

```powershell
# Trong container
docker-compose exec identity-api sh
env | grep -i jwt
env | grep -i connection
env | grep -i smtp
```

### Inspect Image

```powershell
# Xem image layers
docker image inspect IdentityService:latest

# Xem command chạy
docker inspect identity-api | grep -A 10 "Cmd"
```

### Network Debug

```powershell
# Kiểm tra network
docker network ls
docker network inspect ojtbackend_identity-network

# Ping giữa containers
docker-compose exec identity-api ping sqlserver

# Expected: Replying from 172.x.x.x
```

---

## 🛠️ Utilities & Tools

### Quick Health Check Script

```powershell
# Save as health-check.ps1
Write-Host "=== Docker Health Check ===" -ForegroundColor Cyan

# Check running
Write-Host "`nContainer Status:" -ForegroundColor Green
docker-compose ps

# Check API health
Write-Host "`nAPI Health:" -ForegroundColor Green
$health = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing -ErrorAction SilentlyContinue
if ($health) {
    $health.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} else {
    Write-Host "API unreachable" -ForegroundColor Red
}

# Check DB
Write-Host "`nDatabase Connection:" -ForegroundColor Green
docker-compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION" 2>$null || Write-Host "DB unreachable" -ForegroundColor Red
```

### Complete Reset Script

```powershell
# Save as reset.ps1
Write-Host "Resetting Docker environment..." -ForegroundColor Red

# Stop & remove
.\docker-start.ps1 clean

# Remove dangling images
docker image prune -f

# Rebuild
.\docker-start.ps1 build

# Start
.\docker-start.ps1 up

Write-Host "Reset complete!" -ForegroundColor Green
```

---

## 📋 Checklist: Khi mọi thứ hết tác dụng

- [ ] Đã restart Docker Desktop
- [ ] Đã xóa dangling images/containers: `docker system prune`
- [ ] Đã check .env file có tất cả variables
- [ ] Đã verify SQL Server port không conflict
- [ ] Đã tăng Docker memory allocation
- [ ] Đã check firewall rules
- [ ] Đã xem FULL logs: `docker-compose logs`
- [ ] Đã reset từ đầu: `.\docker-start.ps1 clean; .\docker-start.ps1 up`

---

## 📞 Getting Help

Nếu vẫn cần help:

1. **Collect logs**:
   ```powershell
   docker-compose logs > logs.txt
   docker-compose ps > containers.txt
   ```

2. **Include information**:
   - Windows version
   - Docker version
   - .NET version
   - Error messages (full, từ logs)
   - Steps to reproduce

3. **Resources**:
   - [Docker Documentation](https://docs.docker.com/)
   - [MSSQL Docker](https://hub.docker.com/_/microsoft-mssql-server)
   - [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

---

**Last Updated**: March 20, 2026  
**Status**: Ready for troubleshooting
