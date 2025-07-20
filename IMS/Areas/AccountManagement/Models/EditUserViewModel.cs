using System.ComponentModel.DataAnnotations;

namespace IMS.Areas.AccountManagement.Models
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        public string? Password { get; set; }

        public string Role { get; set; }

        public bool CanViewDashboard { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanViewReports { get; set; }
        public bool CanManageTransactions { get; set; }
        public bool CanManageAccounts { get; set; }
        public bool CanManageCounterparties { get; set; }
        public bool CanManageCostCenters { get; set; }
    }
}
