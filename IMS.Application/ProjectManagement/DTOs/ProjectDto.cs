using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Application.ProjectManagement.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }


        public string ProjectName { get; set; } = null!;

      
        public DateTime StartDate { get; set; }

       
        public DateTime EndDate { get; set; }

        public int ProjectTypeId { get; set; }

        
        public ProjectStatus Status { get; set; }

        
        public string ProjectManager { get; set; }

        
        public double ProgressPercent { get; set; }

      
        public ProjectPriority Priority { get; set; }

       
        public string Location { get; set; } = null!;

     
        public string Objectives { get; set; } = null!;

    
        public decimal Budget { get; set; }

       
        public CurrencyType Currency { get; set; }

       
        public string Description { get; set; } = null!;

      
        public int EmployerId { get; set; }

    
        public string? EmployerName { get; set; }
        public string? ProjectTypeName { get; set; }
    }
}
