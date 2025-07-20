using IMS.Areas.AccountManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.AccountManagement.Data
{
    public class AccountManagementDbContext : DbContext
    {
        public AccountManagementDbContext(DbContextOptions<AccountManagementDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CostCenter> CostCenters { get; set; }

        public DbSet<Moein> Moeins { get; set; }
        public DbSet<Tafzil> Tafzils { get; set; }
        public DbSet<SecondTafzil> SecondTafzils { get; set; }
        public DbSet<TransactionDocument> TransactionDocuments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // تنظیم روابط برای جلوگیری از مشکلات Cascade Delete
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Moein)
                .WithMany(m => m.Transactions)
                .HasForeignKey(t => t.MoeinId)
                .OnDelete(DeleteBehavior.Restrict);  // یا DeleteBehavior.NoAction

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);  // یا DeleteBehavior.NoAction

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.TransactionDocument)
                .WithMany(td => td.Transactions)
                .HasForeignKey(t => t.TransactionDocumentId)
                .OnDelete(DeleteBehavior.Restrict);  // یا DeleteBehavior.NoAction

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.CostCenter)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);  // یا DeleteBehavior.NoAction

           

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Tafzil)
                .WithMany(tf => tf.Transactions)
                .HasForeignKey(t => t.TafzilId)
                .OnDelete(DeleteBehavior.Restrict);  // یا DeleteBehavior.NoAction
        }



    }
}
