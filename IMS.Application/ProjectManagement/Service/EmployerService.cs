using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Application.ProjectManagement.DTOs;
using IMS.Domain.ProjectManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.ProjectManagement.Service
{
    public class EmployerService :IEmployerService
    {
        private readonly IApplicationDbContext _context;

        public EmployerService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployerDto>> GetAllEmployersAsync()
        {
            return await _context.Employers
                .Select(e => new EmployerDto
                {
                    Id = e.Id,
                    CompanyName = e.CompanyName,
                    NationalId = e.NationalId,
                    RegistrationNumber = e.RegistrationNumber,
                    LegalPersonType = e.LegalPersonType,
                    Address = e.Address,
                    PhoneNumber = e.PhoneNumber,
                    Website = e.Website,
                    RepresentativeName = e.RepresentativeName,
                    RepresentativePosition = e.RepresentativePosition,
                    RepresentativeMobile = e.RepresentativeMobile,
                    RepresentativeEmail = e.RepresentativeEmail,
                    CooperationType = e.CooperationType,
                    CooperationStartDate = e.CooperationStartDate,
                    AdditionalDescription = e.AdditionalDescription
                })
                .ToListAsync();
        }

        public async Task<EmployerDto?> GetEmployerByIdAsync(int id)
        {
            var employer = await _context.Employers.FindAsync(id);
            if (employer == null) return null;

            return new EmployerDto
            {
                Id = employer.Id,
                CompanyName = employer.CompanyName,
                NationalId = employer.NationalId,
                RegistrationNumber = employer.RegistrationNumber,
                LegalPersonType = employer.LegalPersonType,
                Address = employer.Address,
                PhoneNumber = employer.PhoneNumber,
                Website = employer.Website,
                RepresentativeName = employer.RepresentativeName,
                RepresentativePosition = employer.RepresentativePosition,
                RepresentativeMobile = employer.RepresentativeMobile,
                RepresentativeEmail = employer.RepresentativeEmail,
                CooperationType = employer.CooperationType,
                CooperationStartDate = employer.CooperationStartDate,
                AdditionalDescription = employer.AdditionalDescription
            };
        }

        public async Task<int> CreateEmployerAsync(EmployerDto dto)
        {
            var employer = new Employer
            {
                CompanyName = dto.CompanyName,
                NationalId = dto.NationalId,
                RegistrationNumber = dto.RegistrationNumber,
                LegalPersonType = dto.LegalPersonType,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                Website = dto.Website,
                RepresentativeName = dto.RepresentativeName,
                RepresentativePosition = dto.RepresentativePosition,
                RepresentativeMobile = dto.RepresentativeMobile,
                RepresentativeEmail = dto.RepresentativeEmail,
                CooperationType = dto.CooperationType,
                CooperationStartDate = dto.CooperationStartDate,
                AdditionalDescription = dto.AdditionalDescription
            };

            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();
            return employer.Id;
        }

        public async Task<bool> UpdateEmployerAsync(EmployerDto dto)
        {
            var employer = await _context.Employers.FindAsync(dto.Id);
            if (employer == null) return false;

            employer.CompanyName = dto.CompanyName;
            employer.NationalId = dto.NationalId;
            employer.RegistrationNumber = dto.RegistrationNumber;
            employer.LegalPersonType = dto.LegalPersonType;
            employer.Address = dto.Address;
            employer.PhoneNumber = dto.PhoneNumber;
            employer.Website = dto.Website;
            employer.RepresentativeName = dto.RepresentativeName;
            employer.RepresentativePosition = dto.RepresentativePosition;
            employer.RepresentativeMobile = dto.RepresentativeMobile;
            employer.RepresentativeEmail = dto.RepresentativeEmail;
            employer.CooperationType = dto.CooperationType;
            employer.CooperationStartDate = dto.CooperationStartDate;
            employer.AdditionalDescription = dto.AdditionalDescription;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEmployerAsync(int id)
        {
            var employer = await _context.Employers.FindAsync(id);
            if (employer == null) return false;

            _context.Employers.Remove(employer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
