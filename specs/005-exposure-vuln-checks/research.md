# Research: Exposure-Based Vulnerability Checks

## Decision 1 — Advisory Description Field

**Decision**: Add `Description` property to `GitHubAdvisory` with `[JsonPropertyName("description")]`.

**Rationale**: The GitHub Security Advisory REST API (`GET /advisories`) returns a `description` field containing the full markdown body of the advisory. This is distinct from `summary` (already mapped). The `### Exposure` section lives in this body. Adding one property to the existing model is sufficient — no new API calls are needed.

**Alternatives considered**: Fetching advisory detail separately (per-advisory GET) — rejected because it would multiply API calls by the number of matched advisories and add latency with no benefit.

---

## Decision 2 — Exposure Check Architecture

**Decision**: Three-layer design:
1. `ExposureKeywordParser` — static utility that extracts keywords from a markdown description string.
2. `IExposureCheck` / implementations — per-keyword check logic.
3. `ExposureCheckEvaluator` — singleton that holds all registered checks, runs matching ones, and returns the worst-case `ExposureVerdict`.

**Rationale**: Separating parsing from evaluation from individual checks keeps each unit independently testable and satisfies the single-responsibility principle. The evaluator is the only class that needs to know about the registry; `VulnerabilityService` only sees `IExposureCheckEvaluator`.

**Alternatives considered**: A single "ExposureService" that does parsing + evaluation — rejected because it mixes concerns and makes unit testing harder.

---

## Decision 3 — ExposureVerdict Enum

**Decision**: Define `ExposureVerdict { NotAffected = 0, Mitigated = 1, Vulnerable = 2 }` with numeric values so severity can be compared with `Math.Max` / `.Max()`.

**Rationale**: Numeric ordering allows trivial worst-case selection (`checks.Max(c => c.Verdict)`) without any conditional logic. Maps directly to the spec's "Vulnerable > Mitigated > NotAffected" requirement.

**Alternatives considered**: String comparison — rejected as fragile. A dedicated comparator — rejected as unnecessary.

---

## Decision 4 — DI Lifetime and Singleton Safety

**Decision**: All `IExposureCheck` implementations registered as **singletons**. `NonAdminUsersExposureCheck` receives `IServiceScopeFactory` in its constructor and creates a scope on each check call. `ExposureCheckEvaluator` receives `IEnumerable<IExposureCheck>` (injected at construction by the DI container).

**Rationale**: `VulnerabilityService` is a singleton. The evaluator must also be a singleton. `IUserService` in Umbraco is scoped, so the non-admin check cannot hold a direct reference. `IServiceScopeFactory` is the canonical .NET pattern for resolving scoped services from singletons. `IOptions<DeliveryApiSettings>` is singleton-safe and requires no scope.

**Alternatives considered**: Making exposure checks scoped and resolving via factory in the evaluator — more moving parts for no benefit when one check (CDA) needs no scope at all.

---

## Decision 5 — Content Delivery API Check

**Decision**: Inject `IOptions<DeliveryApiSettings>` from `Umbraco.Cms.Core.Configuration.Models`. Return `Vulnerable` if `settings.Value.Enabled == true`; `Mitigated` if `false`.

**Config key**: `Umbraco:CMS:DeliveryApi:Enabled` in `appsettings.json`.

**Rationale**: This is the canonical Umbraco way to read typed configuration. No network probe or reflection needed.

**Keyword**: `Content Delivery API` (as specified in FR-012).

---

## Decision 6 — Non-Admin Backoffice Users Check

**Decision**: Resolve `IUserService` within a `IServiceScopeFactory`-created scope. Call `GetAll(0, int.MaxValue, out _)` to enumerate all users. Return `Vulnerable` if any user's groups do not include the group with alias `Constants.Security.AdminGroupAlias` (`"admin"`); `Mitigated` otherwise.

**Rationale**: `IUserService.GetAll` is the standard API for enumerating Umbraco backoffice users. Checking group aliases is more robust than checking group IDs (which can vary between installations).

**Edge case**: If `GetAll` returns an empty collection (no users at all), return `Mitigated` — a site with no users cannot be exploited via non-admin backoffice access.

**Keyword**: `Non-Admin Backoffice Users` (as specified in FR-011).

**Note**: In Umbraco 17, `IUserService` may expose async overloads; prefer async if available, fall back to synchronous. Wrap in `Task.Run` if necessary to avoid blocking.

---

## Decision 7 — Status Value Replacement

**Decision**: Replace the string value `"Affected"` with `"Vulnerable"` everywhere in the codebase. New four-value set: `NotAffected`, `Mitigated`, `Vulnerable`, `Unknown`.

**Rationale**: The spec (FR-009) explicitly replaces `Affected` with `Vulnerable`. No database migration is required — the `AffectedStatus` VARCHAR(20) column accommodates all values. Existing data has been cleared (per clarification).

**Impact surface**:
- `VulnerabilityService.DetermineAffectedStatus()` returns `"Vulnerable"` instead of `"Affected"`
- `VulnerabilityService.ConsolidateStatus()` updated to handle `Vulnerable` and `Mitigated`
- `VulnerabilityService.GetDashboardStatusAsync()` ordering updated (Vulnerable > Mitigated > Unknown > NotAffected)
- `VulnerabilityService.RunCheckAsync()` webhook filter updated (`"Vulnerable"` or `"Unknown"`)
- `AdvisoryDto` / `AdvisoryPackageDto` doc comments updated
- `client/src/types.ts` union type updated
- `client/src/components/advisory-item.element.ts` `getStatusColor` updated

---

## Decision 8 — Exposure Check Execution Point

**Decision**: Exposure checks run once per **advisory** (not per vulnerability/package) within `RunCheckAsync`. Keywords are parsed from `advisory.Description` before the inner package loop. The exposure verdict is applied to all version-matched packages in that advisory.

**Rationale**: The `### Exposure` section describes conditions that apply to the advisory as a whole, not to individual affected packages within it. Running the check once per advisory is more efficient and semantically correct.

---

## Decision 9 — Frontend Status Colours

**Decision**: Update `getStatusColor` in `advisory-item.element.ts`:
- `Vulnerable` → `'danger'` (red) — replaces `Affected`
- `Mitigated` → `'caution'` (yellow)
- `Unknown` → `'warning'` (orange/amber)
- `NotAffected` → `'positive'` (green)

Update label display: `Mitigated` renders as `"Mitigated"` (no special case needed). `NotAffected` continues to render as `"Not Affected"`.

**Rationale**: Matches FR-010 colour spec. `uui-tag` color values are the existing UUI design system tokens already used in the component.

---

## Unresolved Items

None. All NEEDS CLARIFICATION items have been resolved.
