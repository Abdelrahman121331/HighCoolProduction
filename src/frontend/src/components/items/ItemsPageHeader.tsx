import { Link } from "react-router-dom";
import { PageHeader } from "../ui";

export function ItemsPageHeader() {
  return (
    <PageHeader
      title="Items"
      actions={
        <Link className="hc-button hc-button--primary hc-button--md" to="/items/new">
          New item
        </Link>
      }
    />
  );
}
