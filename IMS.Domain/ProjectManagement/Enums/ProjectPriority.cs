using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProjectManagement.Enums
{
    public enum ProjectPriority
    {
        [Display(Name = "کم")]
        Low,

        [Display(Name = "متوسط")]
        Medium,

        [Display(Name = "زیاد")]
        High,

        [Display(Name = "بحرانی")]
        Critical
    }
}
