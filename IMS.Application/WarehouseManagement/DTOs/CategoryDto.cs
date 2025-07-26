using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
     
        public List<GroupDto>? Groups { get; set; }

        public string CodePadded
        {
            get
            {
                if (int.TryParse(Code, out int num))
                    return num.ToString("");
                return Code ?? "";
            }
        }

        public string DisplayCode => $"C{CodePadded}";


    }
}
