using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
