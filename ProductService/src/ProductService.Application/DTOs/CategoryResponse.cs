using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = "ACTIVE";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
