using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.WarehouseManagement.Validator
{
    public class StatusValidator : AbstractValidator<Status>
    {
        public StatusValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty()
                .WithMessage("نام وضعیت نباید خالی باشد.")
                .MaximumLength(100)
                .WithMessage("نام وضعیت نباید بیش از 100 کاراکتر باشد.");

            RuleFor(s => s.GroupId)
                .GreaterThan(0)
                .WithMessage("شناسه گروه باید معتبر باشد.");
        }
    }
}
