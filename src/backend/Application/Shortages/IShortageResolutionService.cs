namespace ERP.Application.Shortages;

public interface IShortageResolutionService
{
    Task<IReadOnlyList<ShortageResolutionListItemDto>> ListAsync(ShortageResolutionListQuery query, CancellationToken cancellationToken);

    Task<ShortageResolutionDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<ShortageResolutionDto> CreateDraftAsync(UpsertShortageResolutionRequest request, string actor, CancellationToken cancellationToken);

    Task<ShortageResolutionDto?> UpdateDraftAsync(Guid id, UpsertShortageResolutionRequest request, string actor, CancellationToken cancellationToken);

    Task<IReadOnlyList<ShortageResolutionAllocationDto>> GetAllocationsAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<OpenShortageDto>> ListOpenShortagesAsync(OpenShortageQuery query, CancellationToken cancellationToken);

    Task<OpenShortageDto?> GetShortageAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<SuggestedShortageAllocationDto>> SuggestAllocationsAsync(SuggestShortageAllocationsQuery query, CancellationToken cancellationToken);
}
