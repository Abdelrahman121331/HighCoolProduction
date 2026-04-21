import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { RowActions } from "../components/patterns";
import {
  Badge,
  Button,
  Card,
  DataTable,
  EmptyState,
  Field,
  Input,
  PageHeader,
  Pagination,
  Select,
  SkeletonLoader,
  useToast,
} from "../components/ui";
import { ApiError } from "../services/api";
import { listSuppliers, type Supplier } from "../services/masterDataApi";
import {
  listShortageResolutions,
  type ShortageResolutionFilters,
  type ShortageResolutionListItem,
} from "../services/shortageResolutionsApi";

const PAGE_SIZE = 12;

const INITIAL_FILTERS: ShortageResolutionFilters = {
  search: "",
  supplierId: "",
  resolutionType: "",
  status: "",
  fromDate: "",
  toDate: "",
};

export function ShortageResolutionsPage() {
  const [rows, setRows] = useState<ShortageResolutionListItem[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [filters, setFilters] = useState<ShortageResolutionFilters>(INITIAL_FILTERS);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [reloadKey, setReloadKey] = useState(0);
  const navigate = useNavigate();
  const { showToast } = useToast();

  useEffect(() => {
    let active = true;

    async function loadReferences() {
      try {
        const result = await listSuppliers("", "active");

        if (active) {
          setSuppliers(result);
        }
      } catch {
        if (active) {
          setError("Failed to load shortage resolution filters.");
        }
      }
    }

    void loadReferences();
    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        setLoading(true);
        setError("");
        const result = await listShortageResolutions(filters);

        if (active) {
          setRows(result);
        }
      } catch (loadError) {
        if (active) {
          setError(loadError instanceof ApiError ? loadError.message : "Failed to load shortage resolutions.");
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
  }, [filters, reloadKey]);

  useEffect(() => {
    setPage(1);
  }, [filters]);

  const totalPages = Math.max(1, Math.ceil(rows.length / PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const pageStart = (safePage - 1) * PAGE_SIZE;
  const visibleRows = rows.slice(pageStart, pageStart + PAGE_SIZE);
  const resultLabel = rows.length === 1 ? "1 shortage resolution" : `${rows.length} shortage resolutions`;
  const hasFilters = useMemo(
    () => Object.values(filters).some((value) => value.trim().length > 0),
    [filters],
  );

  function setFilter<K extends keyof ShortageResolutionFilters>(key: K, value: ShortageResolutionFilters[K]) {
    setFilters((current) => ({ ...current, [key]: value }));
  }

  function handleEdit(row: ShortageResolutionListItem) {
    if (row.status === "Posted") {
      showToast({
        tone: "warning",
        title: "Posted resolution is read-only",
        description: "Posted shortage resolutions cannot be edited directly.",
      });
      return;
    }

    navigate(`/shortage-resolutions/${row.id}/edit`);
  }

  return (
    <section className="hc-list-page">
      <PageHeader
        title="Shortage Resolutions"
        description="Manage physical replacements and financial settlements that close receipt shortage rows over time."
        eyebrow="Inventory"
        actions={
          <Link className="hc-button hc-button--primary hc-button--md" to="/shortage-resolutions/new">
            New shortage resolution
          </Link>
        }
      />

      <Card className="hc-inventory-filter-panel" padding="md">
        <div className="hc-inventory-filter-panel__top">
          <div>
            <h2 className="hc-inventory-filter-panel__title">Filters</h2>
            <p className="hc-inventory-filter-panel__description">Review draft and posted shortage resolutions by supplier, type, date, and workflow status.</p>
          </div>
          <div className="hc-inventory-filter-panel__meta">
            <span className="hc-inventory-filter-panel__result">{resultLabel}</span>
            {hasFilters ? (
              <Button size="sm" variant="ghost" onClick={() => setFilters(INITIAL_FILTERS)}>
                Reset filters
              </Button>
            ) : null}
          </div>
        </div>

        <div className="hc-form-grid hc-inventory-filter-grid">
          <Field label="Search">
            <Input
              placeholder="Search resolution no, supplier, notes"
              value={filters.search}
              onChange={(event) => setFilter("search", event.target.value)}
            />
          </Field>

          <Field label="Supplier">
            <Select value={filters.supplierId} onChange={(event) => setFilter("supplierId", event.target.value)}>
              <option value="">All suppliers</option>
              {suppliers.map((supplier) => (
                <option key={supplier.id} value={supplier.id}>
                  {supplier.code} - {supplier.name}
                </option>
              ))}
            </Select>
          </Field>

          <Field label="Resolution type">
            <Select value={filters.resolutionType} onChange={(event) => setFilter("resolutionType", event.target.value)}>
              <option value="">All types</option>
              <option value="Physical">Physical</option>
              <option value="Financial">Financial</option>
            </Select>
          </Field>

          <Field label="Status">
            <Select value={filters.status} onChange={(event) => setFilter("status", event.target.value)}>
              <option value="">All statuses</option>
              <option value="Draft">Draft</option>
              <option value="Posted">Posted</option>
              <option value="Canceled">Canceled</option>
            </Select>
          </Field>

          <Field label="From date">
            <Input type="date" value={filters.fromDate} onChange={(event) => setFilter("fromDate", event.target.value)} />
          </Field>

          <Field label="To date">
            <Input type="date" value={filters.toDate} onChange={(event) => setFilter("toDate", event.target.value)} />
          </Field>
        </div>
      </Card>

      {error ? (
        <Card padding="md">
          <EmptyState
            title="Unable to load shortage resolutions"
            description={error}
            action={<Button variant="secondary" onClick={() => setReloadKey((current) => current + 1)}>Retry</Button>}
          />
        </Card>
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
              <th scope="col">Resolution</th>
              <th scope="col">Supplier</th>
              <th scope="col">Type</th>
              <th scope="col">Totals</th>
              <th scope="col">Date</th>
              <th scope="col">Status</th>
              <th scope="col" className="hc-table__head-actions" aria-label="Actions" />
            </tr>
          }
          rows={visibleRows.map((row) => (
            <tr key={row.id} className="hc-table__row">
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.resolutionNo}</span>
                  <span className="hc-table__subtitle">{row.allocationCount} allocations</span>
                </div>
              </td>
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.supplierName}</span>
                  <span className="hc-table__subtitle">{row.supplierCode}</span>
                </div>
              </td>
              <td><Badge tone={row.resolutionType === "Physical" ? "primary" : "warning"}>{row.resolutionType}</Badge></td>
              <td>
                <div className="hc-table__cell-strong">
                  <span className="hc-table__title">{row.resolutionType === "Physical" ? (row.totalQty?.toLocaleString() ?? "0") : (row.totalAmount?.toLocaleString() ?? "0")}</span>
                  <span className="hc-table__subtitle">{row.resolutionType === "Physical" ? "Total qty" : row.currency ?? "Total amount"}</span>
                </div>
              </td>
              <td><span className="hc-table__subtitle">{new Date(row.resolutionDate).toLocaleDateString()}</span></td>
              <td><Badge tone={row.status === "Posted" ? "success" : row.status === "Canceled" ? "neutral" : "warning"}>{row.status}</Badge></td>
              <td className="hc-table__cell-actions">
                <RowActions>
                  <Button size="sm" variant="secondary" className="hc-table__action-button" onClick={() => handleEdit(row)}>Open</Button>
                </RowActions>
              </td>
            </tr>
          ))}
          footer={<Pagination currentPage={safePage} onPageChange={setPage} pageSize={PAGE_SIZE} totalCount={rows.length} totalPages={totalPages} />}
          emptyState={
            hasFilters
              ? <EmptyState title="No resolutions match the current filters" description="Try broadening the supplier, type, or date range." />
              : <EmptyState title="No shortage resolutions yet" description="Create the first physical or financial settlement document." action={<Link className="hc-button hc-button--primary hc-button--md" to="/shortage-resolutions/new">Create shortage resolution</Link>} />
          }
        />
      ) : null}
    </section>
  );
}
