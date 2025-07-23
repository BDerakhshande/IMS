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
    public class WarehouseDbContext : DbContext , IWarehouseDbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
    : base(options)
        {
        }


        // DbSet ها
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<StorageZone> StorageZones { get; set; } = null!;
        public DbSet<StorageSection> StorageSections { get; set; } = null!;
      
        public DbSet<Inventory> Inventories { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;


        public DbSet<ReceiptOrIssue> ReceiptOrIssues { get; set; }
        public DbSet<ReceiptOrIssueItem> ReceiptOrIssueItems { get; set; }
        public DbSet<ConversionConsumedItem> conversionConsumedItems { get; set; }
        public DbSet<ConversionProducedItem> conversionProducedItems { get; set; }
        public DbSet<ConversionDocument> conversionDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- ConversionConsumedItem ---
            modelBuilder.Entity<ConversionConsumedItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConversionConsumedItem>()
                .HasOne(ci => ci.Zone)
                .WithMany()
                .HasForeignKey(ci => ci.ZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConversionConsumedItem>()
                .HasOne(ci => ci.Section)
                .WithMany()
                .HasForeignKey(ci => ci.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConversionConsumedItem>()
                .HasOne(ci => ci.ConversionDocument)
                .WithMany(d => d.ConsumedItems)
                .HasForeignKey(ci => ci.ConversionDocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- ConversionProducedItem ---
            modelBuilder.Entity<ConversionProducedItem>()
                .HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);




            modelBuilder.Entity<ConversionDocument>()
    .HasOne(cd => cd.Project)
    .WithMany()
    .HasForeignKey(cd => cd.ProjectId)
    .OnDelete(DeleteBehavior.Restrict);



            // --- Inventory ---
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventories");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Quantity)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Warehouse)
                    .WithMany(w => w.Inventories)
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Zone)
                    .WithMany(z => z.Inventories)
                    .HasForeignKey(e => e.ZoneId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Section)
                    .WithMany(s => s.Inventories)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Inventories)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => new { e.WarehouseId, e.ZoneId, e.SectionId, e.ProductId })
                    .IsUnique();
            });

            // --- Warehouse ---
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Code)
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Location)
                    .HasMaxLength(500);

                entity.Property(e => e.Manager)
                    .HasMaxLength(200);

                entity.Property(e => e.StorageConditions)
                    .HasMaxLength(300);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasMany(e => e.Zones)
                    .WithOne(z => z.Warehouse)
                    .HasForeignKey(z => z.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade); // حذف cascade برای Zones از Warehouse صحیح است
            });

            modelBuilder.Entity<StorageZone>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ZoneCode)
                    .IsRequired()
                    .HasMaxLength(50);

                // حذف ایندکس unique روی فقط ZoneCode

                entity.HasMany(z => z.Sections)
                    .WithOne(s => s.Zone)
                    .HasForeignKey(s => s.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StorageZone>()
                .HasIndex(z => new { z.WarehouseId, z.ZoneCode })
                .IsUnique();


            modelBuilder.Entity<StorageSection>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SectionCode)
                    .IsRequired()
                    .HasMaxLength(50);

                

                entity.Property(e => e.Capacity)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Dimensions)
                    .HasMaxLength(300);

                // ✅ ایندکس یونیک ترکیبی درست
                entity.HasIndex(e => new { e.ZoneId, e.SectionCode }).IsUnique();
            });


            // --- Category ---
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasMany(e => e.Groups)
                    .WithOne(g => g.Category)
                    .HasForeignKey(g => g.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Group ---
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(g => g.Category)
                    .WithMany(c => c.Groups)
                    .HasForeignKey(g => g.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(g => g.Statuses)
                    .WithOne(s => s.Group)
                    .HasForeignKey(s => s.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Status ---
            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(s => s.Group)
                    .WithMany(g => g.Statuses)
                    .HasForeignKey(s => s.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.Products)
                    .WithOne(p => p.Status)
                    .HasForeignKey(p => p.StatusId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- ReceiptOrIssue ---
            modelBuilder.Entity<ReceiptOrIssue>()
     .HasOne(r => r.Project)
     .WithMany()
     .HasForeignKey(r => r.ProjectId)
     .OnDelete(DeleteBehavior.Restrict);


            // --- Product ---
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Code)
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Code)
                    .IsUnique();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(p => p.Status)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }


    }
}