using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProcurementManagement.Enums
{
    public enum Status
    {
        [Description("تایید")]
        Confirmation,

        [Description("در انتظار تایید")]
        AwaitingApproval,

        [Description("رد")]
        Reject
    }
}
