using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProjectManagement.Service
{
    public interface IApplicationDbContext
    {
        DbSet<Employer> Employers { get; }
        DbSet<Project> Projects { get; }
        public DbSet<ProjectType> ProjectTypes { get;  }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
