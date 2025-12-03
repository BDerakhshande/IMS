using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.WarehouseManagement.Services;
using IMS.Domain.WarehouseManagement.Entities;
using IMS.Domain.WarehouseManagement.Enums;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Persistence.WarehouseManagement
{
    public class WarehouseDbContext : DbContext, IWarehouseDbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
            : base(options)
        {
        }

        // --- DbSets ---
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<StorageZone> StorageZones { get; set; } = null!;
        public DbSet<StorageSection> StorageSections { get; set; } = null!;
        public DbSet<ProductItem> ProductItems { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;

        public DbSet<ReceiptOrIssue> ReceiptOrIssues { get; set; } = null!;
        public DbSet<ReceiptOrIssueItem> ReceiptOrIssueItems { get; set; } = null!;
        public DbSet<ReceiptOrIssueItemUniqueCode> ReceiptOrIssueItemUniqueCodes { get; set; } = null!;

        public DbSet<ConversionDocument> conversionDocuments { get; set; } = null!;
        public DbSet<ConversionConsumedItem> conversionConsumedItems { get; set; } = null!;
        public DbSet<ConversionConsumedItemUniqueCode> ConversionConsumedItemUniqueCodes { get; set; } = null!;
        public DbSet<ConversionProducedItem> conversionProducedItems { get; set; } = null!;
        public DbSet<ConversionProducedItemUniqueCode> ConversionProducedItemUniqueCodes { get; set; } = null!;

        public DbSet<Unit> Units { get; set; } = null!;
        public DbSet<InventoryReceiptLog> InventoryReceiptLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // ReceiptOrIssueItemUniqueCode
            // ============================
            modelBuilder.Entity<ReceiptOrIssueItemUniqueCode>()
                .ToTable("ReceiptOrIssueItemUniqueCode");

            // ============================
            // InventoryReceiptLog
            // ============================
            modelBuilder.Entity<InventoryReceiptLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.StorageZone)
                      .WithMany()
                      .HasForeignKey(e => e.ZoneId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.StorageSection)
                      .WithMany()
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_InventoryReceiptLogs_StorageSections_SectionId");

                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // ConversionConsumedItemUniqueCode
            // ============================
            modelBuilder.Entity<ConversionConsumedItemUniqueCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.ConversionConsumedItem)
                      .WithMany(c => c.UniqueCodes) // ✅ navigation property درست
                      .HasForeignKey(e => e.ConversionConsumedItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.InventoryItem)
                      .WithMany()
                      .HasForeignKey(e => e.InventoryItemId)
                      .OnDelete(DeleteBehavior.Cascade);

            });


            // ============================
            // ConversionProducedItemUniqueCode
            // ============================
            modelBuilder.Entity<ConversionProducedItemUniqueCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UniqueCode)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasOne(e => e.ConversionProducedItem)
                      .WithMany(c => c.UniqueCodes) // ✅ navigation property درست
                      .HasForeignKey(e => e.ConversionProducedItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // ============================
            // ConversionConsumedItem
            // ============================
            modelBuilder.Entity<ConversionConsumedItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Zone)
                      .WithMany()
                      .HasForeignKey(e => e.ZoneId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Section)
                      .WithMany()
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Group)
                      .WithMany()
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Status)
                      .WithMany()
                      .HasForeignKey(e => e.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Project)
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ConversionDocument)
                      .WithMany(d => d.ConsumedItems)
                      .HasForeignKey(e => e.ConversionDocumentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // ConversionProducedItem
            // ============================
            modelBuilder.Entity<ConversionProducedItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Warehouse)
                      .WithMany()
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Zone)
                      .WithMany()
                      .HasForeignKey(e => e.ZoneId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Section)
                      .WithMany()
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Group)
                      .WithMany()
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Status)
                      .WithMany()
                      .HasForeignKey(e => e.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Project)
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ConversionDocument)
                      .WithMany(d => d.ProducedItems)
                      .HasForeignKey(e => e.ConversionDocumentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // ConversionDocument
            // ============================
            modelBuilder.Entity<ConversionDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DocumentNumber)
                      .IsRequired();
            });

            // ============================
            // InventoryTransaction
            // ============================
            modelBuilder.Entity<InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(t => t.Product).WithMany().HasForeignKey(t => t.ProductId);
                entity.HasOne(t => t.Category).WithMany().HasForeignKey(t => t.CategoryId);
                entity.HasOne(t => t.Group).WithMany().HasForeignKey(t => t.GroupId);
                entity.HasOne(t => t.Status).WithMany().HasForeignKey(t => t.StatusId);
                entity.HasOne(t => t.Warehouse).WithMany().HasForeignKey(t => t.WarehouseId);
                entity.HasOne(t => t.Zone).WithMany().HasForeignKey(t => t.ZoneId);
                entity.HasOne(t => t.Section).WithMany().HasForeignKey(t => t.SectionId);
            });

            // ============================
            // Inventory
            // ============================
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventories");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Quantity)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                // Warehouse
                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.Inventories) // اگر میخوای collection داشته باشی
                      .HasForeignKey(e => e.WarehouseId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Zone
                entity.HasOne(e => e.Zone)
                      .WithMany(z => z.Inventories) // collection اضافه کن در StorageZone
                      .HasForeignKey(e => e.ZoneId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Section
                entity.HasOne(e => e.Section)
                      .WithMany(s => s.Inventories) // collection اضافه کن در StorageSection
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Product
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Inventories)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Index یکتا
                entity.HasIndex(e => new { e.WarehouseId, e.ZoneId, e.SectionId, e.ProductId })
                      .IsUnique();
            });


            // ============================
            // Warehouse
            // ============================
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.Manager).HasMaxLength(200);
                entity.Property(e => e.StorageConditions).HasMaxLength(300);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasMany(e => e.Zones)
                      .WithOne(z => z.Warehouse)
                      .HasForeignKey(z => z.WarehouseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================
            // StorageZone
            // ============================
            modelBuilder.Entity<StorageZone>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ZoneCode).IsRequired().HasMaxLength(50);

                entity.HasMany(z => z.Sections)
                      .WithOne(s => s.Zone)
                      .HasForeignKey(s => s.ZoneId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(z => new { z.WarehouseId, z.ZoneCode }).IsUnique();
            });

            // ============================
            // StorageSection
            // ============================
            modelBuilder.Entity<StorageSection>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SectionCode).IsRequired().HasMaxLength(50);

                entity.Property(e => e.Dimensions).HasMaxLength(300);

                entity.HasIndex(e => new { e.ZoneId, e.SectionCode }).IsUnique();
            });

            // ============================
            // Category / Group / Status
            // ============================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

                entity.HasMany(e => e.Groups)
                      .WithOne(g => g.Category)
                      .HasForeignKey(g => g.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

                entity.HasMany(g => g.Statuses)
                      .WithOne(s => s.Group)
                      .HasForeignKey(s => s.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // ============================
            // Product
            // ============================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)");

                entity.HasIndex(e => new { e.Code, e.StatusId }).IsUnique();

                entity.HasOne(e => e.Status)
                      .WithMany(s => s.Products)
                      .HasForeignKey(e => e.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Unit)
                      .WithMany(u => u.Products)
                      .HasForeignKey(e => e.UnitId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================
            // Units Seed
            // ============================
            modelBuilder.Entity<Unit>().HasData(
               new Unit { Id = 1, Name = "عدد", Symbol = "pcs" },
               new Unit { Id = 2, Name = "کیلوگرم", Symbol = "kg" },
               new Unit { Id = 3, Name = "متر", Symbol = "m" }
            );
        }

    }
}