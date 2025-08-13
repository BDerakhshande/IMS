using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryTransactionReportService: IInventoryTransactionReportService
    {
        private readonly IWarehouseDbContext _dbContext;

        public InventoryTransactionReportService(IWarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<InventoryTransactionReportDto>> GetReportAsync(InventoryTransactionReportItemDto filter)
        {
            var receiptQuery = _dbContext.ReceiptOrIssueItems
                .Include(i => i.ReceiptOrIssue)
                .Include(i => i.Product)
                .Include(i => i.Category)
                .Include(i => i.Group)
                .Include(i => i.Status)
                .Include(i => i.SourceWarehouse)
                .Include(i => i.SourceZone)
                .Include(i => i.SourceSection)
                .Include(i => i.DestinationWarehouse)
                .Include(i => i.DestinationZone)
                .Include(i => i.DestinationSection)
                .AsQueryable();

            if (filter.DocumentType == "Conversion")
            {
                // وقتی نوع سند تبدیل است، داده‌های رسید/حواله/انتقال را فیلتر کن تا نتیجه ندهد
                receiptQuery = receiptQuery.Where(i => false);
            }
            else
            {
                if (filter.FromDate.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.ReceiptOrIssue.Date >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.ReceiptOrIssue.Date <= filter.ToDate.Value);

                if (!string.IsNullOrEmpty(filter.DocumentType))
                {
                    if (Enum.TryParse<ReceiptOrIssueType>(filter.DocumentType, out var docTypeEnum))
                        receiptQuery = receiptQuery.Where(i => i.ReceiptOrIssue.Type == docTypeEnum);
                    else
                        return new List<InventoryTransactionReportDto>();
                }

                if (filter.CategoryId.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.CategoryId == filter.CategoryId);

                if (filter.GroupId.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.GroupId == filter.GroupId);

                if (filter.StatusId.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.StatusId == filter.StatusId);

                if (filter.ProductId.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.ProductId == filter.ProductId);

                if (filter.WarehouseId.HasValue)
                    receiptQuery = receiptQuery.Where(i =>
                        i.SourceWarehouseId == filter.WarehouseId || i.DestinationWarehouseId == filter.WarehouseId);

                if (filter.ZoneId.HasValue)
                    receiptQuery = receiptQuery.Where(i =>
                        i.SourceZoneId == filter.ZoneId || i.DestinationZoneId == filter.ZoneId);

                if (filter.SectionId.HasValue)
                    receiptQuery = receiptQuery.Where(i =>
                        i.SourceSectionId == filter.SectionId || i.DestinationSectionId == filter.SectionId);
            }

            var receiptResults = await receiptQuery
                .OrderBy(i => i.ReceiptOrIssue.Date)
                .Select(i => new InventoryTransactionReportDto
                {
                    Date = i.ReceiptOrIssue.Date.ToString("yyyy/MM/dd"),
                    DocumentNumber = i.ReceiptOrIssue.DocumentNumber,
                    DocumentType = i.ReceiptOrIssue.Type.ToString(),
                    ConversionType = null,

                    CategoryName = i.Category.Name ?? "",
                    GroupName = i.Group.Name ?? "",
                    StatusName = i.Status.Name ?? "",
                    ProductName = i.Product.Name,

                    SourceWarehouseName = i.SourceWarehouse.Name ?? "",
                    SourceDepartmentName = i.SourceZone.Name ?? "",
                    SourceSectionName = i.SourceSection.Name ?? "",

                    DestinationWarehouseName = i.DestinationWarehouse.Name ?? "",
                    DestinationDepartmentName = i.DestinationZone.Name ?? "",
                    DestinationSectionName = i.DestinationSection.Name ?? "",

                    Quantity = i.Quantity
                })
                .ToListAsync();

            // داده‌های سندهای تبدیل

            var consumedQuery = _dbContext.conversionConsumedItems
                .Include(c => c.Product)
                .Include(c => c.Category)
                .Include(c => c.Group)
                .Include(c => c.Status)
                .Include(c => c.Warehouse)
                .Include(c => c.Zone)
                .Include(c => c.Section)
                .Include(c => c.ConversionDocument)
                .AsQueryable();

            var producedQuery = _dbContext.conversionProducedItems
                .Include(p => p.Product)
                .Include(p => p.Category)
                .Include(p => p.Group)
                .Include(p => p.Status)
                .Include(p => p.Warehouse)
                .Include(p => p.Zone)
                .Include(p => p.Section)
                .Include(p => p.ConversionDocument)
                .AsQueryable();

            if (filter.FromDate.HasValue)
            {
                consumedQuery = consumedQuery.Where(c => c.ConversionDocument.CreatedAt >= filter.FromDate.Value);
                producedQuery = producedQuery.Where(p => p.ConversionDocument.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                consumedQuery = consumedQuery.Where(c => c.ConversionDocument.CreatedAt <= filter.ToDate.Value);
                producedQuery = producedQuery.Where(p => p.ConversionDocument.CreatedAt <= filter.ToDate.Value);
            }

            if (filter.DocumentType == "Conversion" || string.IsNullOrEmpty(filter.DocumentType))
            {
                if (filter.CategoryId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.CategoryId == filter.CategoryId);
                    producedQuery = producedQuery.Where(p => p.CategoryId == filter.CategoryId);
                }

                if (filter.GroupId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.GroupId == filter.GroupId);
                    producedQuery = producedQuery.Where(p => p.GroupId == filter.GroupId);
                }

                if (filter.StatusId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.StatusId == filter.StatusId);
                    producedQuery = producedQuery.Where(p => p.StatusId == filter.StatusId);
                }

                if (filter.ProductId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.ProductId == filter.ProductId);
                    producedQuery = producedQuery.Where(p => p.ProductId == filter.ProductId);
                }

                if (filter.WarehouseId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.WarehouseId == filter.WarehouseId);
                    producedQuery = producedQuery.Where(p => p.WarehouseId == filter.WarehouseId);
                }

                if (filter.ZoneId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.ZoneId == filter.ZoneId);
                    producedQuery = producedQuery.Where(p => p.ZoneId == filter.ZoneId);
                }

                if (filter.SectionId.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.SectionId == filter.SectionId);
                    producedQuery = producedQuery.Where(p => p.SectionId == filter.SectionId);
                }
            }
            else
            {
                consumedQuery = consumedQuery.Where(c => false);
                producedQuery = producedQuery.Where(p => false);
            }

            var consumedResults = await consumedQuery
                .OrderBy(c => c.ConversionDocument.CreatedAt)
                .Select(c => new InventoryTransactionReportDto
                {
                    Date = c.ConversionDocument.CreatedAt.ToString("yyyy/MM/dd"),
                    DocumentNumber = c.ConversionDocument.DocumentNumber,
                    DocumentType = "Conversion",
                    ConversionType = "Consumed",

                    CategoryName = c.Category.Name ?? "",
                    GroupName = c.Group.Name ?? "",
                    StatusName = c.Status.Name ?? "",
                    ProductName = c.Product.Name,

                    SourceWarehouseName = c.Warehouse.Name ?? "",
                    SourceDepartmentName = c.Zone.Name ?? "",
                    SourceSectionName = c.Section.Name ?? "",

                    DestinationWarehouseName = "",
                    DestinationDepartmentName = "",
                    DestinationSectionName = "",

                    Quantity = c.Quantity
                })
                .ToListAsync();

            var producedResults = await producedQuery
                .OrderBy(p => p.ConversionDocument.CreatedAt)
                .Select(p => new InventoryTransactionReportDto
                {
                    Date = p.ConversionDocument.CreatedAt.ToString("yyyy/MM/dd"),
                    DocumentNumber = p.ConversionDocument.DocumentNumber,
                    DocumentType = "Conversion",
                    ConversionType = "Produced",

                    CategoryName = p.Category.Name ?? "",
                    GroupName = p.Group.Name ?? "",
                    StatusName = p.Status.Name ?? "",
                    ProductName = p.Product.Name,

                    SourceWarehouseName = "",
                    SourceDepartmentName = "",
                    SourceSectionName = "",

                    DestinationWarehouseName = p.Warehouse.Name ?? "",
                    DestinationDepartmentName = p.Zone.Name ?? "",
                    DestinationSectionName = p.Section.Name ?? "",

                    Quantity = p.Quantity
                })
                .ToListAsync();

            var finalResults = receiptResults
                .Concat(consumedResults)
                .Concat(producedResults)
                .OrderBy(r => r.Date)
                .ToList();

            return finalResults;
        }




        public async Task<List<SelectListItem>> GetZonesByWarehouseIdAsync(int warehouseId)
        {
            return await _dbContext.StorageZones
                .Where(z => z.WarehouseId == warehouseId)
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllZonesAsync()
        {
            return await _dbContext.StorageZones
                .OrderBy(z => z.Name)
                .Select(z => new SelectListItem
                {
                    Value = z.Id.ToString(),
                    Text = z.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllSectionsAsync()
        {
            return await _dbContext.StorageSections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetAllGroupsAsync()
        {
            return await _dbContext.Groups
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllStatusesAsync()
        {
            return await _dbContext.Statuses
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetAllProductsAsync()
        {
            return await _dbContext.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetSectionsByZoneIdAsync(int zoneId)
        {
            return await _dbContext.StorageSections
                .Where(s => s.ZoneId == zoneId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }


        public async Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId)
        {
            return await _dbContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _dbContext.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId)
        {
            return await _dbContext.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }

        private string GetDocumentTypeName(string documentType)
        {
            return documentType switch
            {
                nameof(ReceiptOrIssueType.Receipt) => "رسید",
                nameof(ReceiptOrIssueType.Issue) => "حواله",
                nameof(ReceiptOrIssueType.Transfer) => "انتقال",
                "Conversion" => "تبدیل",
                _ => "نامشخص"
            };
        }


        public async Task<byte[]> ExportReportToExcelAsync(InventoryTransactionReportItemDto filter)
        {
            var data = await GetReportAsync(filter);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Inventory Transaction Report");

            // هدر ستون‌ها
            worksheet.Cell(1, 1).Value = "تاریخ";
            worksheet.Cell(1, 2).Value = "شماره سند";
            worksheet.Cell(1, 3).Value = "نوع سند";
            worksheet.Cell(1, 4).Value = "نوع تبدیل";
            worksheet.Cell(1, 5).Value = "دسته‌بندی";
            worksheet.Cell(1, 6).Value = "گروه";
            worksheet.Cell(1, 7).Value = "طبقه";
            worksheet.Cell(1, 8).Value = "کالا";
            worksheet.Cell(1, 9).Value = "انبار مبدأ";
            worksheet.Cell(1, 10).Value = "قسمت مبدأ";
            worksheet.Cell(1, 11).Value = "بخش مبدأ";
            worksheet.Cell(1, 12).Value = "انبار مقصد";
            worksheet.Cell(1, 13).Value = "قسمت مقصد";
            worksheet.Cell(1, 14).Value = "بخش مقصد";
            worksheet.Cell(1, 15).Value = "مقدار";

            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.Date;
                worksheet.Cell(row, 2).Value = item.DocumentNumber;
                worksheet.Cell(row, 3).Value = GetDocumentTypeName(item.DocumentType);
                worksheet.Cell(row, 4).Value = item.ConversionType ?? "";
                worksheet.Cell(row, 5).Value = item.CategoryName;
                worksheet.Cell(row, 6).Value = item.GroupName;
                worksheet.Cell(row, 7).Value = item.StatusName;
                worksheet.Cell(row, 8).Value = item.ProductName;
                worksheet.Cell(row, 9).Value = item.SourceWarehouseName;
                worksheet.Cell(row, 10).Value = item.SourceDepartmentName;
                worksheet.Cell(row, 11).Value = item.SourceSectionName;
                worksheet.Cell(row, 12).Value = item.DestinationWarehouseName;
                worksheet.Cell(row, 13).Value = item.DestinationDepartmentName;
                worksheet.Cell(row, 14).Value = item.DestinationSectionName;
                worksheet.Cell(row, 15).Value = item.Quantity;
                row++;
            }

            // جمع کل
            var total = data.Sum(x => x.Quantity);
            worksheet.Cell(row, 14).Value = "جمع کل:";
            worksheet.Cell(row, 15).Value = total;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
}
