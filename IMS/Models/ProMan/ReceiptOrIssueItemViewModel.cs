using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class ReceiptOrIssueItemViewModel
    {
        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? StatusId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int? SourceWarehouseId { get; set; }
        public int? SourceZoneId { get; set; }
        public int? SourceSectionId { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public int? DestinationZoneId { get; set; }
        public int? DestinationSectionId { get; set; }
        public List<SelectListItem>? AvailableGroups { get; set; }
        public List<SelectListItem>? AvailableStatuses { get; set; }
        public List<SelectListItem>? AvailableProducts { get; set; }

       

        public List<SelectListItem>? AvailableSourceWarehouses { get; set; }
        public List<SelectListItem>? AvailableSourceZones { get; set; }
        public List<SelectListItem>? AvailableSourceSections { get; set; }

        public List<SelectListItem>? AvailableDestinationWarehouses { get; set; }
        public List<SelectListItem>? AvailableDestinationZones { get; set; }
        public List<SelectListItem>? AvailableDestinationSections { get; set; }


        public List<SelectListItem>? AvailableProjects { get; set; }

        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }
    }
}
