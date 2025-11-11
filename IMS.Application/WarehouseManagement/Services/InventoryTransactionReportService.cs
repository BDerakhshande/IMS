using ClosedXML.Excel;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace IMS.Application.WarehouseManagement.Services
{
    public class InventoryTransactionReportService : IInventoryTransactionReportService
    {
        private readonly IWarehouseDbContext _dbContext;
        private readonly ILogger<InventoryTransactionReportService> _logger;

        public InventoryTransactionReportService(IWarehouseDbContext dbContext, ILogger<InventoryTransactionReportService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<InventoryTransactionReportDto>> GetReportAsync(InventoryTransactionReportItemDto filter)
        {
            var results = new List<InventoryTransactionReportDto>();

            //-----------------------------------
            // تبدیل تاریخ‌ها
            //-----------------------------------
            DateTime? fromDate = filter.FromDate;
            DateTime? toDate = filter.ToDate;

            //-----------------------------------
            // ۱. رسید / حواله / انتقال
            //-----------------------------------
            if (filter.DocumentType == "Receipt" || filter.DocumentType == "Issue" || filter.DocumentType == "Transfer" || string.IsNullOrEmpty(filter.DocumentType))
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

                if (fromDate.HasValue) receiptQuery = receiptQuery.Where(i => i.ReceiptOrIssue.Date >= fromDate.Value);
                if (toDate.HasValue) receiptQuery = receiptQuery.Where(i => i.ReceiptOrIssue.Date <= toDate.Value);

                if (!string.IsNullOrEmpty(filter.DocumentType) &&
                    Enum.TryParse<ReceiptOrIssueType>(filter.DocumentType, out var docTypeEnum))
                {
                    receiptQuery = receiptQuery.Where(i => i.ReceiptOrIssue.Type == docTypeEnum);
                }

                // فیلترهای دیگر
                if (filter.CategoryId.HasValue) receiptQuery = receiptQuery.Where(i => i.CategoryId == filter.CategoryId);
                if (filter.GroupId.HasValue) receiptQuery = receiptQuery.Where(i => i.GroupId == filter.GroupId);
                if (filter.StatusId.HasValue) receiptQuery = receiptQuery.Where(i => i.StatusId == filter.StatusId);
                if (filter.ProductId.HasValue) receiptQuery = receiptQuery.Where(i => i.ProductId == filter.ProductId);
                if (filter.WarehouseId.HasValue)
                    receiptQuery = receiptQuery.Where(i => i.SourceWarehouseId == filter.WarehouseId || i.DestinationWarehouseId == filter.WarehouseId);

                var receiptResults = await receiptQuery.Select(i => new InventoryTransactionReportDto
                {
                    Date = i.ReceiptOrIssue.Date.ToString("yyyy/MM/dd"),
                    DocumentNumber = i.ReceiptOrIssue.DocumentNumber,
                    DocumentType = i.ReceiptOrIssue.Type.ToString(),
                    ConversionType = null,
                    CategoryName = i.Category.Name,
                    GroupName = i.Group.Name,
                    StatusName = i.Status.Name,
                    ProductName = i.Product.Name,
                    SourceWarehouseName = i.SourceWarehouse.Name ?? "",
                    SourceDepartmentName = i.SourceZone.Name ?? "",
                    SourceSectionName = i.SourceSection.Name ?? "",
                    DestinationWarehouseName = i.DestinationWarehouse.Name ?? "",
                    DestinationDepartmentName = i.DestinationZone.Name ?? "",
                    DestinationSectionName = i.DestinationSection.Name ?? "",
                    Quantity = i.Quantity
                }).ToListAsync();

                results.AddRange(receiptResults);
            }

            //-----------------------------------
            // ۲. تبدیل (Conversion)
            //-----------------------------------
            if (filter.DocumentType == "Conversion" || string.IsNullOrEmpty(filter.DocumentType))
            {
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

                if (fromDate.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.ConversionDocument.CreatedAt >= fromDate.Value);
                    producedQuery = producedQuery.Where(p => p.ConversionDocument.CreatedAt >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    consumedQuery = consumedQuery.Where(c => c.ConversionDocument.CreatedAt <= toDate.Value);
                    producedQuery = producedQuery.Where(p => p.ConversionDocument.CreatedAt <= toDate.Value);
                }

                // فیلترهای دیگر
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

                var consumedResults = await consumedQuery.Select(c => new InventoryTransactionReportDto
                {
                    Date = c.ConversionDocument.CreatedAt.ToString("yyyy/MM/dd"),
                    DocumentNumber = c.ConversionDocument.DocumentNumber,
                    DocumentType = "Conversion",
                    ConversionType = "Consumed",
                    CategoryName = c.Category.Name,
                    GroupName = c.Group.Name,
                    StatusName = c.Status.Name,
                    ProductName = c.Product.Name,
                    SourceWarehouseName = c.Warehouse.Name ?? "",
                    SourceDepartmentName = c.Zone.Name ?? "",
                    SourceSectionName = c.Section.Name ?? "",
                    DestinationWarehouseName = "",
                    DestinationDepartmentName = "",
                    DestinationSectionName = "",
                    Quantity = c.Quantity
                }).ToListAsync();

                var producedResults = await producedQuery.Select(p => new InventoryTransactionReportDto
                {
                    Date = p.ConversionDocument.CreatedAt.ToString("yyyy/MM/dd"),
                    DocumentNumber = p.ConversionDocument.DocumentNumber,
                    DocumentType = "Conversion",
                    ConversionType = "Produced",
                    CategoryName = p.Category.Name,
                    GroupName = p.Group.Name,
                    StatusName = p.Status.Name,
                    ProductName = p.Product.Name,
                    SourceWarehouseName = "",
                    SourceDepartmentName = "",
                    SourceSectionName = "",
                    DestinationWarehouseName = p.Warehouse.Name ?? "",
                    DestinationDepartmentName = p.Zone.Name ?? "",
                    DestinationSectionName = p.Section.Name ?? "",
                    Quantity = p.Quantity
                }).ToListAsync();

                results.AddRange(consumedResults);
                results.AddRange(producedResults);
            }

            //-----------------------------------
            // ۳. اصلاح موجودی (Inventory Adjustment)
            //-----------------------------------
            if (filter.DocumentType == "InventoryAdjustment" || string.IsNullOrEmpty(filter.DocumentType))
            {
                var adjustmentQuery = _dbContext.InventoryTransactions
                    .Include(t => t.Product)
                        .ThenInclude(p => p.Status)
                            .ThenInclude(s => s.Group)
                                .ThenInclude(g => g.Category)
                    .Include(t => t.Warehouse)
                    .Include(t => t.Zone)
                    .Include(t => t.Section)
                    .AsQueryable();

                if (fromDate.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.Date >= fromDate.Value);
                if (toDate.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.Date <= toDate.Value);
                if (filter.CategoryId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.CategoryId == filter.CategoryId);
                if (filter.GroupId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.GroupId == filter.GroupId);
                if (filter.StatusId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.StatusId == filter.StatusId);
                if (filter.ProductId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.ProductId == filter.ProductId);
                if (filter.WarehouseId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.WarehouseId == filter.WarehouseId);
                if (filter.ZoneId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.ZoneId == filter.ZoneId);
                if (filter.SectionId.HasValue) adjustmentQuery = adjustmentQuery.Where(t => t.SectionId == filter.SectionId);

                var adjustmentResults = await adjustmentQuery.Select(t => new InventoryTransactionReportDto
                {
                    Date = t.Date.ToString("yyyy/MM/dd"),
                    DocumentNumber = "-",
                    DocumentType = "InventoryAdjustment",
                    ConversionType = null,
                    CategoryName = t.Product.Status.Group.Category.Name,
                    GroupName = t.Product.Status.Group.Name,
                    StatusName = t.Product.Status.Name,
                    ProductName = t.Product.Name,
                    SourceWarehouseName = t.Warehouse.Name ?? "",
                    SourceDepartmentName = t.Zone.Name ?? "",
                    SourceSectionName = t.Section.Name ?? "",
                    DestinationWarehouseName = "",
                    DestinationDepartmentName = "",
                    DestinationSectionName = "",
                    Quantity = t.QuantityChange
                }).ToListAsync();

                results.AddRange(adjustmentResults);
            }


            //-----------------------------------
            // ۴. تراکنش‌های ایجاد کالا (Add Inventory)
            //-----------------------------------
            if (filter.DocumentType == "AddInventory" || string.IsNullOrEmpty(filter.DocumentType))
            {
                Console.WriteLine($"شروع فیلتر AddInventory. مقادیر ورودی: {System.Text.Json.JsonSerializer.Serialize(filter)}");

                var addItemQuery = _dbContext.InventoryReceiptLogs
                    .Include(l => l.Product)
                        .ThenInclude(p => p.Status)
                            .ThenInclude(s => s.Group)
                                .ThenInclude(g => g.Category)
                    .Include(l => l.Warehouse)
                    .Include(l => l.StorageZone)
                    .Include(l => l.StorageSection)
                    .AsQueryable();

                // فیلتر DocumentType
                addItemQuery = addItemQuery.Where(l => l.DocumentType == "AddInventory");

                Console.WriteLine($"بعد از فیلتر DocumentType تعداد رکوردها: {await addItemQuery.CountAsync()}");

                if (fromDate.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.CreatedAt >= fromDate.Value);
                    Console.WriteLine($"اعمال FromDate: {fromDate.Value}");
                }

                if (toDate.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.CreatedAt <= toDate.Value);
                    Console.WriteLine($"اعمال ToDate: {toDate.Value}");
                }

                if (filter.CategoryId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.Product.Status.Group.CategoryId == filter.CategoryId.Value);
                    Console.WriteLine($"اعمال CategoryId: {filter.CategoryId.Value}");
                }

                if (filter.GroupId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.Product.Status.GroupId == filter.GroupId.Value);
                    Console.WriteLine($"اعمال GroupId: {filter.GroupId.Value}");
                }

                if (filter.StatusId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.Product.StatusId == filter.StatusId.Value);
                    Console.WriteLine($"اعمال StatusId: {filter.StatusId.Value}");
                }

                if (filter.ProductId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.ProductId == filter.ProductId.Value);
                    Console.WriteLine($"اعمال ProductId: {filter.ProductId.Value}");
                }

                if (filter.WarehouseId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.WarehouseId == filter.WarehouseId.Value);
                    Console.WriteLine($"اعمال WarehouseId: {filter.WarehouseId.Value}");
                }

                if (filter.ZoneId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.ZoneId == filter.ZoneId.Value);
                    Console.WriteLine($"اعمال ZoneId: {filter.ZoneId.Value}");
                }

                if (filter.SectionId.HasValue)
                {
                    addItemQuery = addItemQuery.Where(l => l.SectionId == filter.SectionId.Value);
                    Console.WriteLine($"اعمال SectionId: {filter.SectionId.Value}");
                }

                var countBeforeSelect = await addItemQuery.CountAsync();
                Console.WriteLine($"تعداد رکوردها بعد از تمام فیلترها: {countBeforeSelect}");

                var addInventoryResults = await addItemQuery.Select(l => new InventoryTransactionReportDto
                {
                    Date = l.CreatedAt.ToString("yyyy/MM/dd"),
                    DocumentType = "AddInventory",
                    CategoryName = l.Product.Status.Group.Category.Name ?? "",
                    GroupName = l.Product.Status.Group.Name ?? "",
                    StatusName = l.Product.Status.Name ?? "",
                    ProductName = l.Product.Name,
                    DestinationWarehouseName = l.Warehouse.Name ?? "",
                    DestinationDepartmentName = l.StorageZone.Name ?? "",
                    DestinationSectionName = l.StorageSection.Name ?? "",
                    Quantity = l.Quantity
                }).ToListAsync();

                Console.WriteLine($"تعداد رکوردهای AddInventory نهایی: {addInventoryResults.Count}");

                results.AddRange(addInventoryResults);
            }

            //-----------------------------------
            // مرتب‌سازی نهایی
            //-----------------------------------
            return results
                .OrderBy(r => DateTime.ParseExact(r.Date, "yyyy/MM/dd", CultureInfo.InvariantCulture))
                .ToList();
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
                "InventoryAdjustment" => "اصلاح موجودی",
                "AddInventory" => "ایجاد کالا",
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