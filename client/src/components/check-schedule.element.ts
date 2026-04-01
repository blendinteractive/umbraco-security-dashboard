import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('security-dashboard-check-schedule')
export class CheckScheduleElement extends LitElement {
  @property({ type: String }) lastSuccessfulCheckAt: string | null = null;
  @property({ type: String }) nextScheduledCheckAt = '';

  static styles = css`
    :host { display: block; }
    .schedule-row {
      display: flex;
      gap: 24px;
      flex-wrap: wrap;
      margin-top: 12px;
    }
    .schedule-item { display: flex; flex-direction: column; }
    .schedule-label { font-size: 0.75rem; color: var(--uui-color-text-alt, #666); text-transform: uppercase; letter-spacing: 0.05em; }
    .schedule-value { font-size: 0.95rem; font-weight: 500; margin-top: 2px; }
  `;

  private formatTimestamp(iso: string | null): string {
    if (!iso) return 'Not yet run';

    const date = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffH = Math.floor(diffMin / 60);
    const diffDays = Math.floor(diffH / 24);

    if (diffMin < 1) return 'Just now';
    if (diffMin < 60) return `${diffMin} minute${diffMin === 1 ? '' : 's'} ago`;
    if (diffH < 24) return `${diffH} hour${diffH === 1 ? '' : 's'} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;

    return new Intl.DateTimeFormat(undefined, {
      month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit'
    }).format(date);
  }

  private formatFutureTimestamp(iso: string): string {
    const date = new Date(iso);
    const now = new Date();
    const diffMs = date.getTime() - now.getTime();
    const diffMin = Math.ceil(diffMs / 60000);
    const diffH = Math.ceil(diffMin / 60);

    if (diffMin < 60) return `In ${diffMin} minute${diffMin === 1 ? '' : 's'}`;
    if (diffH < 24) return `In ${diffH} hour${diffH === 1 ? '' : 's'}`;

    return new Intl.DateTimeFormat(undefined, {
      month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit'
    }).format(date);
  }

  render() {
    return html`
      <div class="schedule-row">
        <div class="schedule-item">
          <span class="schedule-label">Last Check</span>
          <span class="schedule-value">${this.formatTimestamp(this.lastSuccessfulCheckAt)}</span>
        </div>
        <div class="schedule-item">
          <span class="schedule-label">Next Check</span>
          <span class="schedule-value">${this.formatFutureTimestamp(this.nextScheduledCheckAt)}</span>
        </div>
      </div>
    `;
  }
}
