using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.ProcurementManagement.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public class PurchaseRequestFlatItemService : IPurchaseRequestFlatItemService
    {
        private readonly IProcurementManagementDbContext _procurementContext;
        private readonly IApplicationDbContext _projectContext;
        private readonly IWarehouseDbContext _warehouseContext;

        public PurchaseRequestFlatItemService(
            IProcurementManagementDbContext procurementContext,
            IApplicationDbContext projectContext,
            IWarehouseDbContext warehouseContext)
        {
            _procurementContext = procurementContext;
            _projectContext = projectContext;
            _warehouseContext = warehouseContext;
        }

        public async Task<List<SelectListItem>> GetAllCategoriesAsync()
        {
            return await _warehouseContext.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGroupsByCategoryIdAsync(int categoryId)
        {
            return await _warehouseContext.Groups
                .Where(g => g.CategoryId == categoryId)
                .OrderBy(g => g.Name)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetStatusesByGroupIdAsync(int groupId)
        {
            return await _warehouseContext.Statuses
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetProductsByStatusIdAsync(int statusId)
        {
            return await _warehouseContext.Products
                .Where(p => p.StatusId == statusId)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }



        #region Get Flat Items
        public async Task<List<PurchaseRequestFlatItemDto>> GetFlatItemsAsync(
            string? requestNumber = null,
            string? requestTitle = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? requestTypeId = null,
            int? projectId = null,
            List<ProductFilterDto>? products = null,
            CancellationToken cancellationToken = default)
        {
            // مرحله 1: اعمال فیلترهای ساده روی دیتابیس
            var query = _procurementContext.PurchaseRequestFlatItems.AsQueryable();

            if (!string.IsNullOrEmpty(requestNumber))
                query = query.Where(x => x.RequestNumber.Contains(requestNumber));

            if (!string.IsNullOrEmpty(requestTitle))
                query = query.Where(x => x.RequestTitle.Contains(requestTitle));

            if (fromDate.HasValue)
                query = query.Where(x => x.RequestDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.RequestDate <= toDate.Value);

            if (requestTypeId.HasValue)
                query = query.Where(x => x.RequestTypeId == requestTypeId.Value);

            if (projectId.HasValue)
                query = query.Where(x => x.ProjectId == projectId.Value);

            // دریافت داده‌ها از دیتابیس
            var flatItemsList = await query
                .OrderByDescending(x => x.RequestDate)
                .ToListAsync(cancellationToken);

            // اعمال فیلتر سلسله‌مراتبی محصولات در حافظه
            if (products != null && products.Any())
            {
                var conditions = products
                    .Where(p => p.CategoryId.HasValue || p.GroupId.HasValue || p.StatusId.HasValue || p.ProductId.HasValue)
                    .ToList();

                if (conditions.Any())
                {
                    flatItemsList = flatItemsList
                        .Where(x => conditions.Any(p =>
                            (!p.CategoryId.HasValue || x.CategoryId == p.CategoryId) &&
                            (!p.GroupId.HasValue || x.GroupId == p.GroupId) &&
                            (!p.StatusId.HasValue || x.StatusId == p.StatusId) &&
                            (!p.ProductId.HasValue || x.ProductId == p.ProductId)
                        ))
                        .ToList();
                }
            }

            // جمع‌آوری productIds برای محاسبه موجودی و درخواست‌های معلق
            var productIds = flatItemsList.Select(x => x.ProductId).Distinct().ToList();

            // موجودی کل محصولات در انبار مرکزی
            var stocksDict = await _warehouseContext.Inventories
                .Where(inv => productIds.Contains(inv.ProductId) && inv.Warehouse.Name.Contains("مرکزی"))
                .GroupBy(inv => inv.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);

            // درخواست‌های معلق برای محصولات
            var pendingDict = await _procurementContext.PurchaseRequestItems
                .Where(i => productIds.Contains(i.ProductId) && i.PurchaseRequest.Status != Status.Completed && !i.IsSupplyStopped)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, TotalPendingQuantity = g.Sum(x => x.RemainingQuantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalPendingQuantity, cancellationToken);

            var result = new List<PurchaseRequestFlatItemDto>();

            // حلقه امن روی کپی لیست برای حذف آیتم‌ها در صورت نیاز
            foreach (var x in flatItemsList.ToList())
            {
                stocksDict.TryGetValue(x.ProductId, out decimal totalStock);
                pendingDict.TryGetValue(x.ProductId, out decimal totalPending);

                var needToSupply = Math.Max(0, x.Quantity - totalStock);

                // حذف آیتم‌هایی که نیاز به تامین صفر دارند و تامین متوقف نشده
                if (needToSupply == 0 && !x.IsSupplyStopped)
                {
                    _procurementContext.PurchaseRequestFlatItems.Remove(x);
                    continue; // از افزودن به خروجی جلوگیری شود
                }

                result.Add(new PurchaseRequestFlatItemDto
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    RequestTitle = x.RequestTitle,
                    RequestDate = x.RequestDate,
                    RequestTypeId = x.RequestTypeId,
                    RequestTypeName = x.RequestTypeName,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    GroupId = x.GroupId,
                    GroupName = x.GroupName,
                    StatusId = x.StatusId,
                    StatusName = x.StatusName,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    TotalStock = totalStock,
                    PendingRequests = totalPending,
                    NeedToSupply = needToSupply,
                    IsSupplyStopped = x.IsSupplyStopped
                });
            }

            // ذخیره تغییرات حذف شده‌ها
            await _procurementContext.SaveChangesAsync(cancellationToken);

            return result;
        }
        #endregion

        public async Task<byte[]> ExportFlatItemsToExcelAsync(
            string? requestNumber = null,
            string? requestTitle = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? requestTypeId = null,
            int? projectId = null,
            List<ProductFilterDto>? products = null,
            CancellationToken cancellationToken = default)
        {
            var items = await GetFlatItemsAsync(requestNumber, requestTitle, fromDate, toDate, requestTypeId, projectId, products, cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Purchase Requests");

            // تنظیم هدر
            worksheet.Cell(1, 1).Value = "شماره درخواست";
            worksheet.Cell(1, 2).Value = "عنوان درخواست";
            worksheet.Cell(1, 3).Value = "نوع درخواست";
            worksheet.Cell(1, 4).Value = "تاریخ درخواست";
            worksheet.Cell(1, 5).Value = "پروژه";
            worksheet.Cell(1, 6).Value = "محصول";
            worksheet.Cell(1, 7).Value = "نیاز به تامین";

            // استایل هدر
            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(26, 74, 90); // #1a4a5a
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // درج داده‌ها
            int row = 2;
            foreach (var item in items)
            {
                worksheet.Cell(row, 1).Value = item.RequestNumber;
                worksheet.Cell(row, 2).Value = item.RequestTitle;
                worksheet.Cell(row, 3).Value = item.RequestTypeName;
                worksheet.Cell(row, 4).Value = item.RequestDate.ToString("yyyy/MM/dd");
                worksheet.Cell(row, 5).Value = item.ProjectName;
                worksheet.Cell(row, 6).Value = $"{item.CategoryName} | {item.GroupName} | {item.StatusName} | {item.ProductName}";
                worksheet.Cell(row, 7).Value = item.NeedToSupply;
                row++;
            }

            // Auto-fit ستون‌ها
            worksheet.Columns().AdjustToContents();

            // فونت فارسی
            worksheet.Style.Font.FontName = "B Nazanin";
            worksheet.Style.Font.FontSize = 12;

            // خروجی به بایت آرایه
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


    }
}


