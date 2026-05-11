import { UmbElementMixin as St } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT as wt } from "@umbraco-cms/backoffice/auth";
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const q = globalThis, rt = q.ShadowRoot && (q.ShadyCSS === void 0 || q.ShadyCSS.nativeShadow) && "adoptedStyleSheets" in Document.prototype && "replace" in CSSStyleSheet.prototype, ot = Symbol(), ut = /* @__PURE__ */ new WeakMap();
let Ct = class {
  constructor(t, e, i) {
    if (this._$cssResult$ = !0, i !== ot) throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");
    this.cssText = t, this.t = e;
  }
  get styleSheet() {
    let t = this.o;
    const e = this.t;
    if (rt && t === void 0) {
      const i = e !== void 0 && e.length === 1;
      i && (t = ut.get(e)), t === void 0 && ((this.o = t = new CSSStyleSheet()).replaceSync(this.cssText), i && ut.set(e, t));
    }
    return t;
  }
  toString() {
    return this.cssText;
  }
};
const Dt = (r) => new Ct(typeof r == "string" ? r : r + "", void 0, ot), b = (r, ...t) => {
  const e = r.length === 1 ? r[0] : t.reduce((i, s, o) => i + ((a) => {
    if (a._$cssResult$ === !0) return a.cssText;
    if (typeof a == "number") return a;
    throw Error("Value passed to 'css' function must be a 'css' function result: " + a + ". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.");
  })(s) + r[o + 1], r[0]);
  return new Ct(e, r, ot);
}, Tt = (r, t) => {
  if (rt) r.adoptedStyleSheets = t.map((e) => e instanceof CSSStyleSheet ? e : e.styleSheet);
  else for (const e of t) {
    const i = document.createElement("style"), s = q.litNonce;
    s !== void 0 && i.setAttribute("nonce", s), i.textContent = e.cssText, r.appendChild(i);
  }
}, dt = rt ? (r) => r : (r) => r instanceof CSSStyleSheet ? ((t) => {
  let e = "";
  for (const i of t.cssRules) e += i.cssText;
  return Dt(e);
})(r) : r;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const { is: Ut, defineProperty: Nt, getOwnPropertyDescriptor: Ht, getOwnPropertyNames: Rt, getOwnPropertySymbols: jt, getPrototypeOf: zt } = Object, $ = globalThis, ht = $.trustedTypes, It = ht ? ht.emptyScript : "", tt = $.reactiveElementPolyfillSupport, H = (r, t) => r, J = { toAttribute(r, t) {
  switch (t) {
    case Boolean:
      r = r ? It : null;
      break;
    case Object:
    case Array:
      r = r == null ? r : JSON.stringify(r);
  }
  return r;
}, fromAttribute(r, t) {
  let e = r;
  switch (t) {
    case Boolean:
      e = r !== null;
      break;
    case Number:
      e = r === null ? null : Number(r);
      break;
    case Object:
    case Array:
      try {
        e = JSON.parse(r);
      } catch {
        e = null;
      }
  }
  return e;
} }, at = (r, t) => !Ut(r, t), pt = { attribute: !0, type: String, converter: J, reflect: !1, useDefault: !1, hasChanged: at };
Symbol.metadata ?? (Symbol.metadata = Symbol("metadata")), $.litPropertyMetadata ?? ($.litPropertyMetadata = /* @__PURE__ */ new WeakMap());
let k = class extends HTMLElement {
  static addInitializer(t) {
    this._$Ei(), (this.l ?? (this.l = [])).push(t);
  }
  static get observedAttributes() {
    return this.finalize(), this._$Eh && [...this._$Eh.keys()];
  }
  static createProperty(t, e = pt) {
    if (e.state && (e.attribute = !1), this._$Ei(), this.prototype.hasOwnProperty(t) && ((e = Object.create(e)).wrapped = !0), this.elementProperties.set(t, e), !e.noAccessor) {
      const i = Symbol(), s = this.getPropertyDescriptor(t, i, e);
      s !== void 0 && Nt(this.prototype, t, s);
    }
  }
  static getPropertyDescriptor(t, e, i) {
    const { get: s, set: o } = Ht(this.prototype, t) ?? { get() {
      return this[e];
    }, set(a) {
      this[e] = a;
    } };
    return { get: s, set(a) {
      const l = s == null ? void 0 : s.call(this);
      o == null || o.call(this, a), this.requestUpdate(t, l, i);
    }, configurable: !0, enumerable: !0 };
  }
  static getPropertyOptions(t) {
    return this.elementProperties.get(t) ?? pt;
  }
  static _$Ei() {
    if (this.hasOwnProperty(H("elementProperties"))) return;
    const t = zt(this);
    t.finalize(), t.l !== void 0 && (this.l = [...t.l]), this.elementProperties = new Map(t.elementProperties);
  }
  static finalize() {
    if (this.hasOwnProperty(H("finalized"))) return;
    if (this.finalized = !0, this._$Ei(), this.hasOwnProperty(H("properties"))) {
      const e = this.properties, i = [...Rt(e), ...jt(e)];
      for (const s of i) this.createProperty(s, e[s]);
    }
    const t = this[Symbol.metadata];
    if (t !== null) {
      const e = litPropertyMetadata.get(t);
      if (e !== void 0) for (const [i, s] of e) this.elementProperties.set(i, s);
    }
    this._$Eh = /* @__PURE__ */ new Map();
    for (const [e, i] of this.elementProperties) {
      const s = this._$Eu(e, i);
      s !== void 0 && this._$Eh.set(s, e);
    }
    this.elementStyles = this.finalizeStyles(this.styles);
  }
  static finalizeStyles(t) {
    const e = [];
    if (Array.isArray(t)) {
      const i = new Set(t.flat(1 / 0).reverse());
      for (const s of i) e.unshift(dt(s));
    } else t !== void 0 && e.push(dt(t));
    return e;
  }
  static _$Eu(t, e) {
    const i = e.attribute;
    return i === !1 ? void 0 : typeof i == "string" ? i : typeof t == "string" ? t.toLowerCase() : void 0;
  }
  constructor() {
    super(), this._$Ep = void 0, this.isUpdatePending = !1, this.hasUpdated = !1, this._$Em = null, this._$Ev();
  }
  _$Ev() {
    var t;
    this._$ES = new Promise((e) => this.enableUpdating = e), this._$AL = /* @__PURE__ */ new Map(), this._$E_(), this.requestUpdate(), (t = this.constructor.l) == null || t.forEach((e) => e(this));
  }
  addController(t) {
    var e;
    (this._$EO ?? (this._$EO = /* @__PURE__ */ new Set())).add(t), this.renderRoot !== void 0 && this.isConnected && ((e = t.hostConnected) == null || e.call(t));
  }
  removeController(t) {
    var e;
    (e = this._$EO) == null || e.delete(t);
  }
  _$E_() {
    const t = /* @__PURE__ */ new Map(), e = this.constructor.elementProperties;
    for (const i of e.keys()) this.hasOwnProperty(i) && (t.set(i, this[i]), delete this[i]);
    t.size > 0 && (this._$Ep = t);
  }
  createRenderRoot() {
    const t = this.shadowRoot ?? this.attachShadow(this.constructor.shadowRootOptions);
    return Tt(t, this.constructor.elementStyles), t;
  }
  connectedCallback() {
    var t;
    this.renderRoot ?? (this.renderRoot = this.createRenderRoot()), this.enableUpdating(!0), (t = this._$EO) == null || t.forEach((e) => {
      var i;
      return (i = e.hostConnected) == null ? void 0 : i.call(e);
    });
  }
  enableUpdating(t) {
  }
  disconnectedCallback() {
    var t;
    (t = this._$EO) == null || t.forEach((e) => {
      var i;
      return (i = e.hostDisconnected) == null ? void 0 : i.call(e);
    });
  }
  attributeChangedCallback(t, e, i) {
    this._$AK(t, i);
  }
  _$ET(t, e) {
    var o;
    const i = this.constructor.elementProperties.get(t), s = this.constructor._$Eu(t, i);
    if (s !== void 0 && i.reflect === !0) {
      const a = (((o = i.converter) == null ? void 0 : o.toAttribute) !== void 0 ? i.converter : J).toAttribute(e, i.type);
      this._$Em = t, a == null ? this.removeAttribute(s) : this.setAttribute(s, a), this._$Em = null;
    }
  }
  _$AK(t, e) {
    var o, a;
    const i = this.constructor, s = i._$Eh.get(t);
    if (s !== void 0 && this._$Em !== s) {
      const l = i.getPropertyOptions(s), n = typeof l.converter == "function" ? { fromAttribute: l.converter } : ((o = l.converter) == null ? void 0 : o.fromAttribute) !== void 0 ? l.converter : J;
      this._$Em = s;
      const d = n.fromAttribute(e, l.type);
      this[s] = d ?? ((a = this._$Ej) == null ? void 0 : a.get(s)) ?? d, this._$Em = null;
    }
  }
  requestUpdate(t, e, i, s = !1, o) {
    var a;
    if (t !== void 0) {
      const l = this.constructor;
      if (s === !1 && (o = this[t]), i ?? (i = l.getPropertyOptions(t)), !((i.hasChanged ?? at)(o, e) || i.useDefault && i.reflect && o === ((a = this._$Ej) == null ? void 0 : a.get(t)) && !this.hasAttribute(l._$Eu(t, i)))) return;
      this.C(t, e, i);
    }
    this.isUpdatePending === !1 && (this._$ES = this._$EP());
  }
  C(t, e, { useDefault: i, reflect: s, wrapped: o }, a) {
    i && !(this._$Ej ?? (this._$Ej = /* @__PURE__ */ new Map())).has(t) && (this._$Ej.set(t, a ?? e ?? this[t]), o !== !0 || a !== void 0) || (this._$AL.has(t) || (this.hasUpdated || i || (e = void 0), this._$AL.set(t, e)), s === !0 && this._$Em !== t && (this._$Eq ?? (this._$Eq = /* @__PURE__ */ new Set())).add(t));
  }
  async _$EP() {
    this.isUpdatePending = !0;
    try {
      await this._$ES;
    } catch (e) {
      Promise.reject(e);
    }
    const t = this.scheduleUpdate();
    return t != null && await t, !this.isUpdatePending;
  }
  scheduleUpdate() {
    return this.performUpdate();
  }
  performUpdate() {
    var i;
    if (!this.isUpdatePending) return;
    if (!this.hasUpdated) {
      if (this.renderRoot ?? (this.renderRoot = this.createRenderRoot()), this._$Ep) {
        for (const [o, a] of this._$Ep) this[o] = a;
        this._$Ep = void 0;
      }
      const s = this.constructor.elementProperties;
      if (s.size > 0) for (const [o, a] of s) {
        const { wrapped: l } = a, n = this[o];
        l !== !0 || this._$AL.has(o) || n === void 0 || this.C(o, void 0, a, n);
      }
    }
    let t = !1;
    const e = this._$AL;
    try {
      t = this.shouldUpdate(e), t ? (this.willUpdate(e), (i = this._$EO) == null || i.forEach((s) => {
        var o;
        return (o = s.hostUpdate) == null ? void 0 : o.call(s);
      }), this.update(e)) : this._$EM();
    } catch (s) {
      throw t = !1, this._$EM(), s;
    }
    t && this._$AE(e);
  }
  willUpdate(t) {
  }
  _$AE(t) {
    var e;
    (e = this._$EO) == null || e.forEach((i) => {
      var s;
      return (s = i.hostUpdated) == null ? void 0 : s.call(i);
    }), this.hasUpdated || (this.hasUpdated = !0, this.firstUpdated(t)), this.updated(t);
  }
  _$EM() {
    this._$AL = /* @__PURE__ */ new Map(), this.isUpdatePending = !1;
  }
  get updateComplete() {
    return this.getUpdateComplete();
  }
  getUpdateComplete() {
    return this._$ES;
  }
  shouldUpdate(t) {
    return !0;
  }
  update(t) {
    this._$Eq && (this._$Eq = this._$Eq.forEach((e) => this._$ET(e, this[e]))), this._$EM();
  }
  updated(t) {
  }
  firstUpdated(t) {
  }
};
k.elementStyles = [], k.shadowRootOptions = { mode: "open" }, k[H("elementProperties")] = /* @__PURE__ */ new Map(), k[H("finalized")] = /* @__PURE__ */ new Map(), tt == null || tt({ ReactiveElement: k }), ($.reactiveElementVersions ?? ($.reactiveElementVersions = [])).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const R = globalThis, ft = (r) => r, G = R.trustedTypes, gt = G ? G.createPolicy("lit-html", { createHTML: (r) => r }) : void 0, xt = "$lit$", y = `lit$${Math.random().toFixed(9).slice(2)}$`, Et = "?" + y, Lt = `<${Et}>`, x = document, j = () => x.createComment(""), z = (r) => r === null || typeof r != "object" && typeof r != "function", nt = Array.isArray, Vt = (r) => nt(r) || typeof (r == null ? void 0 : r[Symbol.iterator]) == "function", et = `[ 	
\f\r]`, N = /<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g, mt = /-->/g, vt = />/g, S = RegExp(`>|${et}(?:([^\\s"'>=/]+)(${et}*=${et}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`, "g"), yt = /'/g, $t = /"/g, Pt = /^(?:script|style|textarea|title)$/i, Bt = (r) => (t, ...e) => ({ _$litType$: r, strings: t, values: e }), c = Bt(1), O = Symbol.for("lit-noChange"), h = Symbol.for("lit-nothing"), _t = /* @__PURE__ */ new WeakMap(), w = x.createTreeWalker(x, 129);
function Mt(r, t) {
  if (!nt(r) || !r.hasOwnProperty("raw")) throw Error("invalid template strings array");
  return gt !== void 0 ? gt.createHTML(t) : t;
}
const Ft = (r, t) => {
  const e = r.length - 1, i = [];
  let s, o = t === 2 ? "<svg>" : t === 3 ? "<math>" : "", a = N;
  for (let l = 0; l < e; l++) {
    const n = r[l];
    let d, p, u = -1, m = 0;
    for (; m < n.length && (a.lastIndex = m, p = a.exec(n), p !== null); ) m = a.lastIndex, a === N ? p[1] === "!--" ? a = mt : p[1] !== void 0 ? a = vt : p[2] !== void 0 ? (Pt.test(p[2]) && (s = RegExp("</" + p[2], "g")), a = S) : p[3] !== void 0 && (a = S) : a === S ? p[0] === ">" ? (a = s ?? N, u = -1) : p[1] === void 0 ? u = -2 : (u = a.lastIndex - p[2].length, d = p[1], a = p[3] === void 0 ? S : p[3] === '"' ? $t : yt) : a === $t || a === yt ? a = S : a === mt || a === vt ? a = N : (a = S, s = void 0);
    const v = a === S && r[l + 1].startsWith("/>") ? " " : "";
    o += a === N ? n + Lt : u >= 0 ? (i.push(d), n.slice(0, u) + xt + n.slice(u) + y + v) : n + y + (u === -2 ? l : v);
  }
  return [Mt(r, o + (r[e] || "<?>") + (t === 2 ? "</svg>" : t === 3 ? "</math>" : "")), i];
};
class I {
  constructor({ strings: t, _$litType$: e }, i) {
    let s;
    this.parts = [];
    let o = 0, a = 0;
    const l = t.length - 1, n = this.parts, [d, p] = Ft(t, e);
    if (this.el = I.createElement(d, i), w.currentNode = this.el.content, e === 2 || e === 3) {
      const u = this.el.content.firstChild;
      u.replaceWith(...u.childNodes);
    }
    for (; (s = w.nextNode()) !== null && n.length < l; ) {
      if (s.nodeType === 1) {
        if (s.hasAttributes()) for (const u of s.getAttributeNames()) if (u.endsWith(xt)) {
          const m = p[a++], v = s.getAttribute(u).split(y), W = /([.?@])?(.*)/.exec(m);
          n.push({ type: 1, index: o, name: W[2], strings: v, ctor: W[1] === "." ? qt : W[1] === "?" ? Jt : W[1] === "@" ? Gt : X }), s.removeAttribute(u);
        } else u.startsWith(y) && (n.push({ type: 6, index: o }), s.removeAttribute(u));
        if (Pt.test(s.tagName)) {
          const u = s.textContent.split(y), m = u.length - 1;
          if (m > 0) {
            s.textContent = G ? G.emptyScript : "";
            for (let v = 0; v < m; v++) s.append(u[v], j()), w.nextNode(), n.push({ type: 2, index: ++o });
            s.append(u[m], j());
          }
        }
      } else if (s.nodeType === 8) if (s.data === Et) n.push({ type: 2, index: o });
      else {
        let u = -1;
        for (; (u = s.data.indexOf(y, u + 1)) !== -1; ) n.push({ type: 7, index: o }), u += y.length - 1;
      }
      o++;
    }
  }
  static createElement(t, e) {
    const i = x.createElement("template");
    return i.innerHTML = t, i;
  }
}
function D(r, t, e = r, i) {
  var a, l;
  if (t === O) return t;
  let s = i !== void 0 ? (a = e._$Co) == null ? void 0 : a[i] : e._$Cl;
  const o = z(t) ? void 0 : t._$litDirective$;
  return (s == null ? void 0 : s.constructor) !== o && ((l = s == null ? void 0 : s._$AO) == null || l.call(s, !1), o === void 0 ? s = void 0 : (s = new o(r), s._$AT(r, e, i)), i !== void 0 ? (e._$Co ?? (e._$Co = []))[i] = s : e._$Cl = s), s !== void 0 && (t = D(r, s._$AS(r, t.values), s, i)), t;
}
class Wt {
  constructor(t, e) {
    this._$AV = [], this._$AN = void 0, this._$AD = t, this._$AM = e;
  }
  get parentNode() {
    return this._$AM.parentNode;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  u(t) {
    const { el: { content: e }, parts: i } = this._$AD, s = ((t == null ? void 0 : t.creationScope) ?? x).importNode(e, !0);
    w.currentNode = s;
    let o = w.nextNode(), a = 0, l = 0, n = i[0];
    for (; n !== void 0; ) {
      if (a === n.index) {
        let d;
        n.type === 2 ? d = new B(o, o.nextSibling, this, t) : n.type === 1 ? d = new n.ctor(o, n.name, n.strings, this, t) : n.type === 6 && (d = new Kt(o, this, t)), this._$AV.push(d), n = i[++l];
      }
      a !== (n == null ? void 0 : n.index) && (o = w.nextNode(), a++);
    }
    return w.currentNode = x, s;
  }
  p(t) {
    let e = 0;
    for (const i of this._$AV) i !== void 0 && (i.strings !== void 0 ? (i._$AI(t, i, e), e += i.strings.length - 2) : i._$AI(t[e])), e++;
  }
}
class B {
  get _$AU() {
    var t;
    return ((t = this._$AM) == null ? void 0 : t._$AU) ?? this._$Cv;
  }
  constructor(t, e, i, s) {
    this.type = 2, this._$AH = h, this._$AN = void 0, this._$AA = t, this._$AB = e, this._$AM = i, this.options = s, this._$Cv = (s == null ? void 0 : s.isConnected) ?? !0;
  }
  get parentNode() {
    let t = this._$AA.parentNode;
    const e = this._$AM;
    return e !== void 0 && (t == null ? void 0 : t.nodeType) === 11 && (t = e.parentNode), t;
  }
  get startNode() {
    return this._$AA;
  }
  get endNode() {
    return this._$AB;
  }
  _$AI(t, e = this) {
    t = D(this, t, e), z(t) ? t === h || t == null || t === "" ? (this._$AH !== h && this._$AR(), this._$AH = h) : t !== this._$AH && t !== O && this._(t) : t._$litType$ !== void 0 ? this.$(t) : t.nodeType !== void 0 ? this.T(t) : Vt(t) ? this.k(t) : this._(t);
  }
  O(t) {
    return this._$AA.parentNode.insertBefore(t, this._$AB);
  }
  T(t) {
    this._$AH !== t && (this._$AR(), this._$AH = this.O(t));
  }
  _(t) {
    this._$AH !== h && z(this._$AH) ? this._$AA.nextSibling.data = t : this.T(x.createTextNode(t)), this._$AH = t;
  }
  $(t) {
    var o;
    const { values: e, _$litType$: i } = t, s = typeof i == "number" ? this._$AC(t) : (i.el === void 0 && (i.el = I.createElement(Mt(i.h, i.h[0]), this.options)), i);
    if (((o = this._$AH) == null ? void 0 : o._$AD) === s) this._$AH.p(e);
    else {
      const a = new Wt(s, this), l = a.u(this.options);
      a.p(e), this.T(l), this._$AH = a;
    }
  }
  _$AC(t) {
    let e = _t.get(t.strings);
    return e === void 0 && _t.set(t.strings, e = new I(t)), e;
  }
  k(t) {
    nt(this._$AH) || (this._$AH = [], this._$AR());
    const e = this._$AH;
    let i, s = 0;
    for (const o of t) s === e.length ? e.push(i = new B(this.O(j()), this.O(j()), this, this.options)) : i = e[s], i._$AI(o), s++;
    s < e.length && (this._$AR(i && i._$AB.nextSibling, s), e.length = s);
  }
  _$AR(t = this._$AA.nextSibling, e) {
    var i;
    for ((i = this._$AP) == null ? void 0 : i.call(this, !1, !0, e); t !== this._$AB; ) {
      const s = ft(t).nextSibling;
      ft(t).remove(), t = s;
    }
  }
  setConnected(t) {
    var e;
    this._$AM === void 0 && (this._$Cv = t, (e = this._$AP) == null || e.call(this, t));
  }
}
class X {
  get tagName() {
    return this.element.tagName;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  constructor(t, e, i, s, o) {
    this.type = 1, this._$AH = h, this._$AN = void 0, this.element = t, this.name = e, this._$AM = s, this.options = o, i.length > 2 || i[0] !== "" || i[1] !== "" ? (this._$AH = Array(i.length - 1).fill(new String()), this.strings = i) : this._$AH = h;
  }
  _$AI(t, e = this, i, s) {
    const o = this.strings;
    let a = !1;
    if (o === void 0) t = D(this, t, e, 0), a = !z(t) || t !== this._$AH && t !== O, a && (this._$AH = t);
    else {
      const l = t;
      let n, d;
      for (t = o[0], n = 0; n < o.length - 1; n++) d = D(this, l[i + n], e, n), d === O && (d = this._$AH[n]), a || (a = !z(d) || d !== this._$AH[n]), d === h ? t = h : t !== h && (t += (d ?? "") + o[n + 1]), this._$AH[n] = d;
    }
    a && !s && this.j(t);
  }
  j(t) {
    t === h ? this.element.removeAttribute(this.name) : this.element.setAttribute(this.name, t ?? "");
  }
}
class qt extends X {
  constructor() {
    super(...arguments), this.type = 3;
  }
  j(t) {
    this.element[this.name] = t === h ? void 0 : t;
  }
}
class Jt extends X {
  constructor() {
    super(...arguments), this.type = 4;
  }
  j(t) {
    this.element.toggleAttribute(this.name, !!t && t !== h);
  }
}
class Gt extends X {
  constructor(t, e, i, s, o) {
    super(t, e, i, s, o), this.type = 5;
  }
  _$AI(t, e = this) {
    if ((t = D(this, t, e, 0) ?? h) === O) return;
    const i = this._$AH, s = t === h && i !== h || t.capture !== i.capture || t.once !== i.once || t.passive !== i.passive, o = t !== h && (i === h || s);
    s && this.element.removeEventListener(this.name, this, i), o && this.element.addEventListener(this.name, this, t), this._$AH = t;
  }
  handleEvent(t) {
    var e;
    typeof this._$AH == "function" ? this._$AH.call(((e = this.options) == null ? void 0 : e.host) ?? this.element, t) : this._$AH.handleEvent(t);
  }
}
class Kt {
  constructor(t, e, i) {
    this.element = t, this.type = 6, this._$AN = void 0, this._$AM = e, this.options = i;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  _$AI(t) {
    D(this, t);
  }
}
const st = R.litHtmlPolyfillSupport;
st == null || st(I, B), (R.litHtmlVersions ?? (R.litHtmlVersions = [])).push("3.3.2");
const Zt = (r, t, e) => {
  const i = (e == null ? void 0 : e.renderBefore) ?? t;
  let s = i._$litPart$;
  if (s === void 0) {
    const o = (e == null ? void 0 : e.renderBefore) ?? null;
    i._$litPart$ = s = new B(t.insertBefore(j(), o), o, void 0, e ?? {});
  }
  return s._$AI(r), s;
};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const C = globalThis;
class g extends k {
  constructor() {
    super(...arguments), this.renderOptions = { host: this }, this._$Do = void 0;
  }
  createRenderRoot() {
    var e;
    const t = super.createRenderRoot();
    return (e = this.renderOptions).renderBefore ?? (e.renderBefore = t.firstChild), t;
  }
  update(t) {
    const e = this.render();
    this.hasUpdated || (this.renderOptions.isConnected = this.isConnected), super.update(t), this._$Do = Zt(e, this.renderRoot, this.renderOptions);
  }
  connectedCallback() {
    var t;
    super.connectedCallback(), (t = this._$Do) == null || t.setConnected(!0);
  }
  disconnectedCallback() {
    var t;
    super.disconnectedCallback(), (t = this._$Do) == null || t.setConnected(!1);
  }
  render() {
    return O;
  }
}
var At;
g._$litElement$ = !0, g.finalized = !0, (At = C.litElementHydrateSupport) == null || At.call(C, { LitElement: g });
const it = C.litElementPolyfillSupport;
it == null || it({ LitElement: g });
(C.litElementVersions ?? (C.litElementVersions = [])).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const A = (r) => (t, e) => {
  e !== void 0 ? e.addInitializer(() => {
    customElements.define(r, t);
  }) : customElements.define(r, t);
};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const Xt = { attribute: !0, type: String, converter: J, reflect: !1, hasChanged: at }, Qt = (r = Xt, t, e) => {
  const { kind: i, metadata: s } = e;
  let o = globalThis.litPropertyMetadata.get(s);
  if (o === void 0 && globalThis.litPropertyMetadata.set(s, o = /* @__PURE__ */ new Map()), i === "setter" && ((r = Object.create(r)).wrapped = !0), o.set(e.name, r), i === "accessor") {
    const { name: a } = e;
    return { set(l) {
      const n = t.get.call(this);
      t.set.call(this, l), this.requestUpdate(a, n, r, !0, l);
    }, init(l) {
      return l !== void 0 && this.C(a, void 0, r, l), l;
    } };
  }
  if (i === "setter") {
    const { name: a } = e;
    return function(l) {
      const n = this[a];
      t.call(this, l), this.requestUpdate(a, n, r, !0, l);
    };
  }
  throw Error("Unsupported decorator location: " + i);
};
function f(r) {
  return (t, e) => typeof e == "object" ? Qt(r, t, e) : ((i, s, o) => {
    const a = s.hasOwnProperty(o);
    return s.constructor.createProperty(o, i), a ? Object.getOwnPropertyDescriptor(s, o) : void 0;
  })(r, t, e);
}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
function M(r) {
  return f({ ...r, state: !0, attribute: !1 });
}
var Yt = Object.defineProperty, te = Object.getOwnPropertyDescriptor, Q = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? te(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && Yt(t, e, s), s;
};
let T = class extends g {
  constructor() {
    super(...arguments), this.overallStatus = "NeverChecked", this.affectedAdvisoryCount = 0, this.mitigatedAdvisoryCount = 0;
  }
  render() {
    if (this.overallStatus === "Safe")
      return c`
        <div class="status-safe">
          <uui-icon name="check-circle" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">No Active Vulnerabilities</span>
        </div>
      `;
    if (this.overallStatus === "Mitigated")
      return c`
        <div class="status-mitigated">
          <uui-icon name="shield" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">
            ${this.mitigatedAdvisoryCount}
            ${this.mitigatedAdvisoryCount === 1 ? "Vulnerability" : "Vulnerabilities"} Mitigated
          </span>
        </div>
      `;
    if (this.overallStatus === "Vulnerable") {
      const r = this.mitigatedAdvisoryCount > 0 ? ` and ${this.mitigatedAdvisoryCount} Mitigated` : "";
      return c`
        <div class="status-vulnerable">
          <uui-icon name="alert" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">
            ${this.affectedAdvisoryCount} Active${r}
            ${this.affectedAdvisoryCount === 1 ? "Vulnerability" : "Vulnerabilities"} Found
          </span>
        </div>
      `;
    }
    return c`
      <div class="status-neutral">
        <uui-icon name="info" style="font-size: 2rem;"></uui-icon>
        <span class="status-label">Not yet checked</span>
      </div>
    `;
  }
};
T.styles = b`
    :host { display: block; }
    .status-safe { color: var(--uui-color-positive, #00a152); display: flex; align-items: center; gap: 8px; }
    .status-mitigated { color: var(--uui-color-warning, #f5a623); display: flex; align-items: center; gap: 8px; }
    .status-vulnerable { color: var(--uui-color-danger, #d0011b); display: flex; align-items: center; gap: 8px; }
    .status-neutral { color: var(--uui-color-text, #333); display: flex; align-items: center; gap: 8px; }
    .status-label { font-size: 1.2rem; font-weight: 600; }
  `;
Q([
  f({ type: String })
], T.prototype, "overallStatus", 2);
Q([
  f({ type: Number })
], T.prototype, "affectedAdvisoryCount", 2);
Q([
  f({ type: Number })
], T.prototype, "mitigatedAdvisoryCount", 2);
T = Q([
  A("security-dashboard-status-indicator")
], T);
var ee = Object.defineProperty, se = Object.getOwnPropertyDescriptor, F = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? se(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && ee(t, e, s), s;
};
let E = class extends g {
  constructor() {
    super(...arguments), this.isStale = !1, this.lastSuccessfulCheckAt = null, this.lastCheckSucceeded = null, this.lastCheckError = null;
  }
  render() {
    const r = this.isStale, t = this.lastCheckSucceeded === !1;
    return !r && !t ? c`` : c`
      ${r ? c`
        <div class="stale-warning">
          <strong>Data may be outdated</strong> — the last successful check was more than 48 hours ago.
        </div>
      ` : ""}
      ${t ? c`
        <div class="failure-notice">
          <strong>Last check failed</strong>${this.lastCheckError ? `: ${this.lastCheckError}` : "."}
        </div>
      ` : ""}
    `;
  }
};
E.styles = b`
    :host { display: block; }
    .stale-warning {
      margin-top: 8px;
      padding: 8px 12px;
      background: var(--uui-color-warning-surface, #fff3cd);
      border: 1px solid var(--uui-color-warning, #ffc107);
      border-radius: 4px;
      color: var(--uui-color-warning-contrast, #856404);
      font-size: 0.875rem;
    }
    .failure-notice {
      margin-top: 8px;
      padding: 8px 12px;
      background: var(--uui-color-danger-surface, #fde8e8);
      border: 1px solid var(--uui-color-danger, #d0011b);
      border-radius: 4px;
      color: var(--uui-color-danger-contrast, #7b0018);
      font-size: 0.875rem;
    }
  `;
F([
  f({ type: Boolean })
], E.prototype, "isStale", 2);
F([
  f({ type: String })
], E.prototype, "lastSuccessfulCheckAt", 2);
F([
  f({ type: Boolean })
], E.prototype, "lastCheckSucceeded", 2);
F([
  f({ type: String })
], E.prototype, "lastCheckError", 2);
E = F([
  A("security-dashboard-staleness-warning")
], E);
var ie = Object.defineProperty, re = Object.getOwnPropertyDescriptor, lt = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? re(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && ie(t, e, s), s;
};
let L = class extends g {
  constructor() {
    super(...arguments), this.lastSuccessfulCheckAt = null, this.nextScheduledCheckAt = "";
  }
  formatTimestamp(r) {
    if (!r) return "Not yet run";
    const t = new Date(r), i = (/* @__PURE__ */ new Date()).getTime() - t.getTime(), s = Math.floor(i / 6e4), o = Math.floor(s / 60), a = Math.floor(o / 24);
    return s < 1 ? "Just now" : s < 60 ? `${s} minute${s === 1 ? "" : "s"} ago` : o < 24 ? `${o} hour${o === 1 ? "" : "s"} ago` : a < 7 ? `${a} day${a === 1 ? "" : "s"} ago` : new Intl.DateTimeFormat(void 0, {
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit"
    }).format(t);
  }
  formatFutureTimestamp(r) {
    const t = new Date(r), e = /* @__PURE__ */ new Date(), i = t.getTime() - e.getTime(), s = Math.ceil(i / 6e4), o = Math.ceil(s / 60);
    return s < 60 ? `In ${s} minute${s === 1 ? "" : "s"}` : o < 24 ? `In ${o} hour${o === 1 ? "" : "s"}` : new Intl.DateTimeFormat(void 0, {
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit"
    }).format(t);
  }
  render() {
    return c`
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
};
L.styles = b`
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
lt([
  f({ type: String })
], L.prototype, "lastSuccessfulCheckAt", 2);
lt([
  f({ type: String })
], L.prototype, "nextScheduledCheckAt", 2);
L = lt([
  A("security-dashboard-check-schedule")
], L);
var oe = Object.defineProperty, ae = Object.getOwnPropertyDescriptor, kt = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? ae(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && oe(t, e, s), s;
};
const bt = {
  Safe: "/App_Plugins/SecurityDashboard/images/status-safe.png",
  Mitigated: "/App_Plugins/SecurityDashboard/images/status-mitigated.png",
  Vulnerable: "/App_Plugins/SecurityDashboard/images/status-vulnerable.png",
  NeverChecked: "/App_Plugins/SecurityDashboard/images/status-never-checked.png"
};
let K = class extends g {
  render() {
    const r = this.status, t = bt[r.overallStatus] ?? bt.NeverChecked;
    return c`
      <img class="status-image" src=${t} alt=${r.overallStatus} />
      <div class="content">
        <security-dashboard-status-indicator
          .overallStatus=${r.overallStatus}
          .affectedAdvisoryCount=${r.affectedAdvisoryCount}
          .mitigatedAdvisoryCount=${r.mitigatedAdvisoryCount}>
        </security-dashboard-status-indicator>

        <security-dashboard-staleness-warning
          .isStale=${r.isStale}
          .lastSuccessfulCheckAt=${r.lastSuccessfulCheckAt}
          .lastCheckSucceeded=${r.lastCheckSucceeded}
          .lastCheckError=${r.lastCheckError}>
        </security-dashboard-staleness-warning>

        <security-dashboard-check-schedule
          .lastSuccessfulCheckAt=${r.lastSuccessfulCheckAt}
          .nextScheduledCheckAt=${r.nextScheduledCheckAt}>
        </security-dashboard-check-schedule>
      </div>
    `;
  }
};
K.styles = b`
    :host { display: flex; align-items: flex-start; gap: 16px; }
    .status-image { width: 100px; height: 100px; flex-shrink: 0; }
    .content { flex: 1; }
  `;
kt([
  f({ attribute: !1 })
], K.prototype, "status", 2);
K = kt([
  A("security-dashboard-header")
], K);
var ne = Object.defineProperty, le = Object.getOwnPropertyDescriptor, U = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? le(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && ne(t, e, s), s;
};
let _ = class extends St(g) {
  constructor() {
    super(...arguments), this.mode = "mark", this.ghsaId = "", this._description = "", this._submitting = !1, this._error = null;
  }
  async _getToken() {
    return (await this.getContext(wt)).getLatestToken();
  }
  async _handleMark() {
    if (this._description.trim()) {
      this._submitting = !0, this._error = null;
      try {
        const r = await this._getToken(), t = await fetch(
          `/umbraco/management/api/v1/security-dashboard/advisories/${encodeURIComponent(this.ghsaId)}/mitigations`,
          {
            method: "POST",
            headers: {
              Authorization: `Bearer ${r}`,
              "Content-Type": "application/json"
            },
            body: JSON.stringify({ description: this._description })
          }
        );
        if (!t.ok) {
          t.status === 409 ? this._error = "This advisory is already marked as mitigated." : this._error = `Unexpected error (${t.status}). Please try again.`;
          return;
        }
        this.dispatchEvent(new CustomEvent("mitigation-changed", { bubbles: !0, composed: !0 }));
      } catch {
        this._error = "Failed to save mitigation. Please try again.";
      } finally {
        this._submitting = !1;
      }
    }
  }
  async _handleRemove() {
    this._submitting = !0, this._error = null;
    try {
      const r = await this._getToken(), t = await fetch(
        `/umbraco/management/api/v1/security-dashboard/advisories/${encodeURIComponent(this.ghsaId)}/mitigations`,
        {
          method: "DELETE",
          headers: { Authorization: `Bearer ${r}` }
        }
      );
      if (!t.ok) {
        this._error = `Unexpected error (${t.status}). Please try again.`;
        return;
      }
      this.dispatchEvent(new CustomEvent("mitigation-changed", { bubbles: !0, composed: !0 }));
    } catch {
      this._error = "Failed to remove mitigation. Please try again.";
    } finally {
      this._submitting = !1;
    }
  }
  _handleCancel() {
    this.dispatchEvent(new CustomEvent("mitigation-cancelled", { bubbles: !0, composed: !0 }));
  }
  render() {
    return this.mode === "remove" ? c`
        <uui-dialog>
          <uui-dialog-layout headline="Remove Mitigation">
            <p>Are you sure you want to remove this manual mitigation? The advisory will revert to its automatically calculated status.</p>
            ${this._error ? c`<div class="dialog-error">${this._error}</div>` : ""}
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
              >${this._submitting ? "Removing..." : "Remove Mitigation"}</uui-button>
            </div>
          </uui-dialog-layout>
        </uui-dialog>
      ` : c`
      <uui-dialog>
        <uui-dialog-layout headline="Mark As Mitigated">
          <p>Describe how this vulnerability has been mitigated (e.g. compensating controls, configuration changes, WAF rules).</p>
          <uui-textarea
            label="Description"
            name="description"
            placeholder="Describe the mitigation..."
            .value=${this._description}
            @input=${(r) => {
      this._description = r.target.value;
    }}
          ></uui-textarea>
          ${this._error ? c`<div class="dialog-error">${this._error}</div>` : ""}
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
            >${this._submitting ? "Saving..." : "Mark As Mitigated"}</uui-button>
          </div>
        </uui-dialog-layout>
      </uui-dialog>
    `;
  }
};
_.styles = b`
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
U([
  f()
], _.prototype, "mode", 2);
U([
  f()
], _.prototype, "ghsaId", 2);
U([
  M()
], _.prototype, "_description", 2);
U([
  M()
], _.prototype, "_submitting", 2);
U([
  M()
], _.prototype, "_error", 2);
_ = U([
  A("security-dashboard-mitigation-dialog")
], _);
var ce = Object.defineProperty, ue = Object.getOwnPropertyDescriptor, ct = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? ue(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && ce(t, e, s), s;
};
let V = class extends g {
  constructor() {
    super(...arguments), this._showMarkDialog = !1;
  }
  getSeverityColor(r) {
    switch (r) {
      case "Critical":
        return "danger";
      case "High":
        return "warning";
      case "Moderate":
        return "warning";
      case "Low":
        return "default";
      default:
        return "default";
    }
  }
  getStatusColor(r) {
    switch (r) {
      case "Vulnerable":
        return "danger";
      case "Mitigated":
        return "danger";
      case "Unknown":
        return "warning";
      case "NotAffected":
        return "positive";
      default:
        return "default";
    }
  }
  _onMitigationChanged() {
    this._showMarkDialog = !1;
  }
  render() {
    if (!this.advisory) return c``;
    var r = "";
    this.advisory.packages.forEach((i) => {
      r.includes(i.packageName + " — ") ? r += `${i.affectedVersionRange}, ` : r += `${i.packageName} — ${i.affectedVersionRange}, `;
    });
    const t = (this.advisory.affectedStatus === "Vulnerable" || this.advisory.affectedStatus === "Unknown") && !this.advisory.manualMitigation, e = this.advisory.manualMitigation ? new Date(this.advisory.manualMitigation.mitigatedAt).toLocaleDateString() : "";
    return c`
      <div class="advisory-item">
        <div class="advisory-row">
          <div>
            <div class="advisory-title">${this.advisory.title}</div>
            <div class="advisory-package">
              <uui-tag color="${this.getSeverityColor(this.advisory.severity)}">
                ${this.advisory.severity}
              </uui-tag>
              ${r.slice(0, -2)}
            </div>
            ${this.advisory.manualMitigation ? c`
              <div class="mitigation-attribution">
                <span class="attribution-who">
                  Manually mitigated by ${this.advisory.manualMitigation.mitigatedBy} on ${e}
                </span>
                <div class="attribution-description">${this.advisory.manualMitigation.description}</div>
              </div>
            ` : ""}
          </div>
          <div class="badges">
            <uui-tag
              color="${this.getStatusColor(this.advisory.affectedStatus)}"
              look="${this.advisory.affectedStatus === "Mitigated" ? "outline" : "primary"}"
            >
              ${this.advisory.affectedStatus === "NotAffected" ? "Not Affected" : this.advisory.affectedStatus}
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
          ${t ? c`
            <uui-button
              look="primary"
              color="positive"
              @click=${() => {
      this._showMarkDialog = !0;
    }}>
              Mark As Mitigated
            </uui-button>
          ` : c`<span></span>`}
        </div>
        ${this._showMarkDialog ? c`
          <security-dashboard-mitigation-dialog
            mode="mark"
            ghsaId=${this.advisory.ghsaId}
            @mitigation-changed=${this._onMitigationChanged}
            @mitigation-cancelled=${() => {
      this._showMarkDialog = !1;
    }}
          ></security-dashboard-mitigation-dialog>
        ` : ""}
      </div>
    `;
  }
};
V.styles = b`
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
ct([
  f({ type: Object })
], V.prototype, "advisory", 2);
ct([
  M()
], V.prototype, "_showMarkDialog", 2);
V = ct([
  A("security-dashboard-advisory-item")
], V);
var de = Object.defineProperty, he = Object.getOwnPropertyDescriptor, Ot = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? he(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && de(t, e, s), s;
};
let Z = class extends g {
  constructor() {
    super(...arguments), this.advisories = [];
  }
  render() {
    const r = this.advisories.filter(
      (e) => e.affectedStatus === "Vulnerable" || e.affectedStatus === "Mitigated"
    ), t = this.advisories.filter(
      (e) => e.affectedStatus === "Unknown" || e.affectedStatus === "NotAffected"
    );
    return c`
      <h3>Active Vulnerabilities</h3>
      ${r.length === 0 ? c`<div class="empty-state">No active vulnerabilities found.</div>` : r.map((e) => c`
            <security-dashboard-advisory-item .advisory=${e}></security-dashboard-advisory-item>
          `)}

      ${t.length > 0 ? c`
        <h3>Known Advisories</h3>
        ${t.map((e) => c`
          <security-dashboard-advisory-item .advisory=${e}></security-dashboard-advisory-item>
        `)}
      ` : ""}
    `;
  }
};
Z.styles = b`
    :host { display: block; }
    h3 { margin: 16px 0 8px; font-size: 1rem; font-weight: 600; }
    .empty-state {
      padding: 16px;
      text-align: center;
      color: var(--uui-color-text-alt, #666);
      font-style: italic;
    }
  `;
Ot([
  f({ type: Array })
], Z.prototype, "advisories", 2);
Z = Ot([
  A("security-dashboard-advisory-list")
], Z);
var pe = Object.defineProperty, fe = Object.getOwnPropertyDescriptor, Y = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? fe(t, e) : t, o = r.length - 1, a; o >= 0; o--)
    (a = r[o]) && (s = (i ? a(t, e, s) : a(s)) || s);
  return i && s && pe(t, e, s), s;
};
let P = class extends St(g) {
  constructor() {
    super(...arguments), this._status = null, this._loading = !0, this._error = null, this._onMitigationChanged = () => {
      this._fetchStatus();
    };
  }
  connectedCallback() {
    super.connectedCallback(), this._fetchStatus(), this.addEventListener("mitigation-changed", this._onMitigationChanged);
  }
  disconnectedCallback() {
    super.disconnectedCallback(), this.removeEventListener("mitigation-changed", this._onMitigationChanged);
  }
  async _fetchStatus() {
    this._loading = !0, this._error = null;
    try {
      const r = await this.getContext(wt);
      if (!r) {
        this._error = "Authentication context not available.";
        return;
      }
      const t = await r.getLatestToken(), e = await fetch("/umbraco/management/api/v1/security-dashboard/status", {
        headers: {
          Authorization: `Bearer ${t}`,
          "Content-Type": "application/json"
        }
      });
      if (e.status === 401) {
        this._error = "Unauthorized. Please log in to the Umbraco backoffice.";
        return;
      }
      if (!e.ok) {
        this._error = `Server error (${e.status}). Please try again later.`;
        return;
      }
      this._status = await e.json();
    } catch (r) {
      this._error = r instanceof Error ? r.message : "Failed to load vulnerability status.";
    } finally {
      this._loading = !1;
    }
  }
  render() {
    if (this._loading)
      return c`
        <uui-box>
          <h2>Security Health</h2>
          <uui-loader></uui-loader>
        </uui-box>
      `;
    if (this._error)
      return c`
        <uui-box>
          <h2>Security Health</h2>
          <div class="error-box">${this._error}</div>
        </uui-box>
      `;
    const r = this._status;
    return c`
      <uui-box>
        <h2>Security Health</h2>

        <security-dashboard-header
          .status=${r}>
        </security-dashboard-header>

        ${r.advisories.length > 0 ? c`
          <security-dashboard-advisory-list
            .advisories=${r.advisories}>
          </security-dashboard-advisory-list>
        ` : ""}
      </uui-box>
    `;
  }
};
P.styles = b`
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
Y([
  M()
], P.prototype, "_status", 2);
Y([
  M()
], P.prototype, "_loading", 2);
Y([
  M()
], P.prototype, "_error", 2);
P = Y([
  A("security-dashboard")
], P);
const $e = P;
export {
  P as SecurityDashboardElement,
  $e as default
};
