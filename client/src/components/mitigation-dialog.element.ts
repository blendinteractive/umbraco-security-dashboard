import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';

@customElement('security-dashboard-mitigation-dialog')
export class MitigationDialogElement extends UmbElementMixin(LitElement) {
  @property() mode: 'mark' | 'remove' = 'mark';
  @property() ghsaId: string = '';

  @state() private _description = '';
  @state() private _submitting = false;
  @state() private _error: string | null = null;

  static styles = css`
    .dialog-error {
      color: var(--uui-color-danger, #d0011b);
      font-size: 0.875rem;
      margin-top: 8px;
    }
    uui-textarea {
      width: 100%;
      display: block;
      margin-top: 8px;
    }
  `;

  private async _getToken(): Promise<string> {
    const authContext = await this.getContext(UMB_AUTH_CONTEXT);
    return authContext.getLatestToken();
  }

  private async _handleMark() {
    if (!this._description.trim()) return;

    this._submitting = true;
    this._error = null;
    try {
      const token = await this._getToken();
      const response = await fetch(
        `/umbraco/management/api/v1/security-dashboard/advisories/${encodeURIComponent(this.ghsaId)}/mitigations`,
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ description: this._description }),
        }
      );

      if (!response.ok) {
        if (response.status === 409) {
          this._error = 'This advisory is already marked as mitigated.';
        } else {
          this._error = `Unexpected error (${response.status}). Please try again.`;
        }
        return;
      }

      this.dispatchEvent(new CustomEvent('mitigation-changed', { bubbles: true, composed: true }));
    } catch {
      this._error = 'Failed to save mitigation. Please try again.';
    } finally {
      this._submitting = false;
    }
  }

  private async _handleRemove() {
    this._submitting = true;
    this._error = null;
    try {
      const token = await this._getToken();
      const response = await fetch(
        `/umbraco/management/api/v1/security-dashboard/advisories/${encodeURIComponent(this.ghsaId)}/mitigations`,
        {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` },
        }
      );

      if (!response.ok) {
        this._error = `Unexpected error (${response.status}). Please try again.`;
        return;
      }

      this.dispatchEvent(new CustomEvent('mitigation-changed', { bubbles: true, composed: true }));
    } catch {
      this._error = 'Failed to remove mitigation. Please try again.';
    } finally {
      this._submitting = false;
    }
  }

  private _handleCancel() {
    this.dispatchEvent(new CustomEvent('mitigation-cancelled', { bubbles: true, composed: true }));
  }

  render() {
    if (this.mode === 'remove') {
      return html`
        <uui-dialog>
          <uui-dialog-layout headline="Remove Mitigation">
            <p>Are you sure you want to remove this manual mitigation? The advisory will revert to its automatically calculated status.</p>
            ${this._error ? html`<div class="dialog-error">${this._error}</div>` : ''}
            <div slot="actions">
              <uui-button
                look="secondary"
                @click=${this._handleCancel}
                ?disabled=${this._submitting}
              >Cancel</uui-button>
              <uui-button
                look="primary"
                color="danger"
                @click=${this._handleRemove}
                ?disabled=${this._submitting}
              >${this._submitting ? 'Removing...' : 'Remove Mitigation'}</uui-button>
            </div>
          </uui-dialog-layout>
        </uui-dialog>
      `;
    }

    return html`
      <uui-dialog>
        <uui-dialog-layout headline="Mark As Mitigated">
          <p>Describe how this vulnerability has been mitigated (e.g. compensating controls, configuration changes, WAF rules).</p>
          <uui-textarea
            label="Description"
            name="description"
            placeholder="Describe the mitigation..."
            .value=${this._description}
            @input=${(e: Event) => { this._description = (e.target as { value: string }).value; }}
          ></uui-textarea>
          ${this._error ? html`<div class="dialog-error">${this._error}</div>` : ''}
          <div slot="actions">
            <uui-button
              look="secondary"
              @click=${this._handleCancel}
              ?disabled=${this._submitting}
            >Cancel</uui-button>
            <uui-button
              look="primary"
              color="positive"
              @click=${this._handleMark}
              ?disabled=${!this._description.trim() || this._submitting}
            >${this._submitting ? 'Saving...' : 'Mark As Mitigated'}</uui-button>
          </div>
        </uui-dialog-layout>
      </uui-dialog>
    `;
  }
}
