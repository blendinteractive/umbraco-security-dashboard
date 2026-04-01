import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { AdvisoryDto } from '../types.js';
import './advisory-item.element.js';

@customElement('security-dashboard-advisory-list')
export class AdvisoryListElement extends LitElement {
  @property({ type: Array }) advisories: AdvisoryDto[] = [];

  static styles = css`
    :host { display: block; }
    h3 { margin: 16px 0 8px; font-size: 1rem; font-weight: 600; }
    .empty-state {
      padding: 16px;
      text-align: center;
      color: var(--uui-color-text-alt, #666);
      font-style: italic;
    }
  `;

  render() {
    const affectedGroup = this.advisories.filter(
      a => a.affectedStatus === 'Affected' || a.affectedStatus === 'Unknown'
    );
    const notAffectedGroup = this.advisories.filter(
      a => a.affectedStatus === 'NotAffected'
    );

    return html`
      <h3>Active Vulnerabilities</h3>
      ${affectedGroup.length === 0
        ? html`<div class="empty-state">No active vulnerabilities found.</div>`
        : affectedGroup.map(a => html`
            <security-dashboard-advisory-item .advisory=${a}></security-dashboard-advisory-item>
          `)
      }

      ${notAffectedGroup.length > 0 ? html`
        <h3>Known Advisories</h3>
        ${notAffectedGroup.map(a => html`
          <security-dashboard-advisory-item .advisory=${a}></security-dashboard-advisory-item>
        `)}
      ` : ''}
    `;
  }
}
