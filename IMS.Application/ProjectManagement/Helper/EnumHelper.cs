using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace IMS.Application.ProjectManagement.Helper
{
    public static class EnumHelper
    {

        public static string GetDisplayName(this Enum enumValue)
        {
            var type = enumValue.GetType();
            var member = type.GetMember(enumValue.ToString());

            if (member != null && member.Length > 0)
            {
                var attr = member[0].GetCustomAttribute<DisplayAttribute>();
                if (attr != null)
                {
                    return attr.Name ?? enumValue.ToString();
                }
            }
            return enumValue.ToString();
        }
    }
}
