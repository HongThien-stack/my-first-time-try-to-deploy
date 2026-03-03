using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; } // 
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "ACTIVE"; // ACTIVE | INACTIVE
        public bool IsDeleted { get; set; } 
        public int ProductCount { get; set; } // Số lượng sản phẩm trong danh mục
        public DateTime CreatedAt { get; set; } // Ngày tạo
        public DateTime? UpdatedAt { get; set; } // Ngày cập nhật

    }
}
