import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { AdvisoryDto } from '../types.js';

@customElement('security-dashboard-advisory-item')
export class AdvisoryItemElement extends LitElement {
  @property({ type: Object }) advisory!: AdvisoryDto;

  static styles = css`
    :host { display: contents; }
    .advisory-row {
      display: grid;
      grid-template-columns: 1fr auto auto auto;
      align-items: center;
      gap: 12px;
      padding: 12px 0;
      border-bottom: 1px solid var(--uui-color-border, #e0e0e0);
    }
    .advisory-title {
      font-weight: 500;
    }
    .advisory-package {
      font-size: 0.8rem;
      color: var(--uui-color-text-alt, #666);
      margin-top: 2px;
    }
    .badges { display: flex; gap: 6px; align-items: center; }
  `;

  private getSeverityColor(severity: string): string {
    switch (severity) {
      case 'Critical': return 'danger';
      case 'High': return 'warning';
      case 'Moderate': return 'caution';
      case 'Low': return 'default';
      default: return 'default';
    }
  }

  private getStatusColor(status: string): string {
    switch (status) {
      case 'Affected': return 'danger';
      case 'Unknown': return 'warning';
      case 'NotAffected': return 'positive';
      default: return 'default';
    }
  }

  render() {
    if (!this.advisory) return html``;

    return html`
      <div class="advisory-row">
        <div>
          <div class="advisory-title">${this.advisory.title}</div>
          <div class="advisory-package">
            ${this.advisory.packageName}
            ${this.advisory.installedVersion ? `v${this.advisory.installedVersion}` : ''}
            — ${this.advisory.affectedVersionRange}
          </div>
        </div>
        <div class="badges">
          <uui-tag color="${this.getSeverityColor(this.advisory.severity)}">
            ${this.advisory.severity}
          </uui-tag>
          <uui-tag color="${this.getStatusColor(this.advisory.affectedStatus)}">
            ${this.advisory.affectedStatus === 'NotAffected' ? 'Not Affected' : this.advisory.affectedStatus}
          </uui-tag>
        </div>
        <uui-button
          look="secondary"
          href="${this.advisory.advisoryUrl}"
          target="_blank"
          rel="noopener noreferrer"
          label="More info about ${this.advisory.ghsaId}">
          More Info →
        </uui-button>
      </div>
    `;
  }
}
