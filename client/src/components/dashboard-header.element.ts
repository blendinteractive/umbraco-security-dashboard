import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { DashboardStatusResponse } from '../types.js';
import './status-indicator.element.js';
import './staleness-warning.element.js';
import './check-schedule.element.js';

const STATUS_IMAGES: Record<string, string> = {
  Safe: '/App_Plugins/SecurityDashboard/images/status-safe.png',
  Mitigated: '/App_Plugins/SecurityDashboard/images/status-mitigated.png',
  Vulnerable: '/App_Plugins/SecurityDashboard/images/status-vulnerable.png',
  NeverChecked: '/App_Plugins/SecurityDashboard/images/status-never-checked.png',
};

@customElement('security-dashboard-header')
export class DashboardHeaderElement extends LitElement {
  @property({ attribute: false }) status!: DashboardStatusResponse;

  static styles = css`
    :host { display: flex; align-items: flex-start; gap: 16px; }
    .status-image { width: 100px; height: 100px; flex-shrink: 0; }
    .content { flex: 1; }
  `;

  render() {
    const s = this.status;
    const imgSrc = STATUS_IMAGES[s.overallStatus] ?? STATUS_IMAGES['NeverChecked'];
    return html`
      <img class="status-image" src=${imgSrc} alt=${s.overallStatus} />
      <div class="content">
        <security-dashboard-status-indicator
          .overallStatus=${s.overallStatus}
          .affectedAdvisoryCount=${s.affectedAdvisoryCount}
          .mitigatedAdvisoryCount=${s.mitigatedAdvisoryCount}>
        </security-dashboard-status-indicator>

        <security-dashboard-staleness-warning
          .isStale=${s.isStale}
          .scanningDisabled=${s.scanningDisabled}
          .lastSuccessfulCheckAt=${s.lastSuccessfulCheckAt}
          .lastCheckSucceeded=${s.lastCheckSucceeded}
          .lastCheckError=${s.lastCheckError}>
        </security-dashboard-staleness-warning>

        <security-dashboard-check-schedule
          .lastSuccessfulCheckAt=${s.lastSuccessfulCheckAt}
          .nextScheduledCheckAt=${s.nextScheduledCheckAt}>
        </security-dashboard-check-schedule>
      </div>
    `;
  }
}
