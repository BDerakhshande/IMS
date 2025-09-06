using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProjectManagement.Enums
{
    public enum ProjectStatus
    {
        [Display(Name = "درحال برنامه ریزی")]
        Planned,

        [Display(Name = "در حال اجرا")]
        InProgress,

        [Display(Name = "تکمیل شده")]
        Completed,

        [Display(Name = "متوقف شده")]
        Suspended
    }
}
