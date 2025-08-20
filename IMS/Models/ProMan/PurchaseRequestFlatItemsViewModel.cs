using IMS.Application.ProcurementManagement.DTOs;
using IMS.Domain.ProjectManagement.Entities;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Models.ProMan
{
    public class PurchaseRequestFlatItemsViewModel
    {
        public List<PurchaseRequestFlatItemDto> FlatItems { get; set; } = new List<PurchaseRequestFlatItemDto>();

        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Status> Statuses { get; set; } = new List<Status>();
        public List<ProductFilterDto> Products { get; set; } = new List<ProductFilterDto>();

        public List<RequestTypeDto> RequestTypes { get; set; } = new List<RequestTypeDto>();

        public int? SelectedProjectId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedGroupId { get; set; }
        public int? SelectedStatusId { get; set; }
        public int? SelectedProductId { get; set; }
        public int? SelectedRequestTypeId { get; set; }
        public string? SelectedRequestNumber { get; set; }
        public string? SelectedRequestTitle { get; set; } // ← اضافه شد

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? FromDateString { get; set; }  // تاریخ شمسی ورودی کاربر
        public string? ToDateString { get; set; }
    }

}
