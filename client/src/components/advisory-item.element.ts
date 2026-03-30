import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

interface AdvisoryDto {
  ghsaId: string;
  title: string;
  severity: string;
  packageName: string;
  affectedVersionRange: string;
  advisoryUrl: string;
  publishedAt: string;
  installedVersion: string | null;
  affectedStatus: string;
}

type SeverityLevel = 'Critical' | 'High' | 'Moderate' | 'Low';
type StatusType = 'Affected' | 'NotAffected' | 'Unknown';

const SEVERITY_COLORS: Record<SeverityLevel, string> = {
  Critical: 'danger',
  High: 'danger',
  Moderate: 'warning',
  Low: 'default',
};

const STATUS_COLORS: Record<StatusType, string> = {
  Affected: 'danger',
  NotAffected: 'positive',
  Unknown: 'warning',
};

@customElement('security-advisory-item')
export class AdvisoryItemElement extends LitElement {
  static styles = css`
    :host {
      display: block;
      margin-bottom: var(--uui-size-space-3, 8px);
    }
    .advisory-header {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2, 6px);
      flex-wrap: wrap;
    }
    .advisory-title {
      font-weight: bold;
      flex: 1;
    }
    .advisory-meta {
      display: flex;
      gap: var(--uui-size-space-2, 6px);
      align-items: center;
      margin-top: var(--uui-size-space-1, 4px);
      font-size: var(--uui-type-small-size, 13px);
      color: var(--uui-color-text-alt, #666);
      flex-wrap: wrap;
    }
    a {
      color: var(--uui-color-interactive, #1b264f);
    }
    hr {
      border: none;
      border-top: 1px solid var(--uui-color-border, #e0e0e0);
      margin: var(--uui-size-space-3, 8px) 0;
    }
  `;

  @property({ type: Object }) advisory!: AdvisoryDto;

  override render() {
    const severityColor = SEVERITY_COLORS[this.advisory.severity as SeverityLevel] ?? 'default';
    const statusColor = STATUS_COLORS[this.advisory.affectedStatus as StatusType] ?? 'default';
    const publishedDate = new Date(this.advisory.publishedAt).toLocaleDateString();

    return html`
      <div class="advisory-header">
        <span class="advisory-title">${this.advisory.packageName}</span>
        <uui-badge color=${severityColor}>${this.advisory.severity}</uui-badge>
        <uui-badge color=${statusColor}>${this.advisory.affectedStatus}</uui-badge>
      </div>
      <div class="advisory-meta">
        <span>${this.advisory.title}</span>
        &bull;
        <span>Range: ${this.advisory.affectedVersionRange}</span>
        ${this.advisory.installedVersion
          ? html`&bull; <span>Installed: ${this.advisory.installedVersion}</span>`
          : html`&bull; <span>Version: Unknown</span>`}
        &bull;
        <span>Published: ${publishedDate}</span>
        &bull;
        <a href=${this.advisory.advisoryUrl}
           target="_blank"
           rel="noopener noreferrer"
           aria-label="View advisory ${this.advisory.ghsaId} on GitHub (opens in new tab)">
          ${this.advisory.ghsaId}
        </a>
      </div>
      <hr />
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'security-advisory-item': AdvisoryItemElement;
  }
}
