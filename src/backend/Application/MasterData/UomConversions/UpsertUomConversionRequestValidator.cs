using FluentValidation;

namespace ERP.Application.MasterData.UomConversions;

public sealed class UpsertUomConversionRequestValidator : AbstractValidator<UpsertUomConversionRequest>
{
    public UpsertUomConversionRequestValidator()
    {
        RuleFor(request => request.FromUomId)
            .NotEmpty();

        RuleFor(request => request.ToUomId)
            .NotEmpty();

        RuleFor(request => request.Factor)
            .GreaterThan(0m);

        RuleFor(request => request)
            .Must(request => request.FromUomId != request.ToUomId)
            .WithMessage("From UOM and To UOM must be different.");
    }
}
