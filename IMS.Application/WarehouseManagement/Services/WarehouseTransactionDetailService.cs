using System;
using System.Collections.Generic;
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
    public class WarehouseTransactionDetailService:IWarehouseTransactionDetailService
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

        public async Task<(List<WarehouseTransactionDetailDto> Transactions, List<ProjectDto> Projects)> GetAllTransactionsWithProjectsAsync(
            string? projectName = null,
            string? transactionType = null,
            CancellationToken cancellationToken = default)
        {
            // ---------------- Fetch Projects ----------------
            var allProjects = await _projectContext.Projects
                .Select(p => new { p.Id, p.ProjectName })
                .ToListAsync(cancellationToken);

            // ---------------- ReceiptOrIssue Items ----------------
            var receiptOrIssueQuery = _warehouseContext.ReceiptOrIssueItems
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
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(projectName))
            {
                var projectNameLower = projectName.ToLower();
                receiptOrIssueQuery = receiptOrIssueQuery.Where(x =>
                    !string.IsNullOrWhiteSpace(x.ProjectTitle) &&
                    x.ProjectTitle.ToLower().Contains(projectNameLower));
            }

            if (!string.IsNullOrWhiteSpace(transactionType) && !transactionType.Equals("Conversion", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<ReceiptOrIssueType>(transactionType, true, out var typeEnum))
                {
                    receiptOrIssueQuery = receiptOrIssueQuery
                        .Where(x => x.ReceiptOrIssue != null && x.ReceiptOrIssue.Type == typeEnum);
                }
            }

            var receiptOrIssueTransactions = await receiptOrIssueQuery
                .Select(x => new WarehouseTransactionDetailDto
                {
                    Id = x.Id,
                    DocumentNumber = x.ReceiptOrIssue!.DocumentNumber,
                    TransactionType = x.ReceiptOrIssue.Type.ToString(),
                    ProjectName = x.ProjectTitle,
                    Date = x.ReceiptOrIssue.Date,
                    SourceWarehouse = x.SourceWarehouse.Name,
                    SourceZone = x.SourceZone.Name,
                    SourceSection = x.SourceSection.Name,
                    DestinationWarehouse = x.DestinationWarehouse.Name,
                    DestinationZone = x.DestinationZone.Name,
                    DestinationSection = x.DestinationSection.Name,
                    ProductName = x.Product.Name,
                    CategoryName = x.Category.Name ?? x.Product.Status.Group.Category.Name,
                    GroupName = x.Group.Name ?? x.Product.Status.Group.Name,
                    StatusName = x.Status.Name ?? x.Product.Status.Name,
                    Quantity = x.Quantity
                })
                .ToListAsync(cancellationToken);

            // ---------------- Consumed Items ----------------
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

            var consumedTransactions = consumedItems
                .Where(x => string.IsNullOrWhiteSpace(projectName) ||
                            (x.ProjectId.HasValue && allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName.Contains(projectName, StringComparison.OrdinalIgnoreCase) == true))
                .Select(x =>
                {
                    var projectNameMapped = x.ProjectId.HasValue
                        ? allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName
                        : null;

                    return new WarehouseTransactionDetailDto
                    {
                        Id = x.Id,
                        DocumentNumber = x.ConversionDocument.DocumentNumber,
                        TransactionType = "Conversion",
                        ProjectName = projectNameMapped,
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
                    };
                });

            // ---------------- Produced Items ----------------
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

            var producedTransactions = producedItems
                .Where(x => string.IsNullOrWhiteSpace(projectName) ||
                            (x.ProjectId.HasValue && allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName.Contains(projectName, StringComparison.OrdinalIgnoreCase) == true))
                .Select(x =>
                {
                    var projectNameMapped = x.ProjectId.HasValue
                        ? allProjects.FirstOrDefault(p => p.Id == x.ProjectId.Value)?.ProjectName
                        : null;

                    return new WarehouseTransactionDetailDto
                    {
                        Id = x.Id,
                        DocumentNumber = x.ConversionDocument.DocumentNumber,
                        TransactionType = "Conversion",
                        ProjectName = projectNameMapped,
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
                    };
                });

            // ---------------- Combine Conversion ----------------
            var conversionTransactions = consumedTransactions
                .Concat(producedTransactions)
                .ToList();

            // ---------------- Apply TransactionType Filter ----------------
            List<WarehouseTransactionDetailDto> transactions;
            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                if (transactionType.Equals("Conversion", StringComparison.OrdinalIgnoreCase))
                {
                    transactions = conversionTransactions
                        .OrderByDescending(t => t.Date)
                        .ToList();
                }
                else
                {
                    transactions = receiptOrIssueTransactions
                        .Where(t => t.TransactionType.Equals(transactionType, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(t => t.Date)
                        .ToList();
                }
            }
            else
            {
                transactions = receiptOrIssueTransactions
                    .Concat(conversionTransactions)
                    .OrderByDescending(t => t.Date)
                    .ToList();
            }

            // ---------------- Fetch Projects for Filter ----------------
            var projects = allProjects
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    ProjectName = p.ProjectName
                })
                .ToList();

            return (transactions, projects);
        }

        public async Task<byte[]> ExportTransactionsToExcelAsync(string? projectName = null, string? transactionType = null)
        {
            // دریافت داده‌ها با فیلترهای اعمال شده
            var (transactions, _) = await GetAllTransactionsWithProjectsAsync(projectName, transactionType);

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
                worksheet.Cell(row, 3).Value = item.Date.ToString("yyyy/MM/dd");
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

                worksheet.Cell(row, 8).Value = $"{item.ProductName} / {item.CategoryName} / {item.GroupName} / {item.StatusName}";

                row++;
            }

            // جمع کل
            var totalQuantity = transactions.Sum(x => x.Quantity);
            worksheet.Cell(row, 7).Value = "جمع کل:";
            worksheet.Cell(row, 8).Value = totalQuantity;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

