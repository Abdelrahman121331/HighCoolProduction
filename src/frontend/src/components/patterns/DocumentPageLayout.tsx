import type { PropsWithChildren, ReactNode } from "react";

interface DocumentPageLayoutProps {
  title: string;
  eyebrow?: string;
  description?: string;
  status?: ReactNode;
  actions?: ReactNode;
  footer?: ReactNode;
}

export function DocumentPageLayout({
  actions,
  children,
  description,
  eyebrow,
  footer,
  status,
  title,
}: PropsWithChildren<DocumentPageLayoutProps>) {
  return (
    <section className="hc-document-page">
      <div className="hc-document-page__surface">
        <header className="hc-document-page__header">
          <div className="hc-document-page__header-main">
            {eyebrow ? <p className="hc-document-page__eyebrow">{eyebrow}</p> : null}
            <div className="hc-document-page__headline">
              <h1 className="hc-document-page__title">{title}</h1>
              {status ? <div className="hc-document-page__status">{status}</div> : null}
            </div>
            {description ? <p className="hc-document-page__description">{description}</p> : null}
          </div>
          {actions ? <div className="hc-document-page__actions">{actions}</div> : null}
        </header>

        <div className="hc-document-page__body">{children}</div>

        {footer ? <footer className="hc-document-page__footer">{footer}</footer> : null}
      </div>
    </section>
  );
}
