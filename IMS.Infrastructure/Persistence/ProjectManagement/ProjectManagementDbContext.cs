using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.Service;
using IMS.Domain.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Persistence.ProjectManagement
{
    public class ProjectManagementDbContext : DbContext , IApplicationDbContext
    {
        public ProjectManagementDbContext(DbContextOptions<ProjectManagementDbContext> options)
           : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Employer> Employers { get; set; } = null!;
        public DbSet<ProjectType> ProjectTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Project>()
                .ToTable("Projects", "dbo");

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Employer)
                .WithMany()
                .HasForeignKey(p => p.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.ProjectType)
                .WithMany()
                .HasForeignKey(p => p.ProjectTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
