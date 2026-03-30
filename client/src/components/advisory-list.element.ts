import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import './advisory-item.element.js';

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

@customElement('security-advisory-list')
export class AdvisoryListElement extends LitElement {
  static styles = css`
    :host {
      display: block;
    }
    .empty-message {
      color: var(--uui-color-disabled-standalone, #999);
      font-style: italic;
    }
  `;

  @property({ type: Array }) advisories: AdvisoryDto[] = [];

  override render() {
    if (this.advisories.length === 0) {
      return html`<p class="empty-message">No advisories in this category.</p>`;
    }

    return html`
      ${this.advisories.map(
        (advisory) => html`
          <security-advisory-item .advisory=${advisory}></security-advisory-item>
        `,
      )}
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'security-advisory-list': AdvisoryListElement;
  }
}
