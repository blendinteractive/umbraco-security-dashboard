import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('security-dashboard-status-indicator')
export class StatusIndicatorElement extends LitElement {
  @property({ type: String }) overallStatus: 'Safe' | 'Mitigated' | 'Vulnerable' | 'NeverChecked' = 'NeverChecked';
  @property({ type: Number }) affectedAdvisoryCount = 0;
  @property({ type: Number }) mitigatedAdvisoryCount = 0;

  static styles = css`
    :host { display: block; }
    .status-safe { color: var(--uui-color-positive, #00a152); display: flex; align-items: center; gap: 8px; }
    .status-mitigated { color: var(--uui-color-warning, #f5a623); display: flex; align-items: center; gap: 8px; }
    .status-vulnerable { color: var(--uui-color-danger, #d0011b); display: flex; align-items: center; gap: 8px; }
    .status-neutral { color: var(--uui-color-text, #333); display: flex; align-items: center; gap: 8px; }
    .status-label { font-size: 1.2rem; font-weight: 600; }
  `;

  render() {
    if (this.overallStatus === 'Safe') {
      return html`
        <div class="status-safe">
          <uui-icon name="check-circle" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">No Active Vulnerabilities</span>
        </div>
      `;
    }

    if (this.overallStatus === 'Mitigated') {
      return html`
        <div class="status-mitigated">
          <uui-icon name="shield" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">
            ${this.mitigatedAdvisoryCount}
            ${this.mitigatedAdvisoryCount === 1 ? 'Vulnerability' : 'Vulnerabilities'} Mitigated
          </span>
        </div>
      `;
    }

    if (this.overallStatus === 'Vulnerable') {
      const mitigatedSuffix = this.mitigatedAdvisoryCount > 0
        ? ` and ${this.mitigatedAdvisoryCount} Mitigated`
        : '';
      return html`
        <div class="status-vulnerable">
          <uui-icon name="alert" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">
            ${this.affectedAdvisoryCount} Active${mitigatedSuffix}
            ${this.affectedAdvisoryCount === 1 ? 'Vulnerability' : 'Vulnerabilities'} Found
          </span>
        </div>
      `;
    }

    return html`
      <div class="status-neutral">
        <uui-icon name="info" style="font-size: 2rem;"></uui-icon>
        <span class="status-label">Not yet checked</span>
      </div>
    `;
  }
}
