using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryService.Application.Services;

public class DamageReportService : IDamageReportService
{
    private readonly IDamageReportRepository _damageReportRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<DamageReportService> _logger;

    public DamageReportService(
        IDamageReportRepository damageReportRepository,
        ICloudinaryService cloudinaryService,
        ILogger<DamageReportService> logger)
    {
        _damageReportRepository = damageReportRepository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task<IEnumerable<DamageReportListDto>> GetAllDamageReportsAsync()
    {
        try
        {
            var reports = await _damageReportRepository.GetAllAsync();
            return reports.Select(r => new DamageReportListDto
            {
                Id = r.Id,
                ReportNumber = r.ReportNumber,
                LocationType = r.LocationType,
                LocationId = r.LocationId,
                DamageType = r.DamageType,
                ReportedBy = r.ReportedBy,
                ReportedDate = r.ReportedDate,
                TotalValue = r.TotalValue,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all damage reports");
            throw;
        }
    }

    public async Task<DamageReportDto?> GetDamageReportByIdAsync(Guid id)
    {
        try
        {
            var report = await _damageReportRepository.GetByIdAsync(id);
            if (report == null)
                return null;

            return new DamageReportDto
            {
                Id = report.Id,
                ReportNumber = report.ReportNumber,
                LocationType = report.LocationType,
                LocationId = report.LocationId,
                DamageType = report.DamageType,
                ReportedBy = report.ReportedBy,
                ReportedDate = report.ReportedDate,
                TotalValue = report.TotalValue,
                Description = report.Description,
                Photos = !string.IsNullOrEmpty(report.Photos) 
                    ? JsonSerializer.Deserialize<List<string>>(report.Photos) 
                    : null,
                Status = report.Status,
                ApprovedBy = report.ApprovedBy,
                ApprovedDate = report.ApprovedDate,
                CreatedAt = report.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting damage report by id: {Id}", id);
            throw;
        }
    }

    public async Task<DamageReportDto> CreateDamageReportAsync(CreateDamageReportRequest request)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.LocationType))
                throw new ArgumentException("LocationType is required");
            
            if (request.LocationId == Guid.Empty)
                throw new ArgumentException("LocationId is required");
            
            if (string.IsNullOrWhiteSpace(request.DamageType))
                throw new ArgumentException("DamageType is required");
            
            if (request.ReportedBy == Guid.Empty)
                throw new ArgumentException("ReportedBy is required");

            // Upload photos to Cloudinary if provided
            List<string>? photoUrls = null;
            if (request.Photos != null && request.Photos.Any())
            {
                photoUrls = await _cloudinaryService.UploadImagesAsync(request.Photos);
                _logger.LogInformation("Successfully uploaded {Count} photos to Cloudinary", photoUrls.Count);
            }

            // Generate report number (DMG-YYYY-XXXXX)
            var year = DateTime.UtcNow.Year;
            var reportNumber = $"DMG-{year}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";

            // Create damage report entity
            var damageReport = new DamageReport
            {
                Id = Guid.NewGuid(),
                ReportNumber = reportNumber,
                LocationType = request.LocationType.ToUpper(),
                LocationId = request.LocationId,
                DamageType = request.DamageType.ToUpper(),
                ReportedBy = request.ReportedBy,
                ReportedDate = request.ReportedDate,
                TotalValue = request.TotalValue,
                Description = request.Description,
                Photos = photoUrls != null && photoUrls.Any() ? JsonSerializer.Serialize(photoUrls) : null,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            var savedReport = await _damageReportRepository.AddAsync(damageReport);

            _logger.LogInformation("Damage report created successfully: {ReportNumber}", reportNumber);

            // Return DTO
            return new DamageReportDto
            {
                Id = savedReport.Id,
                ReportNumber = savedReport.ReportNumber,
                LocationType = savedReport.LocationType,
                LocationId = savedReport.LocationId,
                DamageType = savedReport.DamageType,
                ReportedBy = savedReport.ReportedBy,
                ReportedDate = savedReport.ReportedDate,
                TotalValue = savedReport.TotalValue,
                Description = savedReport.Description,
                Photos = photoUrls,
                Status = savedReport.Status,
                ApprovedBy = savedReport.ApprovedBy,
                ApprovedDate = savedReport.ApprovedDate,
                CreatedAt = savedReport.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating damage report");
            throw;
        }
    }

    public async Task<DamageReportDto> ApproveDamageReportAsync(Guid id, ApproveDamageReportRequest request)
    {
        try
        {
            // Validate approver
            if (request.ApprovedBy == Guid.Empty)
                throw new ArgumentException("ApprovedBy is required");

            // Get damage report
            var damageReport = await _damageReportRepository.GetByIdAsync(id);
            if (damageReport == null)
                throw new KeyNotFoundException($"Damage report with ID {id} not found");

            // Check if already approved or rejected
            if (damageReport.Status != "PENDING")
                throw new InvalidOperationException($"Cannot approve damage report with status {damageReport.Status}");

            // Update status to APPROVED
            damageReport.Status = "APPROVED";
            damageReport.ApprovedBy = request.ApprovedBy;
            damageReport.ApprovedDate = DateTime.UtcNow;

            // Save changes
            await _damageReportRepository.UpdateAsync(damageReport);

            _logger.LogInformation("Damage report {ReportNumber} approved by {ApprovedBy}", 
                damageReport.ReportNumber, request.ApprovedBy);

            // Return updated DTO
            return new DamageReportDto
            {
                Id = damageReport.Id,
                ReportNumber = damageReport.ReportNumber,
                LocationType = damageReport.LocationType,
                LocationId = damageReport.LocationId,
                DamageType = damageReport.DamageType,
                ReportedBy = damageReport.ReportedBy,
                ReportedDate = damageReport.ReportedDate,
                TotalValue = damageReport.TotalValue,
                Description = damageReport.Description,
                Photos = !string.IsNullOrEmpty(damageReport.Photos) 
                    ? JsonSerializer.Deserialize<List<string>>(damageReport.Photos) 
                    : null,
                Status = damageReport.Status,
                ApprovedBy = damageReport.ApprovedBy,
                ApprovedDate = damageReport.ApprovedDate,
                CreatedAt = damageReport.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving damage report {Id}", id);
            throw;
        }
    }
}
