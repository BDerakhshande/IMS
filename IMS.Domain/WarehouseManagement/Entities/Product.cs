using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class Product 
    { 
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; } = null!;
        public bool IsUnique { get; set; }
        public decimal Price { get; set; }
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>(); 
    }
}
