import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('security-check-schedule')
export class CheckScheduleElement extends LitElement {
  static styles = css`
    :host {
      display: block;
      font-size: var(--uui-type-small-size, 13px);
      color: var(--uui-color-text-alt, #666);
      margin-top: var(--uui-size-space-3, 8px);
    }
    .schedule-row {
      display: flex;
      gap: var(--uui-size-space-2, 6px);
      align-items: center;
      margin-bottom: var(--uui-size-space-1, 4px);
    }
    .error-text {
      color: var(--uui-color-danger-standalone, #d42054);
    }
  `;

  @property({ type: String }) lastCheckAttemptAt: string | null = null;
  @property({ type: Boolean }) lastCheckSucceeded: boolean | null = null;
  @property({ type: String }) lastCheckError: string | null = null;
  @property({ type: String }) nextScheduledCheckAt = '';

  override render() {
    const nextCheckDate = this.nextScheduledCheckAt
      ? new Date(this.nextScheduledCheckAt).toLocaleString()
      : 'Unknown';

    const lastCheckDate = this.lastCheckAttemptAt
      ? new Date(this.lastCheckAttemptAt).toLocaleString()
      : 'Never';

    return html`
      <div class="schedule-row">
        <uui-icon name="time"></uui-icon>
        <span>Last check: ${lastCheckDate}</span>
        ${this.lastCheckSucceeded === false
          ? html`<uui-icon name="alert" style="color: var(--uui-color-danger-standalone)"></uui-icon>`
          : ''}
      </div>

      ${this.lastCheckError
        ? html`
          <div class="schedule-row">
            <span class="error-text">Last error: ${this.lastCheckError}</span>
          </div>
        `
        : ''}

      <div class="schedule-row">
        <uui-icon name="time"></uui-icon>
        <span>Next check: ${nextCheckDate}</span>
      </div>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'security-check-schedule': CheckScheduleElement;
  }
}
