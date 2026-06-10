import { UmbElementMixin as lt } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT as ct } from "@umbraco-cms/backoffice/auth";
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const Z = globalThis, ut = Z.ShadowRoot && (Z.ShadyCSS === void 0 || Z.ShadyCSS.nativeShadow) && "adoptedStyleSheets" in Document.prototype && "replace" in CSSStyleSheet.prototype, dt = Symbol(), ft = /* @__PURE__ */ new WeakMap();
let Pt = class {
  constructor(t, e, s) {
    if (this._$cssResult$ = !0, s !== dt) throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");
    this.cssText = t, this.t = e;
  }
  get styleSheet() {
    let t = this.o;
    const e = this.t;
    if (ut && t === void 0) {
      const s = e !== void 0 && e.length === 1;
      s && (t = ft.get(e)), t === void 0 && ((this.o = t = new CSSStyleSheet()).replaceSync(this.cssText), s && ft.set(e, t));
    }
    return t;
  }
  toString() {
    return this.cssText;
  }
};
const Nt = (r) => new Pt(typeof r == "string" ? r : r + "", void 0, dt), y = (r, ...t) => {
  const e = r.length === 1 ? r[0] : t.reduce((s, i, a) => s + ((o) => {
    if (o._$cssResult$ === !0) return o.cssText;
    if (typeof o == "number") return o;
    throw Error("Value passed to 'css' function must be a 'css' function result: " + o + ". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.");
  })(i) + r[a + 1], r[0]);
  return new Pt(e, r, dt);
}, Rt = (r, t) => {
  if (ut) r.adoptedStyleSheets = t.map((e) => e instanceof CSSStyleSheet ? e : e.styleSheet);
  else for (const e of t) {
    const s = document.createElement("style"), i = Z.litNonce;
    i !== void 0 && s.setAttribute("nonce", i), s.textContent = e.cssText, r.appendChild(s);
  }
}, mt = ut ? (r) => r : (r) => r instanceof CSSStyleSheet ? ((t) => {
  let e = "";
  for (const s of t.cssRules) e += s.cssText;
  return Nt(e);
})(r) : r;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const { is: Ht, defineProperty: jt, getOwnPropertyDescriptor: zt, getOwnPropertyNames: It, getOwnPropertySymbols: Lt, getPrototypeOf: Vt } = Object, A = globalThis, vt = A.trustedTypes, Bt = vt ? vt.emptyScript : "", rt = A.reactiveElementPolyfillSupport, L = (r, t) => r, K = { toAttribute(r, t) {
  switch (t) {
    case Boolean:
      r = r ? Bt : null;
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
} }, ht = (r, t) => !Ht(r, t), yt = { attribute: !0, type: String, converter: K, reflect: !1, useDefault: !1, hasChanged: ht };
Symbol.metadata ?? (Symbol.metadata = Symbol("metadata")), A.litPropertyMetadata ?? (A.litPropertyMetadata = /* @__PURE__ */ new WeakMap());
let D = class extends HTMLElement {
  static addInitializer(t) {
    this._$Ei(), (this.l ?? (this.l = [])).push(t);
  }
  static get observedAttributes() {
    return this.finalize(), this._$Eh && [...this._$Eh.keys()];
  }
  static createProperty(t, e = yt) {
    if (e.state && (e.attribute = !1), this._$Ei(), this.prototype.hasOwnProperty(t) && ((e = Object.create(e)).wrapped = !0), this.elementProperties.set(t, e), !e.noAccessor) {
      const s = Symbol(), i = this.getPropertyDescriptor(t, s, e);
      i !== void 0 && jt(this.prototype, t, i);
    }
  }
  static getPropertyDescriptor(t, e, s) {
    const { get: i, set: a } = zt(this.prototype, t) ?? { get() {
      return this[e];
    }, set(o) {
      this[e] = o;
    } };
    return { get: i, set(o) {
      const c = i == null ? void 0 : i.call(this);
      a == null || a.call(this, o), this.requestUpdate(t, c, s);
    }, configurable: !0, enumerable: !0 };
  }
  static getPropertyOptions(t) {
    return this.elementProperties.get(t) ?? yt;
  }
  static _$Ei() {
    if (this.hasOwnProperty(L("elementProperties"))) return;
    const t = Vt(this);
    t.finalize(), t.l !== void 0 && (this.l = [...t.l]), this.elementProperties = new Map(t.elementProperties);
  }
  static finalize() {
    if (this.hasOwnProperty(L("finalized"))) return;
    if (this.finalized = !0, this._$Ei(), this.hasOwnProperty(L("properties"))) {
      const e = this.properties, s = [...It(e), ...Lt(e)];
      for (const i of s) this.createProperty(i, e[i]);
    }
    const t = this[Symbol.metadata];
    if (t !== null) {
      const e = litPropertyMetadata.get(t);
      if (e !== void 0) for (const [s, i] of e) this.elementProperties.set(s, i);
    }
    this._$Eh = /* @__PURE__ */ new Map();
    for (const [e, s] of this.elementProperties) {
      const i = this._$Eu(e, s);
      i !== void 0 && this._$Eh.set(i, e);
    }
    this.elementStyles = this.finalizeStyles(this.styles);
  }
  static finalizeStyles(t) {
    const e = [];
    if (Array.isArray(t)) {
      const s = new Set(t.flat(1 / 0).reverse());
      for (const i of s) e.unshift(mt(i));
    } else t !== void 0 && e.push(mt(t));
    return e;
  }
  static _$Eu(t, e) {
    const s = e.attribute;
    return s === !1 ? void 0 : typeof s == "string" ? s : typeof t == "string" ? t.toLowerCase() : void 0;
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
    for (const s of e.keys()) this.hasOwnProperty(s) && (t.set(s, this[s]), delete this[s]);
    t.size > 0 && (this._$Ep = t);
  }
  createRenderRoot() {
    const t = this.shadowRoot ?? this.attachShadow(this.constructor.shadowRootOptions);
    return Rt(t, this.constructor.elementStyles), t;
  }
  connectedCallback() {
    var t;
    this.renderRoot ?? (this.renderRoot = this.createRenderRoot()), this.enableUpdating(!0), (t = this._$EO) == null || t.forEach((e) => {
      var s;
      return (s = e.hostConnected) == null ? void 0 : s.call(e);
    });
  }
  enableUpdating(t) {
  }
  disconnectedCallback() {
    var t;
    (t = this._$EO) == null || t.forEach((e) => {
      var s;
      return (s = e.hostDisconnected) == null ? void 0 : s.call(e);
    });
  }
  attributeChangedCallback(t, e, s) {
    this._$AK(t, s);
  }
  _$ET(t, e) {
    var a;
    const s = this.constructor.elementProperties.get(t), i = this.constructor._$Eu(t, s);
    if (i !== void 0 && s.reflect === !0) {
      const o = (((a = s.converter) == null ? void 0 : a.toAttribute) !== void 0 ? s.converter : K).toAttribute(e, s.type);
      this._$Em = t, o == null ? this.removeAttribute(i) : this.setAttribute(i, o), this._$Em = null;
    }
  }
  _$AK(t, e) {
    var a, o;
    const s = this.constructor, i = s._$Eh.get(t);
    if (i !== void 0 && this._$Em !== i) {
      const c = s.getPropertyOptions(i), n = typeof c.converter == "function" ? { fromAttribute: c.converter } : ((a = c.converter) == null ? void 0 : a.fromAttribute) !== void 0 ? c.converter : K;
      this._$Em = i;
      const d = n.fromAttribute(e, c.type);
      this[i] = d ?? ((o = this._$Ej) == null ? void 0 : o.get(i)) ?? d, this._$Em = null;
    }
  }
  requestUpdate(t, e, s, i = !1, a) {
    var o;
    if (t !== void 0) {
      const c = this.constructor;
      if (i === !1 && (a = this[t]), s ?? (s = c.getPropertyOptions(t)), !((s.hasChanged ?? ht)(a, e) || s.useDefault && s.reflect && a === ((o = this._$Ej) == null ? void 0 : o.get(t)) && !this.hasAttribute(c._$Eu(t, s)))) return;
      this.C(t, e, s);
    }
    this.isUpdatePending === !1 && (this._$ES = this._$EP());
  }
  C(t, e, { useDefault: s, reflect: i, wrapped: a }, o) {
    s && !(this._$Ej ?? (this._$Ej = /* @__PURE__ */ new Map())).has(t) && (this._$Ej.set(t, o ?? e ?? this[t]), a !== !0 || o !== void 0) || (this._$AL.has(t) || (this.hasUpdated || s || (e = void 0), this._$AL.set(t, e)), i === !0 && this._$Em !== t && (this._$Eq ?? (this._$Eq = /* @__PURE__ */ new Set())).add(t));
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
    var s;
    if (!this.isUpdatePending) return;
    if (!this.hasUpdated) {
      if (this.renderRoot ?? (this.renderRoot = this.createRenderRoot()), this._$Ep) {
        for (const [a, o] of this._$Ep) this[a] = o;
        this._$Ep = void 0;
      }
      const i = this.constructor.elementProperties;
      if (i.size > 0) for (const [a, o] of i) {
        const { wrapped: c } = o, n = this[a];
        c !== !0 || this._$AL.has(a) || n === void 0 || this.C(a, void 0, o, n);
      }
    }
    let t = !1;
    const e = this._$AL;
    try {
      t = this.shouldUpdate(e), t ? (this.willUpdate(e), (s = this._$EO) == null || s.forEach((i) => {
        var a;
        return (a = i.hostUpdate) == null ? void 0 : a.call(i);
      }), this.update(e)) : this._$EM();
    } catch (i) {
      throw t = !1, this._$EM(), i;
    }
    t && this._$AE(e);
  }
  willUpdate(t) {
  }
  _$AE(t) {
    var e;
    (e = this._$EO) == null || e.forEach((s) => {
      var i;
      return (i = s.hostUpdated) == null ? void 0 : i.call(s);
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
D.elementStyles = [], D.shadowRootOptions = { mode: "open" }, D[L("elementProperties")] = /* @__PURE__ */ new Map(), D[L("finalized")] = /* @__PURE__ */ new Map(), rt == null || rt({ ReactiveElement: D }), (A.reactiveElementVersions ?? (A.reactiveElementVersions = [])).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const V = globalThis, _t = (r) => r, X = V.trustedTypes, $t = X ? X.createPolicy("lit-html", { createHTML: (r) => r }) : void 0, Mt = "$lit$", b = `lit$${Math.random().toFixed(9).slice(2)}$`, kt = "?" + b, Ft = `<${kt}>`, M = document, B = () => M.createComment(""), F = (r) => r === null || typeof r != "object" && typeof r != "function", pt = Array.isArray, Wt = (r) => pt(r) || typeof (r == null ? void 0 : r[Symbol.iterator]) == "function", at = `[ 	
\f\r]`, z = /<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g, bt = /-->/g, At = />/g, C = RegExp(`>|${at}(?:([^\\s"'>=/]+)(${at}*=${at}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`, "g"), St = /'/g, wt = /"/g, Dt = /^(?:script|style|textarea|title)$/i, qt = (r) => (t, ...e) => ({ _$litType$: r, strings: t, values: e }), l = qt(1), O = Symbol.for("lit-noChange"), h = Symbol.for("lit-nothing"), xt = /* @__PURE__ */ new WeakMap(), E = M.createTreeWalker(M, 129);
function Ot(r, t) {
  if (!pt(r) || !r.hasOwnProperty("raw")) throw Error("invalid template strings array");
  return $t !== void 0 ? $t.createHTML(t) : t;
}
const Gt = (r, t) => {
  const e = r.length - 1, s = [];
  let i, a = t === 2 ? "<svg>" : t === 3 ? "<math>" : "", o = z;
  for (let c = 0; c < e; c++) {
    const n = r[c];
    let d, p, u = -1, v = 0;
    for (; v < n.length && (o.lastIndex = v, p = o.exec(n), p !== null); ) v = o.lastIndex, o === z ? p[1] === "!--" ? o = bt : p[1] !== void 0 ? o = At : p[2] !== void 0 ? (Dt.test(p[2]) && (i = RegExp("</" + p[2], "g")), o = C) : p[3] !== void 0 && (o = C) : o === C ? p[0] === ">" ? (o = i ?? z, u = -1) : p[1] === void 0 ? u = -2 : (u = o.lastIndex - p[2].length, d = p[1], o = p[3] === void 0 ? C : p[3] === '"' ? wt : St) : o === wt || o === St ? o = C : o === bt || o === At ? o = z : (o = C, i = void 0);
    const $ = o === C && r[c + 1].startsWith("/>") ? " " : "";
    a += o === z ? n + Ft : u >= 0 ? (s.push(d), n.slice(0, u) + Mt + n.slice(u) + b + $) : n + b + (u === -2 ? c : $);
  }
  return [Ot(r, a + (r[e] || "<?>") + (t === 2 ? "</svg>" : t === 3 ? "</math>" : "")), s];
};
class W {
  constructor({ strings: t, _$litType$: e }, s) {
    let i;
    this.parts = [];
    let a = 0, o = 0;
    const c = t.length - 1, n = this.parts, [d, p] = Gt(t, e);
    if (this.el = W.createElement(d, s), E.currentNode = this.el.content, e === 2 || e === 3) {
      const u = this.el.content.firstChild;
      u.replaceWith(...u.childNodes);
    }
    for (; (i = E.nextNode()) !== null && n.length < c; ) {
      if (i.nodeType === 1) {
        if (i.hasAttributes()) for (const u of i.getAttributeNames()) if (u.endsWith(Mt)) {
          const v = p[o++], $ = i.getAttribute(u).split(b), J = /([.?@])?(.*)/.exec(v);
          n.push({ type: 1, index: a, name: J[2], strings: $, ctor: J[1] === "." ? Zt : J[1] === "?" ? Kt : J[1] === "@" ? Xt : tt }), i.removeAttribute(u);
        } else u.startsWith(b) && (n.push({ type: 6, index: a }), i.removeAttribute(u));
        if (Dt.test(i.tagName)) {
          const u = i.textContent.split(b), v = u.length - 1;
          if (v > 0) {
            i.textContent = X ? X.emptyScript : "";
            for (let $ = 0; $ < v; $++) i.append(u[$], B()), E.nextNode(), n.push({ type: 2, index: ++a });
            i.append(u[v], B());
          }
        }
      } else if (i.nodeType === 8) if (i.data === kt) n.push({ type: 2, index: a });
      else {
        let u = -1;
        for (; (u = i.data.indexOf(b, u + 1)) !== -1; ) n.push({ type: 7, index: a }), u += b.length - 1;
      }
      a++;
    }
  }
  static createElement(t, e) {
    const s = M.createElement("template");
    return s.innerHTML = t, s;
  }
}
function T(r, t, e = r, s) {
  var o, c;
  if (t === O) return t;
  let i = s !== void 0 ? (o = e._$Co) == null ? void 0 : o[s] : e._$Cl;
  const a = F(t) ? void 0 : t._$litDirective$;
  return (i == null ? void 0 : i.constructor) !== a && ((c = i == null ? void 0 : i._$AO) == null || c.call(i, !1), a === void 0 ? i = void 0 : (i = new a(r), i._$AT(r, e, s)), s !== void 0 ? (e._$Co ?? (e._$Co = []))[s] = i : e._$Cl = i), i !== void 0 && (t = T(r, i._$AS(r, t.values), i, s)), t;
}
class Jt {
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
    const { el: { content: e }, parts: s } = this._$AD, i = ((t == null ? void 0 : t.creationScope) ?? M).importNode(e, !0);
    E.currentNode = i;
    let a = E.nextNode(), o = 0, c = 0, n = s[0];
    for (; n !== void 0; ) {
      if (o === n.index) {
        let d;
        n.type === 2 ? d = new G(a, a.nextSibling, this, t) : n.type === 1 ? d = new n.ctor(a, n.name, n.strings, this, t) : n.type === 6 && (d = new Qt(a, this, t)), this._$AV.push(d), n = s[++c];
      }
      o !== (n == null ? void 0 : n.index) && (a = E.nextNode(), o++);
    }
    return E.currentNode = M, i;
  }
  p(t) {
    let e = 0;
    for (const s of this._$AV) s !== void 0 && (s.strings !== void 0 ? (s._$AI(t, s, e), e += s.strings.length - 2) : s._$AI(t[e])), e++;
  }
}
class G {
  get _$AU() {
    var t;
    return ((t = this._$AM) == null ? void 0 : t._$AU) ?? this._$Cv;
  }
  constructor(t, e, s, i) {
    this.type = 2, this._$AH = h, this._$AN = void 0, this._$AA = t, this._$AB = e, this._$AM = s, this.options = i, this._$Cv = (i == null ? void 0 : i.isConnected) ?? !0;
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
    t = T(this, t, e), F(t) ? t === h || t == null || t === "" ? (this._$AH !== h && this._$AR(), this._$AH = h) : t !== this._$AH && t !== O && this._(t) : t._$litType$ !== void 0 ? this.$(t) : t.nodeType !== void 0 ? this.T(t) : Wt(t) ? this.k(t) : this._(t);
  }
  O(t) {
    return this._$AA.parentNode.insertBefore(t, this._$AB);
  }
  T(t) {
    this._$AH !== t && (this._$AR(), this._$AH = this.O(t));
  }
  _(t) {
    this._$AH !== h && F(this._$AH) ? this._$AA.nextSibling.data = t : this.T(M.createTextNode(t)), this._$AH = t;
  }
  $(t) {
    var a;
    const { values: e, _$litType$: s } = t, i = typeof s == "number" ? this._$AC(t) : (s.el === void 0 && (s.el = W.createElement(Ot(s.h, s.h[0]), this.options)), s);
    if (((a = this._$AH) == null ? void 0 : a._$AD) === i) this._$AH.p(e);
    else {
      const o = new Jt(i, this), c = o.u(this.options);
      o.p(e), this.T(c), this._$AH = o;
    }
  }
  _$AC(t) {
    let e = xt.get(t.strings);
    return e === void 0 && xt.set(t.strings, e = new W(t)), e;
  }
  k(t) {
    pt(this._$AH) || (this._$AH = [], this._$AR());
    const e = this._$AH;
    let s, i = 0;
    for (const a of t) i === e.length ? e.push(s = new G(this.O(B()), this.O(B()), this, this.options)) : s = e[i], s._$AI(a), i++;
    i < e.length && (this._$AR(s && s._$AB.nextSibling, i), e.length = i);
  }
  _$AR(t = this._$AA.nextSibling, e) {
    var s;
    for ((s = this._$AP) == null ? void 0 : s.call(this, !1, !0, e); t !== this._$AB; ) {
      const i = _t(t).nextSibling;
      _t(t).remove(), t = i;
    }
  }
  setConnected(t) {
    var e;
    this._$AM === void 0 && (this._$Cv = t, (e = this._$AP) == null || e.call(this, t));
  }
}
class tt {
  get tagName() {
    return this.element.tagName;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  constructor(t, e, s, i, a) {
    this.type = 1, this._$AH = h, this._$AN = void 0, this.element = t, this.name = e, this._$AM = i, this.options = a, s.length > 2 || s[0] !== "" || s[1] !== "" ? (this._$AH = Array(s.length - 1).fill(new String()), this.strings = s) : this._$AH = h;
  }
  _$AI(t, e = this, s, i) {
    const a = this.strings;
    let o = !1;
    if (a === void 0) t = T(this, t, e, 0), o = !F(t) || t !== this._$AH && t !== O, o && (this._$AH = t);
    else {
      const c = t;
      let n, d;
      for (t = a[0], n = 0; n < a.length - 1; n++) d = T(this, c[s + n], e, n), d === O && (d = this._$AH[n]), o || (o = !F(d) || d !== this._$AH[n]), d === h ? t = h : t !== h && (t += (d ?? "") + a[n + 1]), this._$AH[n] = d;
    }
    o && !i && this.j(t);
  }
  j(t) {
    t === h ? this.element.removeAttribute(this.name) : this.element.setAttribute(this.name, t ?? "");
  }
}
class Zt extends tt {
  constructor() {
    super(...arguments), this.type = 3;
  }
  j(t) {
    this.element[this.name] = t === h ? void 0 : t;
  }
}
class Kt extends tt {
  constructor() {
    super(...arguments), this.type = 4;
  }
  j(t) {
    this.element.toggleAttribute(this.name, !!t && t !== h);
  }
}
class Xt extends tt {
  constructor(t, e, s, i, a) {
    super(t, e, s, i, a), this.type = 5;
  }
  _$AI(t, e = this) {
    if ((t = T(this, t, e, 0) ?? h) === O) return;
    const s = this._$AH, i = t === h && s !== h || t.capture !== s.capture || t.once !== s.once || t.passive !== s.passive, a = t !== h && (s === h || i);
    i && this.element.removeEventListener(this.name, this, s), a && this.element.addEventListener(this.name, this, t), this._$AH = t;
  }
  handleEvent(t) {
    var e;
    typeof this._$AH == "function" ? this._$AH.call(((e = this.options) == null ? void 0 : e.host) ?? this.element, t) : this._$AH.handleEvent(t);
  }
}
class Qt {
  constructor(t, e, s) {
    this.element = t, this.type = 6, this._$AN = void 0, this._$AM = e, this.options = s;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  _$AI(t) {
    T(this, t);
  }
}
const ot = V.litHtmlPolyfillSupport;
ot == null || ot(W, G), (V.litHtmlVersions ?? (V.litHtmlVersions = [])).push("3.3.2");
const Yt = (r, t, e) => {
  const s = (e == null ? void 0 : e.renderBefore) ?? t;
  let i = s._$litPart$;
  if (i === void 0) {
    const a = (e == null ? void 0 : e.renderBefore) ?? null;
    s._$litPart$ = i = new G(t.insertBefore(B(), a), a, void 0, e ?? {});
  }
  return i._$AI(r), i;
};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const P = globalThis;
class f extends D {
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
    this.hasUpdated || (this.renderOptions.isConnected = this.isConnected), super.update(t), this._$Do = Yt(e, this.renderRoot, this.renderOptions);
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
var Et;
f._$litElement$ = !0, f.finalized = !0, (Et = P.litElementHydrateSupport) == null || Et.call(P, { LitElement: f });
const nt = P.litElementPolyfillSupport;
nt == null || nt({ LitElement: f });
(P.litElementVersions ?? (P.litElementVersions = [])).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const _ = (r) => (t, e) => {
  e !== void 0 ? e.addInitializer(() => {
    customElements.define(r, t);
  }) : customElements.define(r, t);
};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const te = { attribute: !0, type: String, converter: K, reflect: !1, hasChanged: ht }, ee = (r = te, t, e) => {
  const { kind: s, metadata: i } = e;
  let a = globalThis.litPropertyMetadata.get(i);
  if (a === void 0 && globalThis.litPropertyMetadata.set(i, a = /* @__PURE__ */ new Map()), s === "setter" && ((r = Object.create(r)).wrapped = !0), a.set(e.name, r), s === "accessor") {
    const { name: o } = e;
    return { set(c) {
      const n = t.get.call(this);
      t.set.call(this, c), this.requestUpdate(o, n, r, !0, c);
    }, init(c) {
      return c !== void 0 && this.C(o, void 0, r, c), c;
    } };
  }
  if (s === "setter") {
    const { name: o } = e;
    return function(c) {
      const n = this[o];
      t.call(this, c), this.requestUpdate(o, n, r, !0, c);
    };
  }
  throw Error("Unsupported decorator location: " + s);
};
function g(r) {
  return (t, e) => typeof e == "object" ? ee(r, t, e) : ((s, i, a) => {
    const o = i.hasOwnProperty(a);
    return i.constructor.createProperty(a, s), o ? Object.getOwnPropertyDescriptor(i, a) : void 0;
  })(r, t, e);
}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
function m(r) {
  return g({ ...r, state: !0, attribute: !1 });
}
var ie = Object.defineProperty, se = Object.getOwnPropertyDescriptor, et = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? se(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && ie(t, e, i), i;
};
let U = class extends f {
  constructor() {
    super(...arguments), this.overallStatus = "NeverChecked", this.affectedAdvisoryCount = 0, this.mitigatedAdvisoryCount = 0;
  }
  render() {
    if (this.overallStatus === "Safe")
      return l`
        <div class="status-safe">
          <uui-icon name="check" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">No Active Vulnerabilities</span>
        </div>
      `;
    if (this.overallStatus === "Mitigated")
      return l`
        <div class="status-mitigated">
          <uui-icon name="check" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">
            ${this.mitigatedAdvisoryCount}
            ${this.mitigatedAdvisoryCount === 1 ? "Vulnerability" : "Vulnerabilities"} Mitigated
          </span>
        </div>
      `;
    if (this.overallStatus === "Vulnerable") {
      const r = this.mitigatedAdvisoryCount > 0 ? ` and ${this.mitigatedAdvisoryCount} Mitigated` : "";
      return l`
        <div class="status-vulnerable">
          <uui-icon name="alert" style="font-size: 2rem;"></uui-icon>
          <span class="status-label">
            ${this.affectedAdvisoryCount} Active${r}
            ${this.affectedAdvisoryCount === 1 ? "Vulnerability" : "Vulnerabilities"} Found
          </span>
        </div>
      `;
    }
    return l`
      <div class="status-neutral">
        <uui-icon name="info" style="font-size: 2rem;"></uui-icon>
        <span class="status-label">Not yet checked</span>
      </div>
    `;
  }
};
U.styles = y`
    :host { display: block; }
    .status-safe { color: var(--uui-color-positive, #00a152); display: flex; align-items: center; gap: 8px; }
    .status-mitigated { color: var(--uui-color-warning, #f5a623); display: flex; align-items: center; gap: 8px; }
    .status-vulnerable { color: var(--uui-color-danger, #d0011b); display: flex; align-items: center; gap: 8px; }
    .status-neutral { color: var(--uui-color-text, #333); display: flex; align-items: center; gap: 8px; }
    .status-label { font-size: 1.2rem; font-weight: 600; }
  `;
et([
  g({ type: String })
], U.prototype, "overallStatus", 2);
et([
  g({ type: Number })
], U.prototype, "affectedAdvisoryCount", 2);
et([
  g({ type: Number })
], U.prototype, "mitigatedAdvisoryCount", 2);
U = et([
  _("security-dashboard-status-indicator")
], U);
var re = Object.defineProperty, ae = Object.getOwnPropertyDescriptor, R = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? ae(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && re(t, e, i), i;
};
let S = class extends f {
  constructor() {
    super(...arguments), this.isStale = !1, this.scanningDisabled = !1, this.lastSuccessfulCheckAt = null, this.lastCheckSucceeded = null, this.lastCheckError = null;
  }
  render() {
    const r = this.scanningDisabled, t = !r && this.isStale, e = this.lastCheckSucceeded === !1;
    return !r && !t && !e ? l`` : l`
      ${r ? l`
        <div class="disabled-notice">
          <strong>Automatic scanning is disabled</strong> — set <code>Frequency</code> to <code>Daily</code> or <code>Weekly</code> in <code>appsettings.json</code> to re-enable scheduled checks.
        </div>
      ` : ""}
      ${t ? l`
        <div class="stale-warning">
          <strong>Data may be outdated</strong> — the last successful check was more than 48 hours ago.
        </div>
      ` : ""}
      ${e ? l`
        <div class="failure-notice">
          <strong>Last check failed</strong>${this.lastCheckError ? `: ${this.lastCheckError}` : "."}
        </div>
      ` : ""}
    `;
  }
};
S.styles = y`
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
    .disabled-notice {
      margin-top: 8px;
      padding: 8px 12px;
      background: var(--uui-color-danger-surface, #fde8e8);
      border: 1px solid var(--uui-color-danger, #d0011b);
      border-radius: 4px;
      color: black;
      font-size: 0.875rem;
    }
  `;
R([
  g({ type: Boolean })
], S.prototype, "isStale", 2);
R([
  g({ type: Boolean })
], S.prototype, "scanningDisabled", 2);
R([
  g({ type: String })
], S.prototype, "lastSuccessfulCheckAt", 2);
R([
  g({ type: Boolean })
], S.prototype, "lastCheckSucceeded", 2);
R([
  g({ type: String })
], S.prototype, "lastCheckError", 2);
S = R([
  _("security-dashboard-staleness-warning")
], S);
var oe = Object.defineProperty, ne = Object.getOwnPropertyDescriptor, gt = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? ne(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && oe(t, e, i), i;
};
let q = class extends f {
  constructor() {
    super(...arguments), this.lastSuccessfulCheckAt = null, this.nextScheduledCheckAt = "";
  }
  formatTimestamp(r) {
    if (!r) return "Not yet run";
    const t = new Date(r), s = (/* @__PURE__ */ new Date()).getTime() - t.getTime(), i = Math.floor(s / 6e4), a = Math.floor(i / 60), o = Math.floor(a / 24);
    return i < 1 ? "Just now" : i < 60 ? `${i} minute${i === 1 ? "" : "s"} ago` : a < 24 ? `${a} hour${a === 1 ? "" : "s"} ago` : o < 7 ? `${o} day${o === 1 ? "" : "s"} ago` : new Intl.DateTimeFormat(void 0, {
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit"
    }).format(t);
  }
  formatFutureTimestamp(r) {
    const t = new Date(r), e = /* @__PURE__ */ new Date(), s = t.getTime() - e.getTime(), i = Math.ceil(s / 6e4), a = Math.ceil(i / 60);
    return i < 60 ? `In ${i} minute${i === 1 ? "" : "s"}` : a < 24 ? `In ${a} hour${a === 1 ? "" : "s"}` : new Intl.DateTimeFormat(void 0, {
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit"
    }).format(t);
  }
  render() {
    return l`
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
q.styles = y`
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
gt([
  g({ type: String })
], q.prototype, "lastSuccessfulCheckAt", 2);
gt([
  g({ type: String })
], q.prototype, "nextScheduledCheckAt", 2);
q = gt([
  _("security-dashboard-check-schedule")
], q);
var le = Object.defineProperty, ce = Object.getOwnPropertyDescriptor, Tt = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? ce(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && le(t, e, i), i;
};
const Ct = {
  Safe: "/App_Plugins/SecurityDashboard/images/status-safe.png",
  Mitigated: "/App_Plugins/SecurityDashboard/images/status-mitigated.png",
  Vulnerable: "/App_Plugins/SecurityDashboard/images/status-vulnerable.png",
  NeverChecked: "/App_Plugins/SecurityDashboard/images/status-never-checked.png"
};
let Q = class extends f {
  render() {
    const r = this.status, t = Ct[r.overallStatus] ?? Ct.NeverChecked;
    return l`
      <img class="status-image" src=${t} alt=${r.overallStatus} />
      <div class="content">
        <security-dashboard-status-indicator
          .overallStatus=${r.overallStatus}
          .affectedAdvisoryCount=${r.affectedAdvisoryCount}
          .mitigatedAdvisoryCount=${r.mitigatedAdvisoryCount}>
        </security-dashboard-status-indicator>

        <security-dashboard-staleness-warning
          .isStale=${r.isStale}
          .scanningDisabled=${r.scanningDisabled}
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
Q.styles = y`
    :host { display: flex; align-items: flex-start; gap: 16px; }
    .status-image { width: 100px; height: 100px; flex-shrink: 0; }
    .content { flex: 1; }
  `;
Tt([
  g({ attribute: !1 })
], Q.prototype, "status", 2);
Q = Tt([
  _("security-dashboard-header")
], Q);
var ue = Object.defineProperty, de = Object.getOwnPropertyDescriptor, H = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? de(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && ue(t, e, i), i;
};
let w = class extends lt(f) {
  constructor() {
    super(...arguments), this.mode = "mark", this.ghsaId = "", this._description = "", this._submitting = !1, this._error = null;
  }
  async _getToken() {
    return (await this.getContext(ct)).getLatestToken();
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
    return this.mode === "remove" ? l`
        <uui-dialog>
          <uui-dialog-layout headline="Remove Mitigation">
            <p>Are you sure you want to remove this manual mitigation? The advisory will revert to its automatically calculated status.</p>
            ${this._error ? l`<div class="dialog-error">${this._error}</div>` : ""}
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
      ` : l`
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
          ${this._error ? l`<div class="dialog-error">${this._error}</div>` : ""}
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
w.styles = y`
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
H([
  g()
], w.prototype, "mode", 2);
H([
  g()
], w.prototype, "ghsaId", 2);
H([
  m()
], w.prototype, "_description", 2);
H([
  m()
], w.prototype, "_submitting", 2);
H([
  m()
], w.prototype, "_error", 2);
w = H([
  _("security-dashboard-mitigation-dialog")
], w);
var he = Object.defineProperty, pe = Object.getOwnPropertyDescriptor, it = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? pe(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && he(t, e, i), i;
};
let N = class extends f {
  constructor() {
    super(...arguments), this._showMarkDialog = !1, this._showRemoveDialog = !1;
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
    this._showMarkDialog = !1, this._showRemoveDialog = !1;
  }
  render() {
    if (!this.advisory) return l``;
    var r = "";
    this.advisory.packages.forEach((i) => {
      r.includes(i.packageName + " — ") ? r += `${i.affectedVersionRange}, ` : r += `${i.packageName} — ${i.affectedVersionRange}, `;
    });
    const t = (this.advisory.affectedStatus === "Vulnerable" || this.advisory.affectedStatus === "Unknown") && !this.advisory.manualMitigation, e = this.advisory.affectedStatus === "Mitigated" && !!this.advisory.manualMitigation, s = this.advisory.manualMitigation ? new Date(this.advisory.manualMitigation.mitigatedAt).toLocaleDateString() : "";
    return l`
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
            ${this.advisory.manualMitigation ? l`
              <div class="mitigation-attribution">
                <span class="attribution-who">
                  Manually mitigated by ${this.advisory.manualMitigation.mitigatedBy} on ${s}
                </span>
                <div class="attribution-description">${this.advisory.manualMitigation.description}</div>
              </div>
            ` : this.advisory.affectedStatus === "Mitigated" && this.advisory.exposureCheckMitigationDescription ? l`
              <div class="mitigation-attribution">
                <span class="attribution-who">Auto-mitigated</span>
                <div class="attribution-description">${this.advisory.exposureCheckMitigationDescription}</div>
              </div>
            ` : ""}
          </div>
          ${t ? l`
            <uui-button
              look="outline"
              color="positive"
              @click=${() => {
      this._showMarkDialog = !0;
    }}>
              Mark As Mitigated
            </uui-button>
          ` : e ? l`
            <uui-button
              look="secondary"
              color="danger"
              @click=${() => {
      this._showRemoveDialog = !0;
    }}>
              Remove Mitigation
            </uui-button>
          ` : l`<span></span>`}
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

        </div>
        ${this._showMarkDialog ? l`
          <security-dashboard-mitigation-dialog
            mode="mark"
            ghsaId=${this.advisory.ghsaId}
            @mitigation-changed=${this._onMitigationChanged}
            @mitigation-cancelled=${() => {
      this._showMarkDialog = !1;
    }}
          ></security-dashboard-mitigation-dialog>
        ` : ""}
        ${this._showRemoveDialog ? l`
          <security-dashboard-mitigation-dialog
            mode="remove"
            ghsaId=${this.advisory.ghsaId}
            @mitigation-changed=${this._onMitigationChanged}
            @mitigation-cancelled=${() => {
      this._showRemoveDialog = !1;
    }}
          ></security-dashboard-mitigation-dialog>
        ` : ""}
      </div>
    `;
  }
};
N.styles = y`
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
it([
  g({ type: Object })
], N.prototype, "advisory", 2);
it([
  m()
], N.prototype, "_showMarkDialog", 2);
it([
  m()
], N.prototype, "_showRemoveDialog", 2);
N = it([
  _("security-dashboard-advisory-item")
], N);
var ge = Object.defineProperty, fe = Object.getOwnPropertyDescriptor, Ut = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? fe(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && ge(t, e, i), i;
};
let Y = class extends f {
  constructor() {
    super(...arguments), this.advisories = [];
  }
  render() {
    const r = this.advisories.filter(
      (e) => e.affectedStatus === "Vulnerable" || e.affectedStatus === "Mitigated"
    ), t = this.advisories.filter(
      (e) => e.affectedStatus === "Unknown" || e.affectedStatus === "NotAffected"
    );
    return l`
      <h3>Active Vulnerabilities</h3>
      ${r.length === 0 ? l`<div class="empty-state">No active vulnerabilities found.</div>` : r.map((e) => l`
            <security-dashboard-advisory-item .advisory=${e}></security-dashboard-advisory-item>
          `)}

      ${t.length > 0 ? l`
        <h3>Known Advisories</h3>
        ${t.map((e) => l`
          <security-dashboard-advisory-item .advisory=${e}></security-dashboard-advisory-item>
        `)}
      ` : ""}
    `;
  }
};
Y.styles = y`
    :host { display: block; }
    h3 { margin: 16px 0 8px; font-size: 1rem; font-weight: 600; }
    .empty-state {
      padding: 16px;
      text-align: center;
      color: var(--uui-color-text-alt, #666);
      font-style: italic;
    }
  `;
Ut([
  g({ type: Array })
], Y.prototype, "advisories", 2);
Y = Ut([
  _("security-dashboard-advisory-list")
], Y);
var me = Object.defineProperty, ve = Object.getOwnPropertyDescriptor, j = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? ve(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && me(t, e, i), i;
};
const I = 25;
let x = class extends lt(f) {
  constructor() {
    super(...arguments), this._entries = [], this._totalCount = 0, this._skip = 0, this._loading = !0, this._error = null;
  }
  connectedCallback() {
    super.connectedCallback(), this._fetchPage(0);
  }
  async _fetchPage(r) {
    this._loading = !0, this._error = null;
    try {
      const t = await this.getContext(ct);
      if (!t) {
        this._error = "Authentication context not available.";
        return;
      }
      const e = await t.getLatestToken(), s = await fetch(
        `/umbraco/management/api/v1/security-dashboard/audit-log?skip=${r}&take=${I}`,
        { headers: { Authorization: `Bearer ${e}` } }
      );
      if (!s.ok) {
        this._error = `Server error (${s.status}).`;
        return;
      }
      const i = await s.json();
      this._entries = i.entries, this._totalCount = i.totalCount, this._skip = r;
    } catch (t) {
      this._error = t instanceof Error ? t.message : "Failed to load audit log.";
    } finally {
      this._loading = !1;
    }
  }
  _onPageChange(r) {
    const t = (r.detail.current - 1) * I;
    this._fetchPage(t);
  }
  _formatTimestamp(r) {
    return new Date(r).toLocaleString();
  }
  render() {
    return l`
      <uui-box>
        <h3>Audit History</h3>

        ${this._loading ? l`<uui-loader></uui-loader>` : ""}

        ${this._error ? l`<p>${this._error}</p>` : ""}

        ${!this._loading && !this._error && this._totalCount === 0 ? l`
          <p class="empty-state">No audit log entries yet.</p>
        ` : ""}

        ${!this._loading && !this._error && this._totalCount > 0 ? l`
          <uui-table>
            <uui-table-head>
              <uui-table-head-cell>Timestamp</uui-table-head-cell>
              <uui-table-head-cell>Status</uui-table-head-cell>
              <uui-table-head-cell>Type</uui-table-head-cell>
              <uui-table-head-cell>Actor</uui-table-head-cell>
              <uui-table-head-cell>Description</uui-table-head-cell>
            </uui-table-head>
            ${this._entries.map((r) => l`
              <uui-table-row>
                <uui-table-cell>${this._formatTimestamp(r.timestamp)}</uui-table-cell>
                <uui-table-cell>${r.overallStatus}</uui-table-cell>
                <uui-table-cell>${r.actionType}</uui-table-cell>
                <uui-table-cell>
                  ${r.actorName ? r.actorName : l`<span class="actor-none">System</span>`}
                </uui-table-cell>
                <uui-table-cell>${r.description}</uui-table-cell>
              </uui-table-row>
            `)}
          </uui-table>

          ${this._totalCount > I ? l`
            <div class="pagination">
              <uui-pagination
                .total=${Math.ceil(this._totalCount / I)}
                .current=${Math.floor(this._skip / I) + 1}
                @change=${this._onPageChange}>
              </uui-pagination>
            </div>
          ` : ""}
        ` : ""}
      </uui-box>
    `;
  }
};
x.styles = y`
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
j([
  m()
], x.prototype, "_entries", 2);
j([
  m()
], x.prototype, "_totalCount", 2);
j([
  m()
], x.prototype, "_skip", 2);
j([
  m()
], x.prototype, "_loading", 2);
j([
  m()
], x.prototype, "_error", 2);
x = j([
  _("security-audit-log")
], x);
var ye = Object.defineProperty, _e = Object.getOwnPropertyDescriptor, st = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? _e(t, e) : t, a = r.length - 1, o; a >= 0; a--)
    (o = r[a]) && (i = (s ? o(t, e, i) : o(i)) || i);
  return s && i && ye(t, e, i), i;
};
let k = class extends lt(f) {
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
      const r = await this.getContext(ct);
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
      return l`
        <uui-box>
          <h2>Security Health</h2>
          <uui-loader></uui-loader>
        </uui-box>
      `;
    if (this._error)
      return l`
        <uui-box>
          <h2>Security Health</h2>
          <div class="error-box">${this._error}</div>
        </uui-box>
      `;
    const r = this._status;
    return l`
      <uui-box>
        <h2>Security Health</h2>

        <security-dashboard-header
          .status=${r}>
        </security-dashboard-header>

        ${r.advisories.length > 0 ? l`
          <security-dashboard-advisory-list
            .advisories=${r.advisories}>
          </security-dashboard-advisory-list>
        ` : ""}
      </uui-box>

      <security-audit-log></security-audit-log>
    `;
  }
};
k.styles = y`
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
st([
  m()
], k.prototype, "_status", 2);
st([
  m()
], k.prototype, "_loading", 2);
st([
  m()
], k.prototype, "_error", 2);
k = st([
  _("security-dashboard")
], k);
const we = k;
export {
  k as SecurityDashboardElement,
  we as default
};
