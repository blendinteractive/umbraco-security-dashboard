import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import type { AdvisoryDto } from '../types.js';
import './mitigation-dialog.element.js';

@customElement('security-dashboard-advisory-item')
export class AdvisoryItemElement extends LitElement {
  @property({ type: Object }) advisory!: AdvisoryDto;
  @state() private _showMarkDialog = false;
  @state() private _showRemoveDialog = false;

  static styles = css`
    :host { display: contents; }
    .advisory-item {
      border-bottom: 1px solid var(--uui-color-border, #e0e0e0);
    }
    .advisory-row {
      display: grid;
      grid-template-columns: 1fr auto auto auto;
      align-items: center;
      gap: 12px;
      padding: 12px 0;
    }
    .advisory-title {
      font-weight: 500;
    }
    .advisory-package {
      font-size: 0.8rem;
      color: var(--uui-color-text-alt, #666);
      margin-top: 2px;
    }
    .advisory-package uui-tag {
      font-size: 0.7rem;
      vertical-align: middle;
      margin-left: 4px;
    }
    .mitigation-attribution {
      font-size: 0.8rem;
      color: var(--uui-color-text-alt, #666);
      margin-top: 4px;
      padding: 6px 8px;
      background: var(--uui-color-surface-alt, #f5f5f5);
      border-radius: 4px;
      border-left: 3px solid var(--uui-color-positive, #0a7a0a);
    }
    .mitigation-attribution .attribution-who {
      font-weight: 500;
    }
    .mitigation-attribution .attribution-description {
      margin-top: 2px;
      font-style: italic;
    }
    .badges { display: flex; gap: 6px; align-items: center; }
  `;

  private getSeverityColor(severity: string): string {
    switch (severity) {
      case 'Critical': return 'danger';
      case 'High': return 'warning';
      case 'Moderate': return 'warning';
      case 'Low': return 'default';
      default: return 'default';
    }
  }

  private getStatusColor(status: string): string {
    switch (status) {
      case 'Vulnerable': return 'danger';
      case 'Mitigated': return 'danger';
      case 'Unknown': return 'warning';
      case 'NotAffected': return 'positive';
      default: return 'default';
    }
  }

  private _onMitigationChanged() {
    this._showMarkDialog = false;
    this._showRemoveDialog = false;
  }

  render() {
    if (!this.advisory) return html``;

    var packageInfo:string = '';
    this.advisory.packages.forEach(pkg => {
      if (packageInfo.includes(pkg.packageName + ' — ')) {
        packageInfo += `${pkg.affectedVersionRange}, `;
      } else {
        packageInfo += `${pkg.packageName} — ${pkg.affectedVersionRange}, `;
      }
    });

    const canMark = (this.advisory.affectedStatus === 'Vulnerable' || this.advisory.affectedStatus === 'Unknown')
      && !this.advisory.manualMitigation;
    const canRemove = this.advisory.affectedStatus === 'Mitigated' && !!this.advisory.manualMitigation;

    const mitigatedAt = this.advisory.manualMitigation
      ? new Date(this.advisory.manualMitigation.mitigatedAt).toLocaleDateString()
      : '';

    return html`
      <div class="advisory-item">
        <div class="advisory-row">
          <div>
            <div class="advisory-title">${this.advisory.title}</div>
            <div class="advisory-package">
              <uui-tag color="${this.getSeverityColor(this.advisory.severity)}">
                ${this.advisory.severity}
              </uui-tag>
              ${packageInfo.slice(0, -2)}
            </div>
            ${this.advisory.manualMitigation ? html`
              <div class="mitigation-attribution">
                <span class="attribution-who">
                  Manually mitigated by ${this.advisory.manualMitigation.mitigatedBy} on ${mitigatedAt}
                </span>
                <div class="attribution-description">${this.advisory.manualMitigation.description}</div>
              </div>
            ` : this.advisory.affectedStatus === 'Mitigated' && this.advisory.exposureCheckMitigationDescription ? html`
              <div class="mitigation-attribution">
                <span class="attribution-who">Auto-mitigated</span>
                <div class="attribution-description">${this.advisory.exposureCheckMitigationDescription}</div>
              </div>
            ` : ''}
          </div>
          ${canMark ? html`
            <uui-button
              look="outline"
              color="positive"
              @click=${() => { this._showMarkDialog = true; }}>
              Mark As Mitigated
            </uui-button>
          ` : canRemove ? html`
            <uui-button
              look="secondary"
              color="danger"
              @click=${() => { this._showRemoveDialog = true; }}>
              Remove Mitigation
            </uui-button>
          ` : html`<span></span>`}
          <div class="badges">
            <uui-tag
              color="${this.getStatusColor(this.advisory.affectedStatus)}"
              look="${this.advisory.affectedStatus === 'Mitigated' ? 'outline' : 'primary'}"
            >
              ${this.advisory.affectedStatus === 'NotAffected' ? 'Not Affected' : this.advisory.affectedStatus}
            </uui-tag>
          </div>
          <uui-button
            look="secondary"
            href="${this.advisory.advisoryUrl}"
            target="_blank"
            rel="noopener noreferrer"
            label="More info about ${this.advisory.ghsaId}">
            More Info →
          </uui-button>

        </div>
        ${this._showMarkDialog ? html`
          <security-dashboard-mitigation-dialog
            mode="mark"
            ghsaId=${this.advisory.ghsaId}
            @mitigation-changed=${this._onMitigationChanged}
            @mitigation-cancelled=${() => { this._showMarkDialog = false; }}
          ></security-dashboard-mitigation-dialog>
        ` : ''}
        ${this._showRemoveDialog ? html`
          <security-dashboard-mitigation-dialog
            mode="remove"
            ghsaId=${this.advisory.ghsaId}
            @mitigation-changed=${this._onMitigationChanged}
            @mitigation-cancelled=${() => { this._showRemoveDialog = false; }}
          ></security-dashboard-mitigation-dialog>
        ` : ''}
      </div>
    `;
  }
}
