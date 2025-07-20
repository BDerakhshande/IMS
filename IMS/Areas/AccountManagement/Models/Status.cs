using System.ComponentModel;

namespace IMS.Areas.AccountManagement.Models
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


    public static class EnumHelper
    {
        public static string GetDescription(Status status)
        {
            var field = status.GetType().GetField(status.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? status.ToString() : attribute.Description;
        }
    }

}
