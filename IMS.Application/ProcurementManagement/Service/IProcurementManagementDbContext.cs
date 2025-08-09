using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProcurementManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProcurementManagement.Service
{
    public interface IProcurementManagementDbContext
    {
         public DbSet<GoodsRequest> GoodsRequests { get; set; }
         public DbSet<GoodsRequestItem> GoodsRequestItems { get; set; }
         public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
         public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
