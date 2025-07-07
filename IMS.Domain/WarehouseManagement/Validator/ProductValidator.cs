using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Domain.WarehouseManagement.Validator
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .WithMessage("نام کالا نباید خالی باشد.")
                .MaximumLength(150)
                .WithMessage("نام کالا نباید بیش از 150 کاراکتر باشد.");

            RuleFor(p => p.Code)
                .MaximumLength(50)
                .WithMessage("کد کالا نباید بیش از 50 کاراکتر باشد.")
                .When(p => !string.IsNullOrEmpty(p.Code));

            RuleFor(p => p.Description)
                .MaximumLength(500)
                .WithMessage("توضیحات نباید بیش از 500 کاراکتر باشد.")
                .When(p => !string.IsNullOrEmpty(p.Description));

            RuleFor(p => p.StatusId)
                .GreaterThan(0)
                .WithMessage("وضعیت کالا باید مشخص شود.");

            RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("قیمت کالا نمی‌تواند منفی باشد.");

           
        }
    }
}
