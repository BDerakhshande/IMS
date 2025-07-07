using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProjectManagement.Enums
{
    public enum LegalPersonType
    {
        [Display(Name = " حقیقی")]
        RealPerson,    

        [Display(Name = "حقوقی")]
        LegalPerson   
    }
}
