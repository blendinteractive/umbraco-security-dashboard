# Research: 009-exposure-mitigation-desc

No external unknowns identified. All decisions are based on existing codebase patterns.
Migration version confirmed by reading `SecurityDashboardMigrationPlan.cs`.

---

## Decision 1: Return type for `IExposureCheck.CheckAsync`

**Decision**: Introduce `ExposureCheckResult(ExposureVerdict Verdict, string? MitigationDescription = null)` as a new C# record and change `CheckAsync` to return `Task<ExposureCheckResult>`.

**Rationale**: Keeps the verdict and its associated description co-located. The nullable `MitigationDescription` is only set when `Verdict == Mitigated`; all other call sites remain unchanged structurally (access `.Verdict`).

**Alternatives considered**:
- Add a `string? MitigationDescription` property directly to `IExposureCheck`. Rejected: property would be meaningful only after a call; coupling description to the instance rather than the result is semantically confusing and breaks single-call semantics.
- Return `(ExposureVerdict, string?)` tuple. Rejected: named record is more readable and refactor-friendly.

---

## Decision 2: Return type for `IExposureCheckEvaluator.EvaluateAsync`

**Decision**: Change from `Task<string>` to `Task<ExposureEvaluationResult>` where `ExposureEvaluationResult(string Verdict, string? MitigationDescription)`.

**Rationale**: `VulnerabilityService` is the sole consumer. A named record cleanly extends the contract without breaking the existing verdict-string behavior; the caller destructures what it needs.

**Alternatives considered**:
- `out` parameter. Rejected: async methods cannot have `out` parameters.
- Return tuple. Rejected: named record is more readable.

---

## Decision 3: Description joining strategy in the evaluator

**Decision**: Collect all non-null `MitigationDescription` values from checks that returned `Mitigated`. Join with `"; "`. If none provide a description (all null), use the fallback `"Mitigated by exposure check"`. When the worst verdict is not `Mitigated`, set `MitigationDescription` to `null`.

**Rationale**: Matches spec clarification (semicolon-separated, no silent discard). Fallback satisfies FR-006 and SC-001 (100% of Mitigated advisories must show a non-blank description).

---

## Decision 4: Storage column placement and type

**Decision**: `ExposureCheckDescription NVARCHAR(MAX) NULL` added to `SecurityDashboard_Advisory`.

**Rationale**: Descriptions are produced per evaluator call, which is per advisory (per GHSA ID). Rows in `SecurityDashboard_Advisory` are per-package per-scan; all rows for the same GHSA ID in the same scan share the same description, so each row stores a copy. `NVARCHAR(MAX)` is consistent with other long nullable string fields and accommodates multi-check joins without an arbitrary length cap. Nullable because pre-existing rows from prior scans have no description.

**Alternatives considered**:
- Store on `CheckResultRecord`. Rejected: descriptions are per-advisory (evaluator runs once per advisory), not per scan.
- Separate join table. Rejected: over-engineering for a single nullable string.

---

## Decision 5: Migration version

**Decision**: `SecurityDashboard-1.3.0` — appended after `SecurityDashboard-1.2.0` (AddAuditLogTable, feature 007). Feature 008 had no schema change.

**Confirmed from**: `src/Umbraco.SecurityDashboard/Migrations/SecurityDashboardMigrationPlan.cs`

---

## Decision 6: Frontend display

**Decision**: Show a styled `div` with class `mitigation-attribution` (reusing existing CSS) below the package list when the advisory is `Mitigated` and has no `manualMitigation`. Label it `"Auto-mitigated"` instead of `"Manually mitigated by …"`.

**Rationale**: Spec says "matches or closely resembles the existing manual mitigation description display, minimizing UI complexity." Reusing `.mitigation-attribution` and `.attribution-description` achieves this with minimal new CSS. The absence of attribution metadata (who/when) distinguishes it visually without requiring a new style class.

**Alternatives considered**:
- Inline text without box. Rejected: inconsistent with manual mitigation display.
- New CSS class with distinct color. Left as an implementation option but not required by spec.

---

## Decision 7: Description text for existing checks

**Decision**:
- `ContentDeliveryApiExposureCheck` (Mitigated path): `"Content Delivery API is disabled"`
- `NonAdminUsersExposureCheck` (Mitigated path): `"All backoffice users are administrators"`

**Rationale**: Concise, plain-language strings matching the condition the check evaluates. These are static per check (spec assumption: description does not vary per advisory or per scan run).
