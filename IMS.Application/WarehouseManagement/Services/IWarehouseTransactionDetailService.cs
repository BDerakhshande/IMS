using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.DTOs;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IWarehouseTransactionDetailService
    {
        Task<List<WarehouseTransactionDetailDto>> GetAllTransactionsAsync(
           string? projectName = null,
           string? transactionType = null,
           CancellationToken cancellationToken = default);
    }
}
