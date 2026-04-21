using ERP.Domain.Shortages;

namespace ERP.Application.Shortages;

public interface IShortageResolutionValidationService
{
    Task ValidateDraftAsync(ShortageResolution resolution, CancellationToken cancellationToken);
}
