import { LitElement, html, css } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { AuditLogEntryDto, AuditLogPageResponse } from '../types.js';

const PAGE_SIZE = 25;

@customElement('security-audit-log')
export class AuditLogElement extends UmbElementMixin(LitElement) {
  @state() private _entries: AuditLogEntryDto[] = [];
  @state() private _totalCount = 0;
  @state() private _skip = 0;
  @state() private _loading = true;
  @state() private _error: string | null = null;

  static styles = css`
    :host { display: block; margin-top: 24px; }
    h3 { margin: 0 0 12px; font-size: 1rem; font-weight: 600; }
    .empty-state {
      padding: 16px;
      text-align: center;
      color: var(--uui-color-text-alt, #666);
      font-style: italic;
    }
    .pagination { margin-top: 12px; display: flex; justify-content: center; }
    .actor-none { color: var(--uui-color-text-alt, #999); font-style: italic; }
  `;

  connectedCallback() {
    super.connectedCallback();
    this._fetchPage(0);
  }

  private async _fetchPage(skip: number) {
    this._loading = true;
    this._error = null;
    try {
      const authContext = await this.getContext(UMB_AUTH_CONTEXT);
      if (!authContext) {
        this._error = 'Authentication context not available.';
        return;
      }
      const token = await authContext.getLatestToken();

      const response = await fetch(
        `/umbraco/management/api/v1/security-dashboard/audit-log?skip=${skip}&take=${PAGE_SIZE}`,
        { headers: { 'Authorization': `Bearer ${token}` } }
      );

      if (!response.ok) {
        this._error = `Server error (${response.status}).`;
        return;
      }

      const data = await response.json() as AuditLogPageResponse;
      this._entries = data.entries;
      this._totalCount = data.totalCount;
      this._skip = skip;
    } catch (err) {
      this._error = err instanceof Error ? err.message : 'Failed to load audit log.';
    } finally {
      this._loading = false;
    }
  }

  private _onPageChange(e: CustomEvent) {
    const newSkip = (e.detail.current - 1) * PAGE_SIZE;
    this._fetchPage(newSkip);
  }

  private _formatTimestamp(iso: string): string {
    return new Date(iso).toLocaleString();
  }

  render() {
    return html`
      <uui-box>
        <h3>Audit History</h3>

        ${this._loading ? html`<uui-loader></uui-loader>` : ''}

        ${this._error ? html`<p>${this._error}</p>` : ''}

        ${!this._loading && !this._error && this._totalCount === 0 ? html`
          <p class="empty-state">No audit log entries yet.</p>
        ` : ''}

        ${!this._loading && !this._error && this._totalCount > 0 ? html`
          <uui-table>
            <uui-table-head>
              <uui-table-head-cell>Timestamp</uui-table-head-cell>
              <uui-table-head-cell>Status</uui-table-head-cell>
              <uui-table-head-cell>Type</uui-table-head-cell>
              <uui-table-head-cell>Actor</uui-table-head-cell>
              <uui-table-head-cell>Description</uui-table-head-cell>
            </uui-table-head>
            ${this._entries.map(entry => html`
              <uui-table-row>
                <uui-table-cell>${this._formatTimestamp(entry.timestamp)}</uui-table-cell>
                <uui-table-cell>${entry.overallStatus}</uui-table-cell>
                <uui-table-cell>${entry.actionType}</uui-table-cell>
                <uui-table-cell>
                  ${entry.actorName
                    ? entry.actorName
                    : html`<span class="actor-none">System</span>`}
                </uui-table-cell>
                <uui-table-cell>${entry.description}</uui-table-cell>
              </uui-table-row>
            `)}
          </uui-table>

          ${this._totalCount > PAGE_SIZE ? html`
            <div class="pagination">
              <uui-pagination
                .total=${Math.ceil(this._totalCount / PAGE_SIZE)}
                .current=${Math.floor(this._skip / PAGE_SIZE) + 1}
                @change=${this._onPageChange}>
              </uui-pagination>
            </div>
          ` : ''}
        ` : ''}
      </uui-box>
    `;
  }
}
