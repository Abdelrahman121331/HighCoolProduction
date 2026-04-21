using ERP.Domain.Shortages;

namespace ERP.Application.Shortages;

public interface IShortageResolutionAllocationService
{
    Task ApplyAsync(ShortageResolution resolution, string actor, CancellationToken cancellationToken);
}
