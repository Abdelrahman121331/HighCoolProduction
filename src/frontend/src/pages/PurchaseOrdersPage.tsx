import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { RowActions } from "../components/patterns";
import { MasterDataFilterToolbar } from "../components/masterData";
import { Badge, Button, DataTable, EmptyState, Pagination, SkeletonLoader, PageHeader, useToast } from "../components/ui";
import { ApiError } from "../services/api";
import { listPurchaseOrders, type PurchaseOrderListItem } from "../services/purchaseOrdersApi";

const PAGE_SIZE = 10;

function progressTone(status: PurchaseOrderListItem["receiptProgressStatus"]) {
  switch (status) {
    case "FullyReceived":
      return "success";
    case "PartiallyReceived":
      return "warning";
    default:
      return "neutral";
  }
}

export function PurchaseOrdersPage() {
  const [rows, setRows] = useState<PurchaseOrderListItem[]>([]);
  const [search, setSearch] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [reloadKey, setReloadKey] = useState(0);
  const navigate = useNavigate();
  const { showToast } = useToast();

  function handleEdit(row: PurchaseOrderListItem) {
    if (row.status === "Posted") {
      showToast({
        tone: "warning",
        title: "Cannot edit posted order",
        description: "Posted purchase orders are read-only and cannot be edited.",
      });
      return;
    }

    navigate(`/purchase-orders/${row.id}/edit`);
  }

  function handleDelete(row: PurchaseOrderListItem) {
    if (row.status === "Posted") {
      showToast({
        tone: "warning",
        title: "Cannot delete posted order",
        description: "Posted purchase orders cannot be deleted from the UI.",
      });
      return;
    }

    if (window.confirm("Delete this purchase order? Deletion is not available in this version.")) {
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
        const result = await listPurchaseOrders(search);
        if (active) {
          setRows(result);
        }
      } catch (loadError) {
        if (active) {
          setError(loadError instanceof ApiError ? loadError.message : "Failed to load purchase orders.");
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
  const resultLabel = rows.length === 1 ? "1 purchase order" : `${rows.length} purchase orders`;

  return (
    <section className="hc-list-page">
      <PageHeader
        title="Purchase Orders"
        actions={
          <Link className="hc-button hc-button--primary hc-button--md" to="/purchase-orders/new">
            New purchase order
          </Link>
        }
      />

      <MasterDataFilterToolbar
        hasFilters={hasFilters}
        resultLabel={resultLabel}
        searchLabel="Search"
        searchPlaceholder="Search PO no, supplier, or notes"
        searchValue={search}
        statusEnabled={false}
        emptyText="All purchase orders"
        filteredText="Filtered purchase orders"
        onSearchChange={setSearch}
      />

      {error ? (
        <div className="hc-card hc-card--md">
          <EmptyState
            title="Unable to load purchase orders"
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
              <th scope="col">PO</th>
              <th scope="col">Supplier</th>
              <th scope="col">Order Date</th>
              <th scope="col">Expected</th>
              <th scope="col">Status</th>
              <th scope="col">Receipt Progress</th>
              <th scope="col" className="hc-table__head-actions" aria-label="Actions" />
            </tr>
          }
          rows={visibleRows.map((row) => (
            <tr key={row.id} className="hc-table__row">
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.poNo}</span>
                  <span className="hc-table__subtitle">{row.lineCount} {row.lineCount === 1 ? "line" : "lines"}</span>
                </div>
              </td>
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.supplierName}</span>
                  <span className="hc-table__subtitle">{row.supplierCode}</span>
                </div>
              </td>
              <td><span className="hc-table__subtitle">{new Date(row.orderDate).toLocaleDateString()}</span></td>
              <td><span className="hc-table__subtitle">{row.expectedDate ? new Date(row.expectedDate).toLocaleDateString() : "Not set"}</span></td>
              <td><Badge tone={row.status === "Posted" ? "success" : row.status === "Canceled" ? "neutral" : "warning"}>{row.status}</Badge></td>
              <td><Badge tone={progressTone(row.receiptProgressStatus)}>{row.receiptProgressStatus}</Badge></td>
              <td className="hc-table__cell-actions">
                <RowActions>
                  <Button size="sm" variant="secondary" className="hc-table__action-button" onClick={() => handleEdit(row)}>Edit</Button>
                  <Button size="sm" variant="ghost" className="hc-table__action-button" onClick={() => handleDelete(row)}>Delete</Button>
                  {row.status === "Posted" && row.receiptProgressStatus !== "FullyReceived" ? (
                    <Link className="hc-button hc-button--ghost hc-button--sm hc-table__action-button" to={`/purchase-receipts/new?purchaseOrderId=${row.id}`}>Create receipt</Link>
                  ) : null}
                </RowActions>
              </td>
            </tr>
          ))}
          footer={<Pagination currentPage={safePage} onPageChange={setPage} pageSize={PAGE_SIZE} totalCount={rows.length} totalPages={totalPages} />}
          emptyState={hasFilters
            ? <EmptyState title="No purchase orders match the current search" description="Try a broader search term." />
            : <EmptyState title="No purchase orders yet" description="Create the first purchase order to define expected supplier quantities." action={<Link className="hc-button hc-button--primary hc-button--md" to="/purchase-orders/new">Create purchase order</Link>} />}
        />
      ) : null}
    </section>
  );
}
