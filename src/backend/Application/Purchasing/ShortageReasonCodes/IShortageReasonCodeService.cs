namespace ERP.Application.Purchasing.ShortageReasonCodes;

public interface IShortageReasonCodeService
{
    Task<IReadOnlyList<ShortageReasonCodeDto>> ListActiveAsync(CancellationToken cancellationToken);
}
