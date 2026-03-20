# Identity Service Deployment Checklist

## ✅ Prerequties Check

- [ ] **Windows 10/11** (đã cài đặt)
- [ ] **Docker Desktop** cài đặt
  - [ ] Windows Subsystem for Linux 2 (WSL 2) enabled
  - [ ] Hyper-V or VirtualizationPlatform enabled
- [ ] **.NET 9.0 SDK** - Kiểm tra: `dotnet --version`
- [ ] **SQL Server** 2019+
  - [ ] Hoặc sẽ cài qua Docker
  - [ ] Sa password đã biết (hiện tại: `12345`)

---

## 📦 Docker Setup (Recommended)

### Phase 1: Preparation
- [ ] Mở PowerShell as Administrator
- [ ] Chuyển tới thư mục: `cd c:\GAME\deploy\OJT-Backend`
- [ ] Để tạo file `.env` từ `.env.example`:
- [ ] Inspect `docker-compose.yml` (thêm nếu cần ports khác)
- [ ] Inspect `Dockerfile` (kiểm tra build process)

### Phase 2: Configuration
- [ ] Mở file `.env` và cấu hình:
  - [ ] **DB_PASSWORD**: Thiết lập mật khẩu mạnh cho SQL Server
  - [ ] **JWT_SECRET_KEY**: Giữ hoặc tạo key mới (tối thiểu 32 ký tự)
  - [ ] **SMTP_HOST**, **SMTP_USER**, **SMTP_PASSWORD**: Cấu hình email
  - [ ] **CORS_ORIGIN_***: Thêm frontend URLs của bạn

### Phase 3: Docker Build & Run
- [ ] Xây dựng image:
  ```powershell
  .\docker-start.ps1 build
  ```
- [ ] Khởi chạy containers:
  ```powershell
  .\docker-start.ps1 up
  ```
- [ ] Chờ ~30 giây để services khởi động

### Phase 4: Verification
- [ ] Kiểm tra containers đang chạy:
  ```powershell
  .\docker-start.ps1 ps
  ```
  - [ ] `identity-sqlserver` - **healthy**
  - [ ] `identity-api` - **healthy**

- [ ] Kiểm tra health endpoint:
  ```powershell
  curl http://localhost:8080/health
  ```
  - [ ] Kết quả: `{"status":"Healthy","database":"Connected"}`

- [ ] Kiểm tra Swagger:
  - [ ] Mở browser: http://localhost:8080
  - [ ] Thấy danh sách API endpoints

### Phase 5: Database Verification
- [ ] Mở SQL Server Management Studio (SSMS)
- [ ] Connect tới: `localhost,1433`
  - [ ] Login: `sa`
  - [ ] Password: `DB_PASSWORD` từ `.env`
- [ ] Kiểm tra databases:
  - [ ] [ ] `master` - Tồn tại
  - [ ] [ ] `IdentityDB` - Được tạo
- [ ] Kiểm tra tables trong `IdentityDB`:
  - [ ] `users`
  - [ ] `roles`
  - [ ] `user_login_logs`
  - [ ] `user_audit_logs`

### Phase 6: API Testing
- [ ] Mở Swagger: http://localhost:8080
- [ ] Test health endpoint:
  - [ ] GET `/health` - Returns 200
- [ ] Expected endpoints:
  - [ ] POST `/api/auth/register`
  - [ ] POST `/api/auth/login`
  - [ ] POST `/api/auth/refresh-token`
  - [ ] GET `/api/users/{id}`
  - [ ] GET `/api/roles`

---

## 🖥️ Local Development Setup (Alternative)

### Phase 1: Preparation
- [ ] Mở PowerShell as Administrator
- [ ] Chuyển tới thư mục: `cd c:\GAME\deploy\OJT-Backend`
- [ ] Kiểm tra SQL Server đang chạy:
  ```powershell
  Get-Service "MSSQLSERVER" | Select-Object Status
  ```

### Phase 2: Configuration
- [ ] Mở file `IdentityService/appsettings.json`
- [ ] Cấu hình connection string:
  ```json
  "ConnectionStrings": {
    "IdentityDB": "Server=YOUR_SERVER;Database=IdentityDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
  ```
  - [ ] Thay `YOUR_SERVER` (thường là `.` hoặc `localhost`)
  - [ ] Thay `YOUR_PASSWORD`

### Phase 3: Project Setup
- [ ] Chạy setup script:
  ```powershell
  .\local-start.ps1 setup
  ```
  - [ ] ✅ Restore packages thành công
  - [ ] ✅ Build thành công
  - [ ] ✅ Migrations áp dụng thành công

### Phase 4: Verification
- [ ] Mở SSMS, kiểm tra:
  - [ ] Database `IdentityDB` được tạo
  - [ ] Tables được tạo (users, roles, etc.)

### Phase 5: Run Service
- [ ] Chạy service:
  ```powershell
  .\local-start.ps1 run
  ```
- [ ] Chờ tới thấy:
  ```
  Now listening on: http://localhost:5000
  ```

### Phase 6: API Testing
- [ ] Mở browser: http://localhost:5000
- [ ] Swagger UI hiển thị
- [ ] Test health:
  ```powershell
  .\local-start.ps1 test-api
  ```

---

## 🔄 Switching Between Docker & Local

### From Docker to Local
1. [ ] Dừng Docker:
   ```powershell
   .\docker-start.ps1 down
   ```
2. [ ] Cấu hình local database
3. [ ] Chạy local:
   ```powershell
   .\local-start.ps1 run
   ```

### From Local to Docker
1. [ ] Dừng local service (Ctrl+C)
2. [ ] Cấu hình `.env`
3. [ ] Khởi chạy Docker:
   ```powershell
   .\docker-start.ps1 up
   ```

---

## 🐛 Troubleshooting

### Issue: Docker build fails
- [ ] Clear Docker cache:
  ```powershell
  docker-compose build --no-cache
  ```
- [ ] Check internet connection
- [ ] Check Docker daemon running

### Issue: Port already in use
- [ ] Find and kill process:
  ```powershell
  netstat -ano | findstr :8080
  taskkill /PID <PID> /F
  ```
- [ ] Or change port in docker-compose.yml

### Issue: Database connection fails
- [ ] Check connection string
- [ ] Verify SQL Server running
- [ ] Check credentials (username, password)
- [ ] Verify network (Docker can reach host)

### Issue: Migrations not applied
- [ ] Docker: Check logs
  ```powershell
  .\docker-start.ps1 logs-api
  ```
- [ ] Local: Run manually
  ```powershell
  .\local-start.ps1 migrate
  ```

### Issue: Can't connect SSMS to Docker database
- [ ] Use `localhost,1433` (not just `localhost`)
- [ ] Enable TCP/IP in SQL Server Configuration Manager
- [ ] Check firewall (allow port 1433)
- [ ] Verify database is running:
  ```powershell
  .\docker-start.ps1 logs-db
  ```

---

## 📝 Important Files Created

| File | Purpose |
|------|---------|
| `Dockerfile` | Image definition |
| `docker-compose.yml` | Container orchestration |
| `.env` | Environment variables |
| `.env.example` | Template (for git) |
| `appsettings.Docker.json` | Docker-specific config |
| `.dockerignore` | Exclude files from image |
| `docker-start.ps1` | Docker management script |
| `local-start.ps1` | Local development script |
| `DOCKER_SETUP_GUIDE.md` | Detailed Docker guide |
| `QUICK_START.md` | Quick reference |
| `.gitignore` | Updated (excludes .env) |

---

## 🔐 Security Checklist

- [ ] ✅ `.env` file is in `.gitignore` (not committed)
- [ ] ✅ Strong DB password in `.env` (not in docs)
- [ ] ✅ JWT secret is 32+ characters
- [ ] ✅ SMTP password not in appsettings.json
- [ ] ✅ Token.txt not committed
- [ ] Before production:
  - [ ] Change all default passwords
  - [ ] Use secrets management system
  - [ ] Enable HTTPS
  - [ ] Configure firewall rules

---

## ✨ Next Steps

After successful deployment:

1. **Test API thoroughly**
   - [ ] Register user
   - [ ] Login
   - [ ] Generate tokens
   - [ ] Refresh tokens

2. **Set up other services**
   - [ ] Product Service
   - [ ] Inventory Service
   - [ ] POS Service
   - [ ] Promotion Service

3. **Configure API Gateway**
   - [ ] Route requests properly
   - [ ] Auth validation
   - [ ] Logging & monitoring

4. **CI/CD Pipeline**
   - [ ] GitHub Actions
   - [ ] Automated builds
   - [ ] Automated tests
   - [ ] Auto-deploy

---

## 💡 Useful Commands Reference

### Docker Commands
```powershell
# Startup
.\docker-start.ps1 up              # Start all
.\docker-start.ps1 build           # Build images
.\docker-start.ps1 restart         # Restart

# Management
.\docker-start.ps1 ps              # List containers
.\docker-start.ps1 logs            # View all logs
.\docker-start.ps1 logs-api        # View API logs
.\docker-start.ps1 health          # Check health

# Cleanup
.\docker-start.ps1 stop            # Stop (keep data)
.\docker-start.ps1 down            # Stop and remove
.\docker-start.ps1 clean           # Remove all (delete data!)
```

### Local Commands
```powershell
# Startup
.\local-start.ps1 setup            # First time setup
.\local-start.ps1 run              # Start service

# Management
.\local-start.ps1 migrate          # Apply migrations
.\local-start.ps1 test-api         # Test health
.\local-start.ps1 build            # Build project
.\local-start.ps1 clean            # Clean artifacts

# Development
.\local-start.ps1 add-migration -Name MyMigration
.\local-start.ps1 open-swagger     # Open browser
```

---

## 📞 Support

If you encounter issues:
1. Check **[DOCKER_SETUP_GUIDE.md](DOCKER_SETUP_GUIDE.md)** - Detailed troubleshooting
2. Check **[QUICK_START.md](QUICK_START.md)** - Quick reference
3. Review logs:
   - Docker: `.\docker-start.ps1 logs`
   - Local: Console output from `.\local-start.ps1 run`

---

## 🎉 Success Indicators

You've successfully completed setup when:

- ✅ Containers/services are running (healthy)
- ✅ Database created with tables
- ✅ Health endpoint returns "Healthy"
- ✅ Swagger UI accessible
- ✅ Can connect to database from SSMS
- ✅ No errors in logs

---

**Status**: ⏳ Ready to deploy  
**Last Updated**: March 20, 2026  
**Verified On**: Windows 10/11 + Docker Desktop + SQL Server

