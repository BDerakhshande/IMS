using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.ProjectManagement.Entities;

namespace IMS.Domain.ProjectManagement.Validator
{
    public class EmployerValidator : AbstractValidator<Employer>
    {
        public EmployerValidator()
        {
            RuleFor(e => e.CompanyName)
                .NotEmpty().WithMessage("نام شرکت الزامی است");


            RuleFor(e => e.NationalId)
                .NotEmpty().WithMessage("شناسه ملی الزامی است.")
                .Length(10).WithMessage("شناسه ملی باید 10 رقم باشد.")
                .Matches(@"^\d{10}$").WithMessage("شناسه ملی باید فقط شامل اعداد باشد.");



            RuleFor(e => e.RegistrationNumber)
                .NotEmpty().WithMessage("شماره ثبت الزامی است");


            RuleFor(e => e.Address)
          .NotEmpty().WithMessage("آدرس شرکت الزامی است.");

            RuleFor(e => e.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است.")
                .Matches(@"^0\d{10}$").WithMessage("شماره تلفن معتبر نیست.");

            RuleFor(e => e.Website)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .WithMessage("آدرس وب‌سایت معتبر نیست.");

            RuleFor(e => e.RepresentativeName)
                .NotEmpty().WithMessage("نام نماینده الزامی است.");

            RuleFor(e => e.RepresentativePosition)
                .NotEmpty().WithMessage("سمت نماینده الزامی است.");

            RuleFor(e => e.RepresentativeMobile)
                .NotEmpty().WithMessage("موبایل نماینده الزامی است.")
                .Matches(@"^09\d{9}$").WithMessage("شماره موبایل معتبر نیست.");

            RuleFor(e => e.RepresentativeEmail)
                .EmailAddress().WithMessage("آدرس ایمیل نماینده معتبر نیست.");

            RuleFor(e => e.CooperationStartDate)
                .LessThanOrEqualTo(DateTime.Today).WithMessage("تاریخ همکاری نمی‌تواند در آینده باشد.");

        }
    }
}
