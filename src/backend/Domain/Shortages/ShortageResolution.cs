using ERP.Domain.Common;
using ERP.Domain.MasterData;

namespace ERP.Domain.Shortages;

public sealed class ShortageResolution : BusinessDocument
{
    public string ResolutionNo { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public ShortageResolutionType ResolutionType { get; set; }

    public DateTime ResolutionDate { get; set; }

    public decimal? TotalQty { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Currency { get; set; }

    public string? Notes { get; set; }

    public string? ApprovedBy { get; set; }

    public ICollection<ShortageResolutionAllocation> Allocations { get; set; } = new List<ShortageResolutionAllocation>();
}
