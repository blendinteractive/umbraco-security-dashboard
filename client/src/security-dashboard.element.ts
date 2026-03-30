import { LitElement, html, css } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

import './components/status-indicator.element.js';
import './components/staleness-warning.element.js';
import './components/advisory-list.element.js';
import './components/check-schedule.element.js';

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

interface DashboardStatusResponse {
  overallStatus: 'Safe' | 'Vulnerable' | 'NeverChecked';
  isStale: boolean;
  lastSuccessfulCheckAt: string | null;
  lastCheckAttemptAt: string | null;
  lastCheckSucceeded: boolean | null;
  lastCheckError: string | null;
  nextScheduledCheckAt: string;
  affectedAdvisoryCount: number;
  advisories: AdvisoryDto[];
}

@customElement('security-dashboard')
export class SecurityDashboardElement extends LitElement {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-space-5, 16px);
    }
    .error-box {
      color: var(--uui-color-danger-standalone);
    }
  `;

  @state() private _loading = true;
  @state() private _error: string | null = null;
  @state() private _data: DashboardStatusResponse | null = null;

  override connectedCallback(): void {
    super.connectedCallback();
    this._fetchStatus();
  }

  private async _fetchStatus(): Promise<void> {
    this._loading = true;
    this._error = null;

    try {
      const response = await fetch('/umbraco/management/api/v1/security-dashboard/status', {
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
      });

      if (!response.ok) {
        this._error = `Request failed: ${response.status} ${response.statusText}`;
        return;
      }

      this._data = await response.json() as DashboardStatusResponse;
    } catch (err) {
      this._error = err instanceof Error ? err.message : 'Unknown error';
    } finally {
      this._loading = false;
    }
  }

  override render() {
    if (this._loading) {
      return html`<uui-loader aria-label="Loading vulnerability status"></uui-loader>`;
    }

    if (this._error) {
      return html`
        <uui-box>
          <p class="error-box" role="alert">Failed to load vulnerability status: ${this._error}</p>
        </uui-box>
      `;
    }

    if (!this._data) return html``;

    const affectedAdvisories = this._data.advisories.filter(
      (a) => a.affectedStatus === 'Affected' || a.affectedStatus === 'Unknown',
    );
    const notAffectedAdvisories = this._data.advisories.filter(
      (a) => a.affectedStatus === 'NotAffected',
    );

    return html`
      <uui-box headline="Vulnerability Status">
        <section aria-live="polite" aria-label="Current vulnerability status">
        <security-status-indicator
          .overallStatus=${this._data.overallStatus}
          .isStale=${this._data.isStale}
          .affectedCount=${this._data.affectedAdvisoryCount}>
        </security-status-indicator>

        ${this._data.isStale
          ? html`<security-staleness-warning
                  .lastSuccessfulCheckAt=${this._data.lastSuccessfulCheckAt}>
                </security-staleness-warning>`
          : ''}

        <security-check-schedule
          .lastCheckAttemptAt=${this._data.lastCheckAttemptAt}
          .lastCheckSucceeded=${this._data.lastCheckSucceeded}
          .lastCheckError=${this._data.lastCheckError}
          .nextScheduledCheckAt=${this._data.nextScheduledCheckAt}>
        </security-check-schedule>
        </section>
      </uui-box>

      ${affectedAdvisories.length > 0
        ? html`
          <uui-box headline="Affected Packages">
            <security-advisory-list .advisories=${affectedAdvisories}></security-advisory-list>
          </uui-box>
        `
        : ''}

      ${notAffectedAdvisories.length > 0
        ? html`
          <uui-box headline="All Advisories (Not Affected)">
            <security-advisory-list .advisories=${notAffectedAdvisories}></security-advisory-list>
          </uui-box>
        `
        : ''}
    `;
  }
}

export default SecurityDashboardElement;
