using FluentValidation;
using TaskMasterPro.Application.DTOs;

namespace TaskMasterPro.Application.Validators;

public class CreateColumnRequestValidator : AbstractValidator<CreateColumnRequest>
{
    public CreateColumnRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
    }
}

public class UpdateColumnRequestValidator : AbstractValidator<UpdateColumnRequest>
{
    public UpdateColumnRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
    }
}
