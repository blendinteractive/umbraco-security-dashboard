import { LitElement, html, css } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { DashboardStatusResponse } from './types.js';
import './components/dashboard-header.element.js';
import './components/advisory-list.element.js';

@customElement('security-dashboard')
export class SecurityDashboardElement extends UmbElementMixin(LitElement) {
  @state() private _status: DashboardStatusResponse | null = null;
  @state() private _loading = true;
  @state() private _error: string | null = null;

  static styles = css`
    :host { display: block; padding: 24px; }
    h2 { margin: 0 0 16px; font-size: 1.4rem; }
    .error-box {
      padding: 16px;
      background: var(--uui-color-danger-surface, #fde8e8);
      border: 1px solid var(--uui-color-danger, #d0011b);
      border-radius: 4px;
      color: var(--uui-color-danger-contrast, #7b0018);
    }
  `;

  connectedCallback() {
    super.connectedCallback();
    this._fetchStatus();
  }

  private async _fetchStatus() {
    this._loading = true;
    this._error = null;
    try {
      const authContext = await this.getContext(UMB_AUTH_CONTEXT);
      const token = await authContext.getLatestToken();

      const response = await fetch('/umbraco/management/api/v1/security-dashboard/status', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        }
      });

      if (response.status === 401) {
        this._error = 'Unauthorized. Please log in to the Umbraco backoffice.';
        return;
      }

      if (!response.ok) {
        this._error = `Server error (${response.status}). Please try again later.`;
        return;
      }

      this._status = await response.json() as DashboardStatusResponse;
    } catch (err) {
      this._error = err instanceof Error ? err.message : 'Failed to load vulnerability status.';
    } finally {
      this._loading = false;
    }
  }

  render() {
    if (this._loading) {
      return html`
        <uui-box>
          <h2>Security Health</h2>
          <uui-loader></uui-loader>
        </uui-box>
      `;
    }

    if (this._error) {
      return html`
        <uui-box>
          <h2>Security Health</h2>
          <div class="error-box">${this._error}</div>
        </uui-box>
      `;
    }

    const s = this._status!;

    return html`
      <uui-box>
        <h2>Security Health</h2>

        <security-dashboard-header
          .status=${s}>
        </security-dashboard-header>

        ${s.advisories.length > 0 ? html`
          <security-dashboard-advisory-list
            .advisories=${s.advisories}>
          </security-dashboard-advisory-list>
        ` : ''}
      </uui-box>
    `;
  }
}

export default SecurityDashboardElement;
