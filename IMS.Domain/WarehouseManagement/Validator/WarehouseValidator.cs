using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.WarehouseManagement.Validator
{
    public class WarehouseValidator : AbstractValidator<Warehouse>
    {
        public WarehouseValidator()
        {
            RuleFor(w => w.Code)
                .NotEmpty()
                .WithMessage("کد انبار نباید خالی باشد.")
                .MaximumLength(50)
                .WithMessage("کد انبار نباید بیش از 50 کاراکتر باشد.");

            RuleFor(w => w.Name)
                .NotEmpty()
                .WithMessage("نام انبار نباید خالی باشد.")
                .MaximumLength(100)
                .WithMessage("نام انبار نباید بیش از 100 کاراکتر باشد.");
        }
    }
}
