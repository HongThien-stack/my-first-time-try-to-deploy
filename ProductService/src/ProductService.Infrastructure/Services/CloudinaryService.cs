using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        
        var account = new Account(
            settings.Value.CloudName,
            settings.Value.ApiKey,
            settings.Value.ApiSecret
        );
        
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder = "products")
    {
        ValidateImage(file);

        try
        {
            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation()
                    .Width(1000)
                    .Height(1000)
                    .Crop("limit")
                    .Quality("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                throw new Exception($"Lỗi upload ảnh: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Image uploaded successfully: {Url}", uploadResult.SecureUrl);
            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            throw new Exception("Lỗi khi upload ảnh lên Cloudinary", ex);
        }
    }

    public async Task<List<string>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "products")
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
            throw new ArgumentException("File không hợp lệ");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"Kích thước file không được vượt quá {MaxFileSize / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", _allowedExtensions)}");
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
        // Extract public_id from Cloudinary URL
        // Example: https://res.cloudinary.com/cloud-name/image/upload/v123456/folder/image.jpg
        // Public ID: folder/image
        
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Split('/');
        
        // Find the index after "upload"
        var uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex == -1 || uploadIndex >= segments.Length - 2)
        {
            throw new Exception("Invalid Cloudinary URL format");
        }
        
        // Skip version (v123456) and get the rest
        var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
        var publicId = string.Join("/", publicIdParts);
        
        // Remove file extension
        return Path.GetFileNameWithoutExtension(publicId);
    }
}
