using IMS.Application.ProcurementManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class GoodsRequestViewModel
    {
        public GoodsRequestViewModel()
        {
            Input = new GoodsRequestInputDto(); // این مهمه
        }
        public GoodsRequestInputDto Input { get; set; } = new GoodsRequestInputDto();

        // سلسله مراتب برای SelectBoxها
        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
            public IEnumerable<SelectListItem> Groups { get; set; } = Enumerable.Empty<SelectListItem>();
            public IEnumerable<SelectListItem> Statuses { get; set; } = Enumerable.Empty<SelectListItem>();
            public IEnumerable<SelectListItem> Products { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Projects { get; set; } = Enumerable.Empty<SelectListItem>();


    }
}
