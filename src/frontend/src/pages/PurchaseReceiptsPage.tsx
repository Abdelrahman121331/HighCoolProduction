import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { RowActions } from "../components/patterns";
import { MasterDataFilterToolbar } from "../components/masterData";
import { Badge, Button, DataTable, EmptyState, Pagination, SkeletonLoader, PageHeader, useToast } from "../components/ui";
import { ApiError } from "../services/api";
import { listPurchaseReceiptDrafts, type PurchaseReceiptListItem } from "../services/purchaseReceiptsApi";

const PAGE_SIZE = 10;

export function PurchaseReceiptsPage() {
  const [rows, setRows] = useState<PurchaseReceiptListItem[]>([]);
  const [search, setSearch] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [reloadKey, setReloadKey] = useState(0);
  const navigate = useNavigate();
  const { showToast } = useToast();

  function handleEdit(row: PurchaseReceiptListItem) {
    if (row.status === "Posted") {
      showToast({
        tone: "warning",
        title: "Cannot edit posted receipt",
        description: "Posted receipts are read-only and cannot be edited.",
      });
      return;
    }

    navigate(`/purchase-receipts/${row.id}/edit`);
  }

  function handleDelete(row: PurchaseReceiptListItem) {
    if (row.status === "Posted") {
      showToast({
        tone: "warning",
        title: "Cannot delete posted receipt",
        description: "Posted receipts cannot be deleted from the UI.",
      });
      return;
    }

    if (window.confirm("Delete this purchase receipt? Deletion is not available in this version.")) {
      showToast({
        tone: "info",
        title: "Delete action unavailable",
        description: "Permanent delete is not supported in this UI yet.",
      });
    }
  }

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        setLoading(true);
        setError("");
        const result = await listPurchaseReceiptDrafts(search);

        if (active) {
          setRows(result);
        }
      } catch (loadError) {
        if (active) {
          setError(loadError instanceof ApiError ? loadError.message : "Failed to load purchase receipts.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void load();
    return () => {
      active = false;
    };
  }, [reloadKey, search]);

  useEffect(() => {
    setPage(1);
  }, [search]);

  const totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const pageStart = (safePage - 1) * PAGE_SIZE;
  const visibleRows = rows.slice(pageStart, pageStart + PAGE_SIZE);
  const hasFilters = Boolean(search.trim());
  const resultLabel = rows.length === 1 ? "1 purchase receipt" : `${rows.length} purchase receipts`;

  return (
    <section className="hc-list-page">
      <PageHeader
        title="Purchase Receipts"
        actions={
          <Link className="hc-button hc-button--primary hc-button--md" to="/purchase-receipts/new">
            New purchase receipt
          </Link>
        }
      />

      <MasterDataFilterToolbar
        hasFilters={hasFilters}
        resultLabel={resultLabel}
        searchLabel="Search"
        searchPlaceholder="Search receipt no, supplier, warehouse, PO, or notes"
        searchValue={search}
        statusEnabled={false}
        emptyText="All purchase receipts"
        filteredText="Filtered purchase receipts"
        onSearchChange={setSearch}
      />

      {error ? (
        <div className="hc-card hc-card--md">
          <EmptyState
            title="Unable to load purchase receipts"
            description={error}
            action={<Button variant="secondary" onClick={() => setReloadKey((current) => current + 1)}>Retry</Button>}
          />
        </div>
      ) : null}

      {loading ? (
        <div className="hc-card hc-card--md hc-table-card">
          <div className="hc-skeleton-stack">
            <SkeletonLoader height="2.75rem" variant="rect" />
            <SkeletonLoader height="3.5rem" variant="rect" />
            <SkeletonLoader height="3.5rem" variant="rect" />
          </div>
        </div>
      ) : null}

      {!loading && !error ? (
        <DataTable
          hasData={rows.length > 0}
          columns={
            <tr>
              <th scope="col">Receipt</th>
              <th scope="col">Supplier</th>
              <th scope="col">Warehouse</th>
              <th scope="col">PO</th>
              <th scope="col">Date</th>
              <th scope="col">Status</th>
              <th scope="col" className="hc-table__head-actions" aria-label="Actions" />
            </tr>
          }
          rows={visibleRows.map((row) => (
            <tr key={row.id} className="hc-table__row">
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.receiptNo}</span>
                  <span className="hc-table__subtitle">{row.lineCount} {row.lineCount === 1 ? "line" : "lines"}</span>
                </div>
              </td>
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.supplierName}</span>
                  <span className="hc-table__subtitle">{row.supplierCode}</span>
                </div>
              </td>
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.warehouseName}</span>
                  <span className="hc-table__subtitle">{row.warehouseCode}</span>
                </div>
              </td>
              <td><span className="hc-table__subtitle">{row.purchaseOrderNo ?? "Manual"}</span></td>
              <td><span className="hc-table__subtitle">{new Date(row.receiptDate).toLocaleDateString()}</span></td>
              <td><Badge tone={row.status === "Posted" ? "success" : row.status === "Canceled" ? "neutral" : "warning"}>{row.status}</Badge></td>
              <td className="hc-table__cell-actions">
                <RowActions>
                  <Button size="sm" variant="secondary" className="hc-table__action-button" onClick={() => handleEdit(row)}>Edit</Button>
                  <Button size="sm" variant="ghost" className="hc-table__action-button" onClick={() => handleDelete(row)}>Delete</Button>
                </RowActions>
              </td>
            </tr>
          ))}
          footer={<Pagination currentPage={safePage} onPageChange={setPage} pageSize={PAGE_SIZE} totalCount={rows.length} totalPages={totalPages} />}
          emptyState={hasFilters
            ? <EmptyState title="No purchase receipts match the current search" description="Try a broader search term." />
            : <EmptyState title="No purchase receipts yet" description="Create your first purchase receipt." action={<Link className="hc-button hc-button--primary hc-button--md" to="/purchase-receipts/new">Create purchase receipt</Link>} />}
        />
      ) : null}
    </section>
  );
}
