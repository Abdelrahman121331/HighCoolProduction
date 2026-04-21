using ERP.Domain.Common;
using ERP.Domain.MasterData;

namespace ERP.Domain.Statements;

public sealed class SupplierStatementEntry : AuditableEntity
{
    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public SupplierStatementEffectType EffectType { get; set; }

    public SupplierStatementSourceDocumentType SourceDocType { get; set; }

    public Guid SourceDocId { get; set; }

    public Guid? SourceLineId { get; set; }

    public decimal AmountDelta { get; set; }

    public decimal RunningBalance { get; set; }

    public string? Currency { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Notes { get; set; }
}
