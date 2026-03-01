using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload một ảnh lên Cloudinary
    /// </summary>
    /// <param name="file">File ảnh cần upload</param>
    /// <param name="folder">Thư mục trên Cloudinary (mặc định: products)</param>
    /// <returns>URL của ảnh đã upload</returns>
    Task<string> UploadImageAsync(IFormFile file, string folder = "products");

    /// <summary>
    /// Upload nhiều ảnh lên Cloudinary
    /// </summary>
    /// <param name="files">Danh sách file ảnh</param>
    /// <param name="folder">Thư mục trên Cloudinary</param>
    /// <returns>Danh sách URL của các ảnh đã upload</returns>
    Task<List<string>> UploadImagesAsync(IEnumerable<IFormFile> files, string folder = "products");

    /// <summary>
    /// Xóa một ảnh từ Cloudinary
    /// </summary>
    /// <param name="publicId">Public ID của ảnh trên Cloudinary</param>
    /// <returns>True nếu xóa thành công</returns>
    Task<bool> DeleteImageAsync(string publicId);

    /// <summary>
    /// Xóa nhiều ảnh từ Cloudinary
    /// </summary>
    /// <param name="publicIds">Danh sách Public IDs</param>
    /// <returns>True nếu tất cả xóa thành công</returns>
    Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds);
}
