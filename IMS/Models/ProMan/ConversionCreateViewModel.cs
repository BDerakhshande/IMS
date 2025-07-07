using IMS.Application.WarehouseManagement.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Models.ProMan
{
    public class ConversionCreateViewModel
    {
        public string? DateString { get; set; } // برای ورودی شمسی کاربر

        public DateTime? Date { get; set; } // برای ذخیره در دیتابیس به میلادی

        public string? DocumentNumber { get; set; } // شماره سند

        public int WarehouseId { get; set; }
        public List<SelectListItem> Warehouses { get; set; }

        // لیست‌های جدید برای دسته‌بندی، گروه، وضعیت
        public List<SelectListItem> Categories { get; set; }
        public List<GroupDto> Groups { get; set; }

        public List<StatusDto> Statuses { get; set; }

        public List<ProductDto> Products { get; set; }
        public List<StorageZoneDto> Zones { get; set; }
        public List<StorageSectionDto> Sections { get; set; }

        public List<ConversionConsumedItemDto> ConsumedItems { get; set; }
        public List<ConversionProducedItemDto> ProducedItems { get; set; }
        


    }
}
