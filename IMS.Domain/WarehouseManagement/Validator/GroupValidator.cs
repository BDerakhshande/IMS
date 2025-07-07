using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.WarehouseManagement.Validator
{
    public class GroupValidator : AbstractValidator<Group>
    {
        public GroupValidator()
        {
            RuleFor(g => g.Name)
                .NotEmpty()
                .WithMessage("نام گروه نباید خالی باشد.")
                .MaximumLength(100)
                .WithMessage("نام گروه نباید بیش از 100 کاراکتر باشد.");

            RuleFor(g => g.CategoryId)
                .GreaterThan(0)
                .WithMessage("شناسه دسته‌بندی باید مقدار مثبت باشد.");
        }
    }
}
