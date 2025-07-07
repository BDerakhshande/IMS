using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using IMS.Domain.ProjectManagement.Entities;
using IMS.Domain.WarehouseManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Domain.WarehouseManagement.Validator
{
    public class CategoryValidator: AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage("نام دسته‌بندی نباید خالی باشد.")

                // طول نام نباید بیشتر از 100 کاراکتر باشد
                .MaximumLength(100)
                .WithMessage("نام دسته‌بندی نباید بیش از 100 کاراکتر باشد.")

                // نام باید فقط شامل حروف، اعداد، فاصله و - یا _ باشد (مثلاً)
                .Matches(@"^[\p{L}\d\s\-_]+$")
                .WithMessage("نام دسته‌بندی فقط می‌تواند شامل حروف، اعداد، فاصله، خط تیره و زیرخط باشد.");

            // می‌توان اعتبارسنجی روی لیست گروه‌ها هم اضافه کرد اگر لازم بود
            RuleForEach(c => c.Groups).SetValidator(new GroupValidator());

            // (اختیاری) مثلا Id نباید منفی باشد
            RuleFor(c => c.Id)
                .GreaterThanOrEqualTo(0)
                .WithMessage("شناسه دسته‌بندی نمی‌تواند منفی باشد.");
        }
    }
}
