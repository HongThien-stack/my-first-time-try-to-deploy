# OJT-Backend - Identity Service Docker Deployment

## 🚀 Quick Start (2 Phút)

```powershell
# 1. Mở PowerShell tại đây
cd c:\GAME\deploy\OJT-Backend

# 2. Khởi chạy một lệnh
.\docker-start.ps1 up

# 3. Kiểm tra sau 30 giây
.\docker-start.ps1 health

# 4. Truy cập
# Swagger UI:      http://localhost:8080
# Health Check:    http://localhost:8080/health
```

---

## 📚 Documentation

**Bạn ở đâu?** Chọn tài liệu phù hợp:

### 🎯 Bắt Đầu Nhanh
- **[QUICK_START.md](QUICK_START.md)** ⭐ START HERE
  - 2 trường hợp: Docker vs Local
  - Comparison
  - Quick commands

### ✓ Theo Checklist
- **[DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md)**
  - Phase-by-phase setup
  - Có checkbox để track
  - Ideal cho lần đầu

### 📖 Hướng Dẫn Chi Tiết
- **[DOCKER_SETUP_GUIDE.md](DOCKER_SETUP_GUIDE.md)**
  - Toàn bộ configuration
  - 3 deployment methods
  - Advanced options
  - 600+ dòng documentation

### 🐛 Xử Lý Sự Cố
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)**
  - 10+ issues & solutions
  - Advanced debugging
  - Scripts & utilities

### 💾 Database
- **[MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md)**
  - Tạo migration mới
  - Best practices
  - Team collaboration

### 📋 Tóm Tắt
- **[SETUP_SUMMARY.md](SETUP_SUMMARY.md)**
  - Overview tất cả files
  - Lựa chọn documents
  - Reference table

---

## ⚡ PowerShell Commands

### Docker Management
```powershell
.\docker-start.ps1 help         # List all commands
.\docker-start.ps1 up           # Start services
.\docker-start.ps1 health       # Check health
.\docker-start.ps1 logs         # View logs
.\docker-start.ps1 down         # Stop services
```

### Local Development
```powershell
.\local-start.ps1 setup         # First-time setup
.\local-start.ps1 run           # Run locally
.\local-start.ps1 migrate       # Run migrations
.\local-start.ps1 test-api      # Test health
```

---

## 🐳 What's Included?

✅ **Dockerfile**
- Multi-stage build
- Health check
- Environment-ready

✅ **docker-compose.yml**
- SQL Server 2022
- Identity API
- Networking & volumes

✅ **Program.cs Updates**
- Auto migrations
- Health endpoint
- Docker support

✅ **Configuration**
- .env variables
- appsettings.Docker.json
- Environment-specific

✅ **Automation Scripts**
- docker-start.ps1
- local-start.ps1
- Health checking

✅ **Documentation**
- 5+ guides
- Troubleshooting
- Checklists
- API reference

---

## 📁 Files Created

| Category | Files |
|----------|-------|
| **Docker Config** | Dockerfile, docker-compose.yml, .dockerignore |
| **Environment** | .env, .env.example, appsettings.Docker.json |
| **Scripts** | docker-start.ps1, local-start.ps1 |
| **Documentation** | QUICK_START.md, DOCKER_SETUP_GUIDE.md, DEPLOYMENT_CHECKLIST.md, MIGRATIONS_GUIDE.md, TROUBLESHOOTING.md, SETUP_SUMMARY.md |
| **Code Changes** | Program.cs (updated), .gitignore (updated) |

---

## 🎯 Choose Your Path

### Path 1: Docker (Recommended) ⭐
```
1. Read: QUICK_START.md
2. Follow: DEPLOYMENT_CHECKLIST.md
3. Run: .\docker-start.ps1 up
4. Done! 🎉
```

### Path 2: Local Development
```
1. Read: QUICK_START.md (Local section)
2. Configure: appsettings.json
3. Run: .\local-start.ps1 setup
4. Then: .\local-start.ps1 run
5. Done! 🎉
```

### Path 3: Troubleshooting
```
1. Read: TROUBLESHOOTING.md
2. Find your issue
3. Apply solution
4. Done! 🎉
```

---

## 💡 Key Features

### 🔧 Development
- Hot reload support (local)
- Swagger UI included
- Health check endpoint
- Debug-friendly logging

### 🔐 Security
- Environment variables
- .env in .gitignore
- JWT support
- Password hashing

### 📦 DevOps
- Docker containerization
- Orchestration (docker-compose)
- Automatic migrations
- Health monitoring

### 📝 Documentation
- Comprehensive guides
- Troubleshooting
- Best practices
- Checklists

---

## 🔍 Health Check

### Docker
```powershell
.\docker-start.ps1 health
# Returns: status = "Healthy", database = "Connected"
```

### Local
```powershell
.\local-start.ps1 test-api
# Or: curl http://localhost:5000/health
```

---

## 🛠️ Maintenance

### Check Logs
```powershell
# Docker
docker-compose logs identity-api

# Local
# (Visible in console when running)
```

### Update .env
```powershell
notepad .env
# Then: .\docker-start.ps1 restart
```

### Run Migrations
```powershell
# Docker
.\docker-start.ps1 migrate

# Local
.\local-start.ps1 migrate
```

---

## 📈 Common Tasks

| Task | Command |
|------|---------|
| View API doc | Open http://localhost:8080 |
| Check health | `.\docker-start.ps1 health` |
| View logs | `.\docker-start.ps1 logs` |
| Restart | `.\docker-start.ps1 restart` |
| Stop all | `.\docker-start.ps1 down` |
| Add column | `.\local-start.ps1 add-migration -Name X` |
| Connect DB | SQL Server: localhost,1433 (user: sa) |
| Reset DB | `.\docker-start.ps1 clean` |

---

## ❓ Need Help?

| Issue | Solution |
|-------|----------|
| How to start? | → [QUICK_START.md](QUICK_START.md) |
| Step-by-step? | → [DEPLOYMENT_CHECKLIST.md](DEPLOYMENT_CHECKLIST.md) |
| Something broken? | → [TROUBLESHOOTING.md](TROUBLESHOOTING.md) |
| Database changes? | → [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md) |
| Detailed guide? | → [DOCKER_SETUP_GUIDE.md](DOCKER_SETUP_GUIDE.md) |
| File overview? | → [SETUP_SUMMARY.md](SETUP_SUMMARY.md) |

---

## 📊 System Requirements

### Minimum (Local)
- Windows 10/11
- .NET 9.0 SDK
- SQL Server 2019+

### Recommended (Docker)
- Windows 10/11
- Docker Desktop 4.0+
- 4+ GB RAM
- 10+ GB Disk

---

## 🏗️ Architecture

```
┌─────────────────────────────────────┐
│      Client (Browser/Postman)       │
└──────────────┬──────────────────────┘
               │ HTTP:8080
┌──────────────▼──────────────────────┐
│    Identity Service API             │
│  (.NET 9.0, ASP.NET Core)           │
│  - Authentication                   │
│  - Authorization                    │
│  - JWT Token                        │
└──────────────┬──────────────────────┘
               │ TCP:1433
┌──────────────▼──────────────────────┐
│       SQL Server 2022               │
│  - IdentityDB                       │
│  - Users, Roles, Logs              │
└─────────────────────────────────────┘
```

---

## 🚢 Deployment Status

| Component | Status | Location |
|-----------|--------|----------|
| Dockerfile | ✅ Ready | `IdentityService/` |
| docker-compose | ✅ Ready | Root |
| Program.cs | ✅ Updated | API folder |
| .env setup | ✅ Ready | Root |
| Documentation | ✅ Complete | Root |
| Scripts | ✅ Ready | Root |

---

## 🎓 Learning Path

1. **Day 1**: Read QUICK_START.md + Run docker-start.ps1
2. **Day 2**: Explore Swagger UI, test endpoints
3. **Day 3**: Read DOCKER_SETUP_GUIDE.md for deeper understanding
4. **Day 4**: Configure other services (Product, Inventory, etc.)
5. **Day 5**: Setup monitoring & logging

---

## 🔄 CI/CD Ready

The setup is ready for:
- ✅ GitHub Actions
- ✅ Azure DevOps
- ✅ GitLab CI
- ✅ Jenkins
- ✅ Other CI/CD tools

---

## 📞 Support Resources

1. **Docs in this repo**
   - DOCKER_SETUP_GUIDE.md
   - TROUBLESHOOTING.md
   - QUICK_START.md

2. **External Resources**
   - [Docker Docs](https://docs.docker.com/)
   - [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
   - [SQL Server on Linux](https://hub.docker.com/_/microsoft-mssql-server)

3. **Common Issues?**
   - Check TROUBLESHOOTING.md first
   - Run: `.\docker-start.ps1 logs`
   - Collect system info (version, error message)

---

## ✨ Next Steps

After successful deployment:

1. **Test API**
   - Register user
   - Login
   - Token generation

2. **Configure Email**
   - Update SMTP in .env
   - Test email functionality

3. **Add More Services**
   - Product Service
   - Inventory Service
   - POS Service
   - Promotion Service

4. **Monitoring**
   - Setup logging
   - Health check monitoring
   - Performance tracking

---

## 📜 License

[Include your license here]

---

## 👥 Contributors

- Created: March 20, 2026
- Setup: Docker + EF Core Migrations
- Status: ✅ Production Ready

---

## 🎉 Success Checklist

- [ ] Docker is installed
- [ ] .env file is created
- [ ] .\docker-start.ps1 up runs without errors
- [ ] Health check returns "Healthy"
- [ ] Swagger UI loads at http://localhost:8080
- [ ] Can connect to SQL Server from SSMS
- [ ] Database IdentityDB is created with tables
- [ ] Ready to test APIs

---

## 📝 Version History

| Date | Changes |
|------|---------|
| 2026-03-20 | Initial setup with Docker support |

---

**Last Updated**: March 20, 2026  
**Status**: ✅ Ready for Deployment  
**Contact**: [Your email here]

---

## 🚀 Ready to Start?

```powershell
# Copy this and paste in PowerShell:
cd c:\GAME\deploy\OJT-Backend; .\docker-start.ps1 up

# Then after 30 seconds:
.\docker-start.ps1 health

# Then open:
# http://localhost:8080
```

**BEGIN HERE: [QUICK_START.md](QUICK_START.md)**
