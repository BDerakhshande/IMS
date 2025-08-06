using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Application.ProcurementManagement.DTOs
{
    public class GoodsRequestInputDto
    {
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public int StatusId { get; set; }
        public int ProductId { get; set; }
        public int ProjectId { get; set; }

        public string RequestedByName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;


        [Required(ErrorMessage = "لطفا مقدار درخواست را وارد کنید")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مقدار درخواست باید بزرگتر از صفر باشد")]
        public decimal RequestedQuantity { get; set; }



        // سلسله مراتب برای SelectBoxها
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Groups { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Products { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Projects { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
