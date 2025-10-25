using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.WarehouseManagement.DTOs
{
    public class UniqueCodeHierarchyDto
    {

        public string Code { get; set; }           // کد یکتا
        public string CategoryName { get; set; }   // دسته
        public string GroupName { get; set; }      // گروه
        public string StatusName { get; set; }     // طبقه یا وضعیت
        public string ProductName { get; set; }    // محصول
    }
}
