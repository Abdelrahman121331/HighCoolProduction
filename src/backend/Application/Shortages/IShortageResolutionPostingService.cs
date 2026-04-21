namespace ERP.Application.Shortages;

public interface IShortageResolutionPostingService
{
    Task<ShortageResolutionDto?> PostAsync(Guid id, string actor, CancellationToken cancellationToken);
}
