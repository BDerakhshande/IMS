using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.ProjectManagement.Entities;

namespace IMS.Domain.ProjectManagement.Validator
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(p => p.ProjectName)
                .NotEmpty().WithMessage("نام پروژه نمی‌تواند خالی باشد.")
                .MaximumLength(100).WithMessage("نام پروژه نباید بیش از ۱۰۰ کاراکتر باشد.");

            RuleFor(p => p.EmployerId)
                .GreaterThan(0).WithMessage("شناسه کارفرما معتبر نیست.");

            RuleFor(p => p.StartDate)
                .LessThanOrEqualTo(p => p.EndDate)
                .WithMessage("تاریخ پایان نباید قبل از تاریخ شروع باشد.");

            RuleFor(p => p.ProjectType)
                .IsInEnum().WithMessage("نوع پروژه معتبر نیست.");

            RuleFor(p => p.Status)
                .IsInEnum().WithMessage("وضعیت پروژه معتبر نیست.");

           

            RuleFor(p => p.ProgressPercent)
                .InclusiveBetween(0, 100).WithMessage("درصد پیشرفت باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(p => p.Priority)
                .IsInEnum().WithMessage("اولویت پروژه معتبر نیست.");

            RuleFor(p => p.Currency)
                .IsInEnum().WithMessage("واحد پول معتبر نیست.");

            RuleFor(p => p.Budget)
                .GreaterThanOrEqualTo(0).WithMessage("بودجه پروژه نمی‌تواند منفی باشد.");

            RuleFor(p => p.Location)
                .MaximumLength(200).WithMessage("محل اجرا نباید بیش از ۲۰۰ کاراکتر باشد.");

            RuleFor(p => p.Objectives)
                .MaximumLength(500).WithMessage("اهداف پروژه نباید بیش از ۵۰۰ کاراکتر باشد.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد.");
        }
    }
}
