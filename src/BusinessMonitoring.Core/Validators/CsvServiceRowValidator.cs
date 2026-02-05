using BusinessMonitoring.Core.DTOs;
using FluentValidation;

namespace BusinessMonitoring.Core.Validators;

public class CsvServiceRowValidator : AbstractValidator<CsvServiceRow>
{
    public CsvServiceRowValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required")
            .MaximumLength(100).WithMessage("CustomerId must not exceed 100 characters");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("ServiceType is required")
            .MaximumLength(50).WithMessage("ServiceType must not exceed 50 characters")
            .Must(BeValidServiceType).WithMessage("ServiceType must be one of: hosting, pec, spid, fatturazione");

        RuleFor(x => x.ActivationDate)
            .NotEmpty().WithMessage("ActivationDate is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("ActivationDate cannot be in the future");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty().WithMessage("ExpirationDate is required")
            .GreaterThan(x => x.ActivationDate).WithMessage("ExpirationDate must be after ActivationDate");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0")
            .LessThanOrEqualTo(999999.99m).WithMessage("Amount must not exceed 999999.99");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage("Status must be one of: active, expired, pending_renewal");
    }

    private bool BeValidServiceType(string serviceType)
    {
        var validTypes = new[] { "hosting", "pec", "spid", "fatturazione" };
        return validTypes.Contains(serviceType.ToLowerInvariant());
    }
    private bool BeValidStatus(string status)
    {
        var validTypes = new[] { "active", "expired", "pending_renewal"};
        return validTypes.Contains(status.ToLowerInvariant());
    }
}
