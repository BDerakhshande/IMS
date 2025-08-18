using FluentValidation;
using IMS.Application.ProcurementManagement.Service;
using IMS.Application.ProjectManagement.Service;
using IMS.Application.WarehouseManagement.MappingProfiles;
using IMS.Application.WarehouseManagement.Services;
using IMS.Areas.AccountManagement.Data;

using IMS.Domain.ProjectManagement.Validator;
using IMS.Domain.WarehouseManagement.Validator;
using IMS.Infrastructure.Persistence.ProcurementManagement;
using IMS.Infrastructure.Persistence.ProjectManagement;
using IMS.Infrastructure.Persistence.WarehouseManagement;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddValidatorsFromAssemblyContaining<EmployerValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProjectValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CategoryValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<GroupValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

builder.Services.AddValidatorsFromAssemblyContaining<StatusValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<WarehouseValidator>();



builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<IEmployerService, EmployerService>();

builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<IProjectTypeService, ProjectTypeService>();

builder.Services.AddScoped<IInventoryReportService, InventoryReportService>();
builder.Services.AddScoped<IInventoryTransactionReportService, InventoryTransactionReportService>();


builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IReceiptOrIssueService, ReceiptOrIssueService>();
builder.Services.AddScoped<IConversionService, ConversionService>();
builder.Services.AddScoped<IGoodsRequestService, GoodsRequestService>();
builder.Services.AddScoped<IPurchaseRequestService,PurchaseRequestService>();
builder.Services.AddScoped<IRequestTypeService, RequestTypeService>();
builder.Services.AddScoped<IPurchaseRequestTrackingService, PurchaseRequestTrackingService>();

builder.Services.AddScoped<IApplicationDbContext,ProjectManagementDbContext>();
builder.Services.AddScoped<IWarehouseDbContext, WarehouseDbContext>();
builder.Services.AddScoped<IProcurementManagementDbContext, ProcurementManagementDbContext>();
builder.Services.AddScoped<IInventoryTurnoverService, InventoryTurnoverService>();
builder.Services.AddScoped<IWarehouseTransactionDetailService, WarehouseTransactionDetailService>();
builder.Services.AddScoped<IPurchaseRequestFlatItemService, PurchaseRequestFlatItemService>();

builder.Services.AddAutoMapper(typeof(WarehouseMappingProfile));





builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WarehouseDb")));


builder.Services.AddDbContext<AccountManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AccountManagementDb")));


builder.Services.AddDbContext<ProcurementManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProcurementManagementDb")));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.UseRotativa();


app.Run();
