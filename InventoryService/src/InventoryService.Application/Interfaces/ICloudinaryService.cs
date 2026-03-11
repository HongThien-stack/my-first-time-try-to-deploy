using Microsoft.AspNetCore.Http;

namespace InventoryService.Application.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload a single image to Cloudinary
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="folder">Folder on Cloudinary (default: damage-reports)</param>
    /// <returns>URL of uploaded image</returns>
    Task<string> UploadImageAsync(IFormFile file, string folder = "damage-reports");

    /// <summary>
    /// Upload multiple images to Cloudinary
    /// </summary>
    /// <param name="files">List of image files</param>
    /// <param name="folder">Folder on Cloudinary</param>
    /// <returns>List of URLs of uploaded images</returns>
    Task<List<string>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "damage-reports");

    /// <summary>
    /// Delete an image from Cloudinary using its public ID
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteImageAsync(string publicId);

    /// <summary>
    /// Delete multiple images from Cloudinary
    /// </summary>
    /// <param name="publicIds">List of Public IDs</param>
    /// <returns>True if all deleted successfully</returns>
    Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds);
}
