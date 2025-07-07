using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Domain.ProjectManagement.Entities
{
    public class ProjectType
    {
        public int Id { get; set; }

        // عنوان نوع پروژه
        public string Name { get; set; } = null!;

        // توضیح اختیاری
        public string? Description { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();

    }
}
