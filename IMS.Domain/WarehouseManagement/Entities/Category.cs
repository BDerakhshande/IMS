using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMS.Domain.WarehouseManagement.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = string.Empty;
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
