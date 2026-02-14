using Microsoft.AspNetCore.Identity;

namespace IMS.Infrastructure.Persistence.Identity
{
    // مدل کاربر برای Identity
    public class ApplicationUser : IdentityUser<Guid>
    {
        // اطلاعات اضافی مانند نام و نام خانوادگی می‌تواند اضافه شود
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
