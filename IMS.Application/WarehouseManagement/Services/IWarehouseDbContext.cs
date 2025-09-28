using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.WarehouseManagement.Services
{
    public interface IWarehouseDbContext
    {
        public DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbSet<Warehouse> Warehouses { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Group> Groups { get; set; }
        DbSet<Status> Statuses { get; set; }
        DbSet<ReceiptOrIssue> ReceiptOrIssues { get; set; }
        DbSet<ReceiptOrIssueItem> ReceiptOrIssueItems  { get; set; }
        DbSet<ConversionConsumedItem> conversionConsumedItems  { get; set; }
        DbSet<ConversionProducedItem> conversionProducedItems  { get; set; }
        DbSet<ConversionDocument> conversionDocuments { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<Inventory> Inventories { get; set; }
        public DbSet<StorageZone> StorageZones { get; }
        public DbSet<StorageSection> StorageSections { get;  } 
        public DbSet<ProductItem> ProductItems { get;  } 

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
