using ERP.Domain.Common;
using ERP.Domain.MasterData;

namespace ERP.Domain.Purchasing;

public sealed class PurchaseOrder : BusinessDocument
{
    public string PoNo { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
