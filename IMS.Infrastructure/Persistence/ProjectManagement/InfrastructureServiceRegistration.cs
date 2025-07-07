using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Infrastructure.Persistence.WarehouseManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IMS.Infrastructure.Persistence.ProjectManagement
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ProjectManagementDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("ProjectManagementDb"),
                    sqlOptions => sqlOptions.MigrationsAssembly("IMS.Infrastructure") // حتما این رو اضافه کن
                ));

            services.AddDbContext<WarehouseDbContext>(options =>
                 options.UseSqlServer(
                     configuration.GetConnectionString("WarehouseManagementDb"),
                     sqlOptions => sqlOptions.MigrationsAssembly("IMS.Infrastructure") // حتما این رو اضافه کن
                 ));

            return services;
        }
    }
}
