using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.Internal;
using ClosedXML.Excel;
using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.DTOs;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public class WarehouseTransactionDetailService : IWarehouseTransactionDetailService
    {
        private readonly IWarehouseDbContext _warehouseContext;
        private readonly IApplicationDbContext _projectContext;

        public WarehouseTransactionDetailService(
             IWarehouseDbContext warehouseContext,
             IApplicationDbContext projectContext)
        {
            _warehouseContext = warehouseContext;
            _projectContext = projectContext;
        }

        public async Task<(List<WarehouseTransactionDetailDto> Transactions, List<ProjectDto> Projects, List<string> TransactionTypes)>
    GetAllTransactionsWithProjectsAsync(
        string? projectName = null,
        string? transactionType = null,
        bool isSearchClicked = false,
        CancellationToken cancellationToken = default)
        {
            // ---------------- Fetch Projects (همیشه کامل) ----------------
            var allProjects = await _projectContext.Projects
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName
                })
                .ToListAsync(cancellationToken);
            // همه نوع تراکنش
            var allTransactionTypes = new List<string> { "Receipt", "Issue", "Conversion" , "Transfer"};

            if (!isSearchClicked)
                return (new List<WarehouseTransactionDetailDto>(), allProjects, allTransactionTypes);

            

            var receiptOrIssueItems = await _warehouseContext.ReceiptOrIssueItems
                .Include(x => x.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Include(x => x.SourceWarehouse)
                .Include(x => x.SourceZone)
                .Include(x => x.SourceSection)
                .Include(x => x.DestinationWarehouse)
                .Include(x => x.DestinationZone)
                .Include(x => x.DestinationSection)
                .Include(x => x.ReceiptOrIssue)
                .ToListAsync(cancellationToken);

            var consumedItems = await _warehouseContext.conversionConsumedItems
                .Include(x => x.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Include(x => x.Warehouse)
                .Include(x => x.Zone)
                .Include(x => x.Section)
                .Include(x => x.ConversionDocument)
                .ToListAsync(cancellationToken);

            var producedItems = await _warehouseContext.conversionProducedItems
                .Include(x => x.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Include(x => x.Warehouse)
                .Include(x => x.Zone)
                .Include(x => x.Section)
                .Include(x => x.ConversionDocument)
                .ToListAsync(cancellationToken);

            // ---------------- Map Transactions ----------------
            var receiptOrIssueTransactions = receiptOrIssueItems.Select(x => new WarehouseTransactionDetailDto
            {
                Id = x.Id,
                DocumentNumber = x.ReceiptOrIssue!.DocumentNumber,
                TransactionType = x.ReceiptOrIssue.Type.ToString(),
                ProjectName = !string.IsNullOrWhiteSpace(x.ProjectTitle)
                    ? x.ProjectTitle
                    : x.ProjectId.HasValue
                        ? allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName
                        : null,
                Date = x.ReceiptOrIssue.Date,
                SourceWarehouse = x.SourceWarehouse?.Name,
                SourceZone = x.SourceZone?.Name,
                SourceSection = x.SourceSection?.Name,
                DestinationWarehouse = x.DestinationWarehouse?.Name,
                DestinationZone = x.DestinationZone?.Name,
                DestinationSection = x.DestinationSection?.Name,
                ProductName = x.Product.Name,
                CategoryName = x.Category?.Name ?? x.Product.Status.Group.Category.Name,
                GroupName = x.Group?.Name ?? x.Product.Status.Group.Name,
                StatusName = x.Status?.Name ?? x.Product.Status.Name,
                Quantity = x.Quantity
            }).ToList();

            var consumedTransactions = consumedItems.Select(x => new WarehouseTransactionDetailDto
            {
                Id = x.Id,
                DocumentNumber = x.ConversionDocument.DocumentNumber,
                TransactionType = "Conversion",
                ProjectName = x.ProjectId.HasValue ? allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName : null,
                Date = x.ConversionDocument.CreatedAt,
                SourceWarehouse = x.Warehouse?.Name,
                SourceZone = x.Zone?.Name,
                SourceSection = x.Section?.Name,
                DestinationWarehouse = null,
                DestinationZone = null,
                DestinationSection = null,
                ProductName = x.Product?.Name,
                CategoryName = x.Category?.Name ?? x.Product?.Status?.Group?.Category?.Name,
                GroupName = x.Group?.Name ?? x.Product?.Status?.Group?.Name,
                StatusName = x.Status?.Name ?? x.Product?.Status?.Name,
                Quantity = x.Quantity
            }).ToList();

            var producedTransactions = producedItems.Select(x => new WarehouseTransactionDetailDto
            {
                Id = x.Id,
                DocumentNumber = x.ConversionDocument.DocumentNumber,
                TransactionType = "Conversion",
                ProjectName = x.ProjectId.HasValue ? allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName : null,
                Date = x.ConversionDocument.CreatedAt,
                SourceWarehouse = null,
                SourceZone = null,
                SourceSection = null,
                DestinationWarehouse = x.Warehouse?.Name,
                DestinationZone = x.Zone?.Name,
                DestinationSection = x.Section?.Name,
                ProductName = x.Product?.Name,
                CategoryName = x.Category?.Name ?? x.Product?.Status?.Group?.Category?.Name,
                GroupName = x.Group?.Name ?? x.Product?.Status?.Group?.Name,
                StatusName = x.Status?.Name ?? x.Product?.Status?.Name,
                Quantity = x.Quantity
            }).ToList();

            var conversionTransactions = consumedTransactions.Concat(producedTransactions).ToList();

            // ---------------- Apply Filters on Transactions Only ----------------
            var allTransactions = receiptOrIssueTransactions.Concat(conversionTransactions).ToList();

            if (!string.IsNullOrWhiteSpace(projectName) && projectName != "همه")
                allTransactions = allTransactions
                    .Where(t => !string.IsNullOrWhiteSpace(t.ProjectName) &&
                                t.ProjectName.Contains(projectName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(transactionType) && transactionType != "همه")
                allTransactions = allTransactions
                    .Where(t => t.TransactionType.Equals(transactionType, StringComparison.OrdinalIgnoreCase))
                    .ToList();


            var transactions = allTransactions.OrderByDescending(t => t.Date).ToList();

            return (transactions, allProjects , allTransactionTypes);
        }


        private static string ToShamsi(DateTime date)
        {
            var pc = new PersianCalendar();
            return $"{pc.GetYear(date):0000}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";
        }

        public async Task<byte[]> ExportTransactionsToExcelAsync(
         string? projectName = null,
         string? transactionType = null,
         CancellationToken cancellationToken = default)
        {
            // دریافت داده‌ها با فیلترهای اعمال شده
            var (transactions, _, _) = await GetAllTransactionsWithProjectsAsync(projectName, transactionType, true, cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("گزارش تراکنش‌های انبار");

            // هدر ستون‌ها
            worksheet.Cell(1, 1).Value = "ردیف";
            worksheet.Cell(1, 2).Value = "شماره سند";
            worksheet.Cell(1, 3).Value = "تاریخ";
            worksheet.Cell(1, 4).Value = "نوع تراکنش";
            worksheet.Cell(1, 5).Value = "نام پروژه";
            worksheet.Cell(1, 6).Value = "مبدا";
            worksheet.Cell(1, 7).Value = "مقصد";
            worksheet.Cell(1, 8).Value = "کالا";
            worksheet.Cell(1, 9).Value = "تعداد";

            // ترجمه نوع تراکنش به فارسی
            var transactionTypeNames = new Dictionary<string, string>
    {
        { "Conversion", "تبدیل" },
        { "Receipt", "رسید" },
        { "Issue", "حواله" },
        { "Transfer", "انتقال" }
    };

            int row = 2;
            for (int i = 0; i < transactions.Count; i++)
            {
                var item = transactions[i];

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = item.DocumentNumber;
                worksheet.Cell(row, 3).Value = item.Date;
                worksheet.Cell(row, 4).Value = transactionTypeNames.ContainsKey(item.TransactionType)
                                               ? transactionTypeNames[item.TransactionType]
                                               : item.TransactionType;
                worksheet.Cell(row, 5).Value = item.ProjectName;

                // مبدا
                var source = string.Join(" / ", new[] { item.SourceWarehouse, item.SourceZone, item.SourceSection }
                                        .Where(x => !string.IsNullOrWhiteSpace(x)));
                worksheet.Cell(row, 6).Value = string.IsNullOrEmpty(source) ? "-" : source;

                // مقصد
                var destination = string.Join(" / ", new[] { item.DestinationWarehouse, item.DestinationZone, item.DestinationSection }
                                             .Where(x => !string.IsNullOrWhiteSpace(x)));
                worksheet.Cell(row, 7).Value = string.IsNullOrEmpty(destination) ? "-" : destination;

                // کالا
                worksheet.Cell(row, 8).Value = $"{item.ProductName} / {item.CategoryName} / {item.GroupName} / {item.StatusName}";

                // تعداد
                worksheet.Cell(row, 9).Value = item.Quantity;

                row++;
            }
       

            // تنظیم خودکار عرض ستون‌ها
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
}

