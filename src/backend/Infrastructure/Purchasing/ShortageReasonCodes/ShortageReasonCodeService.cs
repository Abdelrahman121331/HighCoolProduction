using ERP.Application.Purchasing.ShortageReasonCodes;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Purchasing.ShortageReasonCodes;

public sealed class ShortageReasonCodeService(AppDbContext dbContext) : IShortageReasonCodeService
{
    public async Task<IReadOnlyList<ShortageReasonCodeDto>> ListActiveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ShortageReasonCodes
            .AsNoTracking()
            .Where(entity => entity.IsActive)
            .OrderBy(entity => entity.Code)
            .Select(entity => new ShortageReasonCodeDto(
                entity.Id,
                entity.Code,
                entity.Name,
                entity.Description,
                entity.AffectsSupplierBalance,
                entity.AffectsStock,
                entity.RequiresApproval))
            .ToListAsync(cancellationToken);
    }
}
