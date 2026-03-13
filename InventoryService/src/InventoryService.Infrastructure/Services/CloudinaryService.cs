using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing or incomplete");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _logger = logger;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder = "damage-reports")
    {
        ValidateImage(file);

        try
        {
            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation()
                    .Width(1500)
                    .Height(1500)
                    .Crop("limit")
                    .Quality("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Image uploaded successfully: {Url}", uploadResult.SecureUrl);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            throw;
        }
    }

    public async Task<List<string>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "damage-reports")
    {
        var urls = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var url = await UploadImageAsync(file, folder);
                urls.Add(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image {FileName}", file.FileName);
                
                // Rollback: Delete all uploaded images if one fails
                if (urls.Any())
                {
                    await RollbackUploadedImages(urls);
                }
                
                throw;
            }
        }

        return urls;
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }

    public async Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds)
    {
        var allDeleted = true;

        foreach (var publicId in publicIds)
        {
            var deleted = await DeleteImageAsync(publicId);
            if (!deleted)
            {
                allDeleted = false;
            }
        }

        return allDeleted;
    }

    private void ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is not valid");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File size cannot exceed {MaxFileSize / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File format not supported. Only accept: {string.Join(", ", _allowedExtensions)}");
        }
    }

    private async Task RollbackUploadedImages(List<string> urls)
    {
        _logger.LogWarning("Rolling back {Count} uploaded images", urls.Count);
        
        foreach (var url in urls)
        {
            try
            {
                var publicId = GetPublicIdFromUrl(url);
                await DeleteImageAsync(publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during rollback for image: {Url}", url);
            }
        }
    }

    private string GetPublicIdFromUrl(string url)
    {
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Split('/');
        
        var uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex == -1 || uploadIndex >= segments.Length - 2)
        {
            throw new Exception("Invalid Cloudinary URL format");
        }
        
        var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
        var publicId = string.Join("/", publicIdParts);
        
        return Path.GetFileNameWithoutExtension(publicId);
    }
}
