import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('security-dashboard-staleness-warning')
export class StalenessWarningElement extends LitElement {
  @property({ type: Boolean }) isStale = false;
  @property({ type: String }) lastSuccessfulCheckAt: string | null = null;
  @property({ type: Boolean }) lastCheckSucceeded: boolean | null = null;
  @property({ type: String }) lastCheckError: string | null = null;

  static styles = css`
    :host { display: block; }
    .stale-warning {
      margin-top: 8px;
      padding: 8px 12px;
      background: var(--uui-color-warning-surface, #fff3cd);
      border: 1px solid var(--uui-color-warning, #ffc107);
      border-radius: 4px;
      color: var(--uui-color-warning-contrast, #856404);
      font-size: 0.875rem;
    }
    .failure-notice {
      margin-top: 8px;
      padding: 8px 12px;
      background: var(--uui-color-danger-surface, #fde8e8);
      border: 1px solid var(--uui-color-danger, #d0011b);
      border-radius: 4px;
      color: var(--uui-color-danger-contrast, #7b0018);
      font-size: 0.875rem;
    }
  `;

  render() {
    const showStale = this.isStale;
    const showFailure = this.lastCheckSucceeded === false;

    if (!showStale && !showFailure) return html``;

    return html`
      ${showStale ? html`
        <div class="stale-warning">
          <strong>Data may be outdated</strong> — the last successful check was more than 48 hours ago.
        </div>
      ` : ''}
      ${showFailure ? html`
        <div class="failure-notice">
          <strong>Last check failed</strong>${this.lastCheckError ? `: ${this.lastCheckError}` : '.'}
        </div>
      ` : ''}
    `;
  }
}
