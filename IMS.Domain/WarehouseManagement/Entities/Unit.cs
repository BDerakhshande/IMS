using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class Unit
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!; // مثل کیلوگرم، عدد، بسته
        public string? Symbol { get; set; }       // مثل kg, pcs, box

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
