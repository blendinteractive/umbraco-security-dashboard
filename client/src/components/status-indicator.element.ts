import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('security-status-indicator')
export class StatusIndicatorElement extends LitElement {
  static styles = css`
    :host {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3, 8px);
      margin-bottom: var(--uui-size-space-4, 12px);
    }
    .status-label {
      font-size: var(--uui-type-large-size, 18px);
      font-weight: bold;
    }
    .safe {
      color: var(--uui-color-positive-standalone, #27a344);
    }
    .vulnerable {
      color: var(--uui-color-danger-standalone, #d42054);
    }
    .unknown {
      color: var(--uui-color-disabled-standalone, #999);
    }
  `;

  @property({ type: String }) overallStatus: 'Safe' | 'Vulnerable' | 'NeverChecked' = 'NeverChecked';
  @property({ type: Boolean }) isStale = false;
  @property({ type: Number }) affectedCount = 0;

  override render() {
    switch (this.overallStatus) {
      case 'Safe':
        return html`
          <uui-icon name="check" class="safe"></uui-icon>
          <span class="status-label safe">No Active Vulnerabilities</span>
        `;
      case 'Vulnerable':
        return html`
          <uui-icon name="alert" class="vulnerable"></uui-icon>
          <span class="status-label vulnerable">
            ${this.affectedCount} ${this.affectedCount === 1 ? 'Vulnerability' : 'Vulnerabilities'} Found
          </span>
        `;
      case 'NeverChecked':
      default:
        return html`
          <uui-icon name="info" class="unknown"></uui-icon>
          <span class="status-label unknown">Not yet checked</span>
        `;
    }
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'security-status-indicator': StatusIndicatorElement;
  }
}
