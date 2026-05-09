# Feature Specification: Exposure-Based Vulnerability Checks

**Feature Branch**: `005-exposure-vuln-checks`  
**Created**: 2026-05-09  
**Status**: Draft  

## Overview

Not every security advisory affects every Umbraco installation equally — a vulnerability that requires non-admin users, or that is only exploitable when the Content Delivery API is active, does not affect installations without those conditions. This feature introduces an extensible system of site-configuration checks that evaluate whether a matched advisory's exposure conditions are actually present, so that administrators receive an accurate picture of their real risk rather than a worst-case assumption.

When an advisory is determined to affect an installed package by version range, its description is scanned for an `### Exposure` section. Keywords found there trigger registered checks. Each check inspects the running site's configuration and returns one of two verdicts: **Mitigated**, or **Vulnerable**. Advisories with no applicable checks remain **Vulnerable** by default.

The current binary `Affected / NotAffected / Unknown` status is replaced by this three-value scale: **Not Affected, **Mitigated**, and **Vulnerable**

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Accurate Risk Assessment for Conditional Vulnerabilities (Priority: P1)

A site administrator views the security dashboard after a new Umbraco advisory is published. The advisory includes an `### Exposure` section indicating it only applies to sites with non-admin members. The administrator's site has no non-admin users. The dashboard shows the advisory as **Mitigated** (yellow) rather than the generic "Affected" it would have shown previously.

**Why this priority**: This is the core value of the feature — reducing false positives so administrators can focus on real threats.

**Independent Test**: Can be fully tested by seeding an advisory with a matching `### Exposure` keyword, configuring the site without the exposure condition, and verifying the dashboard displays "Mitigated" for that advisory.

**Acceptance Scenarios**:

1. **Given** an advisory whose version range matches an installed package and whose description contains `### Exposure` followed by `* *Non-Admin Users*`, **When** the site has no non-admin users, **Then** the advisory is displayed with status **Mitigated** (yellow).
2. **Given** the same advisory, **When** the site has at least one non-admin user, **Then** the advisory is displayed with status **Vulnerable** (red).
3. **Given** an advisory whose `### Exposure` section contains a keyword for which no check is registered, or contains no `### Exposure` section, **When** the dashboard loads, **Then** the advisory is displayed as **Vulnerable** (red) — unrecognised keywords do not suppress warnings.

---

### User Story 2 - Mitigated Status for Partially-Addressed Vulnerabilities (Priority: P2)

A site administrator has the Content Delivery API installed but has disabled it via configuration. A relevant advisory lists `Content Delivery API` as an exposure keyword. The dashboard shows the advisory as **Mitigated** (yellow) to signal that the feature is present but currently inactive.

**Why this priority**: The mitigated state gives administrators meaningful nuance — they know the risk exists but is currently controlled, and they will be alerted if the configuration changes.

**Independent Test**: Can be tested by enabling the Content Delivery API feature in configuration, then verifying the dashboard shows "Vulnerable" for a matching advisory. Disabling the API would show "Mitigated"

**Acceptance Scenarios**:

1. **Given** an advisory whose exposure includes `Content Delivery API`, **When** the API is installed but disabled in configuration, **Then** status is **Mitigated** (yellow).
2. **Given** the same advisory, **When** the Content Delivery API is enabled and publicly accessible, **Then** status is **Vulnerable** (red).

---

### User Story 3 - Advisories Without Exposure Checks Default to Vulnerable (Priority: P2)

An advisory matches an installed package by version range but contains no `### Exposure` section (or contains exposure keywords with no matching registered checks). The dashboard treats this advisory as **Vulnerable** (red) to maintain a safe default posture.

**Why this priority**: Preserving the fail-safe default ensures new or unrecognised advisories are never silently dismissed.

**Independent Test**: Can be tested with a synthetic advisory that has no `### Exposure` heading and verifying the dashboard displays "Vulnerable."

**Acceptance Scenarios**:

1. **Given** a version-matched advisory with no `### Exposure` section, **When** the dashboard loads, **Then** status is **Vulnerable** (red).
2. **Given** a version-matched advisory whose `### Exposure` section contains only unregistered keywords, **When** the dashboard loads, **Then** status is **Vulnerable** (red).

---

### User Story 4 - Extensible Check Registration (Priority: P3)

A developer wants to add a new exposure check for a keyword not covered in the initial set (e.g., `Public Registration`). They can register a new check without modifying the core advisory-processing logic, and the check is automatically applied to any advisory whose exposure section includes the corresponding keyword.

**Why this priority**: Extensibility ensures the system remains useful as new advisory patterns emerge without requiring core changes.

**Independent Test**: Can be tested by registering a custom check implementation, seeding an advisory with the matching keyword, and verifying the check's verdict is applied to the advisory status.

**Acceptance Scenarios**:

1. **Given** a new check registered against keyword `Public Registration`, **When** an advisory contains `* *Public Registration*` in its exposure section, **Then** the registered check is invoked and its verdict determines the advisory status.
2. **Given** multiple checks registered, **When** an advisory exposure section contains multiple matching keywords, **Then** all matching checks run and the most severe verdict (Vulnerable > Mitigated > Not Affected) determines the final status.

---

### Edge Cases

- What happens when the `### Exposure` section exists but has no bullet-point keywords? Advisory is treated as Vulnerable (fail-safe).
- What happens if a check throws an unexpected error? The check is treated as Vulnerable to maintain fail-safe posture; the error is logged.
- What happens when multiple checks match and return differing verdicts? The most severe result wins (Vulnerable > Mitigated > Not Affected).
- What happens if the advisory description is null or empty? Advisory status defaults to Vulnerable.
- What if a keyword matches more than one registered check? All matching checks run; the most severe verdict wins.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST parse advisory descriptions for a `### Exposure` heading when evaluating an advisory that matches an installed package by version range.
- **FR-002**: The system MUST extract keywords from lines matching the pattern `* *[Keyword]*` in the text following the `### Exposure` heading.
- **FR-003**: The system MUST maintain a registry of checks, where each check is associated with one or more keywords.
- **FR-004**: When keywords are found, the system MUST invoke all registered checks whose keywords match any extracted keyword.
- **FR-005**: Each check MUST return one of three verdicts: **Not Affected**, **Mitigated**, or **Vulnerable**.
- **FR-006**: When multiple checks run for a single advisory, the system MUST use the most severe verdict as the final status (Vulnerable > Mitigated > Not Affected).
- **FR-007**: When no matching checks exist for any extracted keyword, the advisory MUST be treated as **Vulnerable**.
- **FR-008**: When no `### Exposure` section is present in an otherwise version-matched advisory, the advisory MUST be treated as **Vulnerable**.
- **FR-009**: The `AffectedStatus` field MUST support four values: `NotAffected`, `Mitigated`, `Vulnerable`, and `Unknown` (replacing the previous `Affected / NotAffected / Unknown` set).
- **FR-010**: The dashboard UI MUST display **Not Affected** in green, **Mitigated** in yellow, and **Vulnerable** in red.
- **FR-011**: The system MUST include a built-in check for the keyword `Non-Admin Backoffice Users` that returns **Vulnerable** if any users with a role other than Administrator exist, and **Mitigated** otherwise.
- **FR-012**: The system MUST include a built-in check for the keyword `Content Delivery API` that returns **Vulnerable** if the Content Delivery API is enabled and **Mitigated** if disabled.
- **FR-013**: New checks MUST be registerable without modifying the core advisory-evaluation logic.
- **FR-014**: Check errors MUST be logged and treated as **Vulnerable** to preserve fail-safe posture.

### Key Entities

- **Exposure Check**: A named, keyword-associated unit of logic that inspects site configuration and returns a verdict (Mitigated / Vulnerable).
- **Check Registry**: A collection of all registered exposure checks, keyed by keyword, consulted during advisory evaluation.
- **Exposure Verdict**: The outcome of one or more checks for a single advisory — the most severe result among all checks that ran.
- **Advisory Status**: The final per-advisory classification shown in the dashboard: `NotAffected`, `Mitigated`, or `Vulnerable`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Advisories that match a site's installed package version range but whose exposure conditions are not met are displayed as **Mitigated**, reducing false-positive alerts for compliant configurations.
- **SC-002**: Advisories whose exposure conditions are present but inactive (e.g., feature disabled) are displayed as **Mitigated**, providing administrators a clear signal that risk is controlled but latent.
- **SC-003**: Advisories with no exposure checks or unrecognised keywords are always displayed as **Vulnerable** — no version-matched advisory is silently dismissed.
- **SC-004**: A developer can add a new exposure check and keyword mapping without editing any existing advisory-processing code.
- **SC-005**: Dashboard status colours (green / yellow / red) correctly reflect the three-value scale for 100% of evaluated advisories.

---

## Assumptions

- Umbraco advisories that include exposure keywords follow the `### Exposure` heading with `* *[Keyword]*` bullet format; advisories without this format are treated as Vulnerable.
- The `Non-Admin Backoffice Users` check uses Umbraco's built-in user store to determine whether non-admin users exist; no external identity system is assumed.
- The `Content Delivery API` check inspects the Umbraco application configuration at runtime; the definition of "enabled" is that the API is configured and its public endpoint is reachable.
- Existing `Affected` records in stored data will be migrated or re-evaluated to `Vulnerable` to preserve the fail-safe default.
- The extensibility mechanism is code-level (e.g., dependency injection registration); a UI for managing checks is out of scope.
- Webhook payloads that include advisory status should reflect the new three-value scale.
