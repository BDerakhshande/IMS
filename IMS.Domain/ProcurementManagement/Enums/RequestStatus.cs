using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProcurementManagement.Enums
{
    public enum RequestStatus
    {
        Pending = 0,  // منتظر بررسی  
        Approved = 1, // تایید شده  
        Rejected = 2  // رد شده

    }
}
