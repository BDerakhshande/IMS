using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Domain.ProjectManagement.Enums;

namespace IMS.Application.ProjectManagement.DTOs
{
    public class EmployerDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public long RegistrationNumber { get; set; }
        public LegalPersonType LegalPersonType { get; set; }
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Website { get; set; } = null!;
        public string RepresentativeName { get; set; } = null!;
        public string RepresentativePosition { get; set; } = null!;
        public string RepresentativeMobile { get; set; } = null!;
        public string RepresentativeEmail { get; set; } = null!;
        public CooperationType CooperationType { get; set; }
        public DateTime CooperationStartDate { get; set; }
        public string AdditionalDescription { get; set; } = null!;
    }
}
