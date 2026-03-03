using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs;
// DTO for updating category, chỉ cho phép cập nhật Name và Status
public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Id is required")]
    [MaxLength(255, ErrorMessage = "Id cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("ACTIVE|INACTIVE", ErrorMessage = "Status must be either 'ACTIVE' or 'INACTIVE'")]
    public string Status { get; set; } = "ACTIVE";
    public bool IsDeleted { get; set; } = false;

}
