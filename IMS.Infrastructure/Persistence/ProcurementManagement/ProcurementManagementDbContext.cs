using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProcurementManagement.Service;
using IMS.Domain.ProcurementManagement.Entities;
using IMS.Infrastructure.Persistence.ProjectManagement;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Persistence.ProcurementManagement
{
    public class ProcurementManagementDbContext : DbContext , IProcurementManagementDbContext
    {
        public ProcurementManagementDbContext(DbContextOptions<ProcurementManagementDbContext> options)
          : base(options)
        {
        }
        public DbSet<GoodsRequest> GoodsRequests { get; set; } = null!;
        public DbSet<GoodsRequestItem> GoodsRequestItems { get; set; } = null!;
        public DbSet<PurchaseRequest> PurchaseRequests { get; set; } = null!;
        public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; } = null!;
        public DbSet<Supplier> Supplier { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GoodsRequestItem>()
                .HasOne(g => g.GoodsRequest)
                .WithMany(r => r.Items)
                .HasForeignKey(g => g.GoodsRequestId)
                .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
