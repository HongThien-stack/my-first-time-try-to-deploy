using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.DTOs
{
    public class WarehouseUpdateRequest
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public int? Capacity { get; set; }
        public string? Status { get; set; }
    }
}
