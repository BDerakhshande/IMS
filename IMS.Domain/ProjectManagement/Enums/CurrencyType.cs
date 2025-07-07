using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProjectManagement.Enums
{
    public enum CurrencyType
    {
        [Display(Name = "ریال")]
        IRR,

        [Display(Name = "دلار")]
        USD,

        [Display(Name = "یورو")]
        EUR
    }
}
