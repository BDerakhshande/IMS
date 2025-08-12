using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.DTOs;
using IMS.Domain.ProcurementManagement.Entities;

namespace IMS.Application.ProcurementManagement.Service
{
    public interface IPurchaseRequestTrackingService
    {
        Task<List<PurchaseRequest>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<PurchaseRequestItemDto>> GetItemsWithStockAndNeedAsync(int purchaseRequestId, CancellationToken cancellationToken = default);
    }

}
