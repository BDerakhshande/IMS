using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.DTOs;
using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IWarehouseTransactionDetailService
    {


        Task<(List<WarehouseTransactionDetailDto> Transactions, List<ProjectDto> Projects, List<string> TransactionTypes)>
    GetAllTransactionsWithProjectsAsync(
        string? projectName = null,
        string? transactionType = null,
        bool isSearchClicked = false,
        CancellationToken cancellationToken = default);
        Task<byte[]> ExportTransactionsToExcelAsync(
                 string? projectName = null,
                 string? transactionType = null,
                 CancellationToken cancellationToken = default);
    }
}
