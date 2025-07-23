using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<List<WarehouseTransactionDetailDto>> GetAllTransactionsAsync(
           string? projectName = null,
           string? transactionType = null,
           CancellationToken cancellationToken = default)
        {
            // ابتدا پروژه‌ها را از DbContext پروژه‌ها بگیر
            var projects = await _projectContext.Projects.ToListAsync(cancellationToken);

            #region Query ReceiptOrIssueItems (داده‌های انبار)

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

            var receiptOrIssueItems = await receiptOrIssueQuery.ToListAsync(cancellationToken);

            // فیلتر روی پروژه و نوع تراکنش در حافظه چون join مستقیم امکان پذیر نیست
            var filteredReceiptOrIssueItems = receiptOrIssueItems
                .Where(x =>
                    (string.IsNullOrWhiteSpace(projectName) ||
                        (x.ReceiptOrIssue != null
                         && projects.Any(p => p.Id == x.ReceiptOrIssue.ProjectId
                                              && p.ProjectName.Contains(projectName))))
                    &&
                    (string.IsNullOrWhiteSpace(transactionType) ||
                        (x.ReceiptOrIssue != null
                         && x.ReceiptOrIssue.Type.ToString().Contains(transactionType, StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            var receiptOrIssueTransactions = filteredReceiptOrIssueItems
                .Select(x =>
                {
                    var project = projects.FirstOrDefault(p => p.Id == x.ReceiptOrIssue.ProjectId);
                    return new WarehouseTransactionDetailDto
                    {
                        Id = x.Id,
                        DocumentNumber = x.ReceiptOrIssue?.DocumentNumber,
                        TransactionType = x.ReceiptOrIssue?.Type.ToString(),
                        ProjectName = project?.ProjectName,
                        Date = x.ReceiptOrIssue?.Date ?? DateTime.MinValue,
                        SourceWarehouse = x.ReceiptOrIssue?.Type == ReceiptOrIssueType.Issue ? x.SourceWarehouse?.Name : null,
                        SourceZone = x.ReceiptOrIssue?.Type == ReceiptOrIssueType.Issue ? x.SourceZone?.Name : null,
                        SourceSection = x.ReceiptOrIssue?.Type == ReceiptOrIssueType.Issue ? x.SourceSection?.Name : null,
                        DestinationWarehouse = x.ReceiptOrIssue?.Type == ReceiptOrIssueType.Receipt ? x.DestinationWarehouse?.Name : null,
                        DestinationZone = x.ReceiptOrIssue?.Type == ReceiptOrIssueType.Receipt ? x.DestinationZone?.Name : null,
                        DestinationSection = x.ReceiptOrIssue?.Type == ReceiptOrIssueType.Receipt ? x.DestinationSection?.Name : null,
                        ProductName = x.Product?.Name,
                        CategoryName = x.Product?.Status?.Group?.Category?.Name,
                        GroupName = x.Product?.Status?.Group?.Name,
                        StatusName = x.Product?.Status?.Name,
                        Quantity = x.Quantity
                    };
                })
                .ToList();

            #endregion

            #region Query conversionConsumedItems (مصرف)

            var queryConsumed = _warehouseContext.conversionConsumedItems
                .Include(x => x.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Include(x => x.Warehouse)
                .Include(x => x.Zone)
                .Include(x => x.Section)
                .Include(x => x.ConversionDocument)
                .AsQueryable();

            var consumedItems = await queryConsumed.ToListAsync(cancellationToken);

            // فیلتر پروژه و نوع تراکنش در حافظه
            var filteredConsumed = consumedItems
                .Where(x =>
                    (string.IsNullOrWhiteSpace(projectName) ||
                        (x.ConversionDocument != null
                         && projects.Any(p => p.Id == x.ConversionDocument.ProjectId
                                              && p.ProjectName.Contains(projectName))))
                    &&
                    (string.IsNullOrWhiteSpace(transactionType) ||
                        transactionType.Equals("Consumption", StringComparison.OrdinalIgnoreCase))
                )
                .ToList();

            var consumedTransactions = filteredConsumed
                .Select(x =>
                {
                    var project = projects.FirstOrDefault(p => p.Id == x.ConversionDocument.ProjectId);
                    return new WarehouseTransactionDetailDto
                    {
                        Id = x.Id,
                        DocumentNumber = x.ConversionDocument?.DocumentNumber,
                        TransactionType = "Consumption",
                        ProjectName = project?.ProjectName,
                        Date = x.ConversionDocument?.CreatedAt ?? DateTime.MinValue,
                        SourceWarehouse = x.Warehouse?.Name,
                        SourceZone = x.Zone?.Name,
                        SourceSection = x.Section?.Name,
                        DestinationWarehouse = null,
                        DestinationZone = null,
                        DestinationSection = null,
                        ProductName = x.Product?.Name,
                        CategoryName = x.Product?.Status?.Group?.Category?.Name,
                        GroupName = x.Product?.Status?.Group?.Name,
                        StatusName = x.Product?.Status?.Name,
                        Quantity = x.Quantity
                    };
                })
                .ToList();

            #endregion

            #region Query conversionProducedItems (تولید)

            var queryProduced = _warehouseContext.conversionProducedItems
                .Include(x => x.Product)
                    .ThenInclude(p => p.Status)
                        .ThenInclude(s => s.Group)
                            .ThenInclude(g => g.Category)
                .Include(x => x.Warehouse)
                .Include(x => x.Zone)
                .Include(x => x.Section)
                .Include(x => x.ConversionDocument)
                .AsQueryable();

            var producedItems = await queryProduced.ToListAsync(cancellationToken);

            var filteredProduced = producedItems
                .Where(x =>
                    (string.IsNullOrWhiteSpace(projectName) ||
                        (x.ConversionDocument != null
                         && projects.Any(p => p.Id == x.ConversionDocument.ProjectId
                                              && p.ProjectName.Contains(projectName))))
                    &&
                    (string.IsNullOrWhiteSpace(transactionType) ||
                        transactionType.Equals("Production", StringComparison.OrdinalIgnoreCase))
                )
                .ToList();

            var producedTransactions = filteredProduced
                .Select(x =>
                {
                    var project = projects.FirstOrDefault(p => p.Id == x.ConversionDocument.ProjectId);
                    return new WarehouseTransactionDetailDto
                    {
                        Id = x.Id,
                        DocumentNumber = x.ConversionDocument?.DocumentNumber,
                        TransactionType = "تبدیل",
                        ProjectName = project?.ProjectName,
                        Date = x.ConversionDocument?.CreatedAt ?? DateTime.MinValue,
                        SourceWarehouse = null,
                        SourceZone = null,
                        SourceSection = null,
                        DestinationWarehouse = x.Warehouse?.Name,
                        DestinationZone = x.Zone?.Name,
                        DestinationSection = x.Section?.Name,
                        ProductName = x.Product?.Name,
                        CategoryName = x.Product?.Status?.Group?.Category?.Name,
                        GroupName = x.Product?.Status?.Group?.Name,
                        StatusName = x.Product?.Status?.Name,
                        Quantity = x.Quantity
                    };
                })
                .ToList();

            #endregion

            // ترکیب و مرتب‌سازی نهایی:
            return receiptOrIssueTransactions
                .Concat(consumedTransactions)
                .Concat(producedTransactions)
                .OrderByDescending(t => t.Date)
                .ToList();
        }


    }
}

