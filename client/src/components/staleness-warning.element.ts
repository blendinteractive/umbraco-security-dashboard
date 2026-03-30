import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('security-staleness-warning')
export class StalenessWarningElement extends LitElement {
  static styles = css`
    :host {
      display: block;
      margin-top: var(--uui-size-space-3, 8px);
    }
    .stale-warning {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2, 6px);
      color: var(--uui-color-warning-standalone, #f5a623);
      font-size: var(--uui-type-small-size, 13px);
    }
  `;

  @property({ type: String }) lastSuccessfulCheckAt: string | null = null;

  override render() {
    const dateStr = this.lastSuccessfulCheckAt
      ? new Date(this.lastSuccessfulCheckAt).toLocaleString()
      : 'unknown';

    return html`
      <div class="stale-warning">
        <uui-icon name="alert"></uui-icon>
        <span>
          Data may be out of date — last successful check was ${dateStr}.
          The scheduled check will update this automatically.
        </span>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'security-staleness-warning': StalenessWarningElement;
  }
}
