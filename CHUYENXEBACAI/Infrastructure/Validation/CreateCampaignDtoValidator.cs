using FluentValidation;
using CHUYENXEBACAI.Controllers;

namespace CHUYENXEBACAI.Infrastructure.Validation;

public class CreateCampaignDtoValidator : AbstractValidator<CreateCampaignDto>
{
    public CreateCampaignDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GoalAmount).GreaterThanOrEqualTo(0).When(x => x.GoalAmount.HasValue);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("end_date must be >= start_date");
    }
}
