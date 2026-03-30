<!--
SYNC IMPACT REPORT
==================
Version change: (none) → 1.0.0 (initial ratification — template fully populated)

Modified principles: N/A (first version)

Added sections:
  - I. Clean Code & Simplicity
  - II. Minimal External Dependencies
  - III. Test-First Development
  - IV. Umbraco UX Consistency
  - V. Security by Design
  - Technology Constraints
  - Development Workflow
  - Governance

Removed sections: N/A

Templates checked:
  ✅ .specify/templates/plan-template.md — Constitution Check section is generic; no updates needed
  ✅ .specify/templates/spec-template.md — structure aligns with principles; no updates needed
  ✅ .specify/templates/tasks-template.md — task categories align with principles; no updates needed
  ⚠  .specify/templates/commands/ — directory does not exist; nothing to update

Deferred TODOs:
  - TODO(RATIFICATION_DATE): Using today's date as no prior record exists.
-->

# Security Dashboard Constitution

## Core Principles

### I. Clean Code & Simplicity

Code MUST be written for readability and maintainability first. Every function,
class, and module MUST have a single, clear responsibility. Complexity MUST be
justified — if a simpler approach exists, it MUST be used instead.

- Avoid over-engineering: implement only what is required by the current feature.
- Follow SOLID principles; avoid deep inheritance hierarchies.
- Names MUST be descriptive and unambiguous — abbreviations are not permitted
  unless they are universally understood within the Umbraco ecosystem.
- Dead code MUST NOT be committed; remove rather than comment out.

**Rationale**: A security dashboard will be maintained and audited over time.
Clarity reduces the likelihood of security-relevant bugs introduced by
misunderstanding existing logic.

### II. Minimal External Dependencies

External libraries MUST only be introduced when the benefit clearly outweighs
the maintenance and security risk of the dependency.

- Before adding a dependency, the implementer MUST verify: (a) the platform or
  framework does not already provide equivalent functionality, and (b) the
  dependency is actively maintained with a credible security track record.
- Every new dependency MUST be documented in the feature plan with explicit
  justification.
- Prefer Umbraco-native APIs, .NET BCL, and the existing Umbraco package
  ecosystem over third-party alternatives.

**Rationale**: Each external dependency is a potential attack surface and a
future maintenance burden. Keeping the dependency footprint small aligns with
security-first thinking.

### III. Test-First Development

All features MUST have acceptance tests derived from user stories before
implementation begins. Unit tests MUST cover all non-trivial logic.

- Acceptance criteria in spec.md are the authoritative definition of "done".
- Tests MUST fail before the implementation is written (Red-Green-Refactor).
- Integration tests MUST use real Umbraco/database contexts, not mocks, for
  any code that touches persistence or the Umbraco service layer.
- Test coverage for security-sensitive code paths (authentication, authorisation,
  audit logging) MUST be explicitly reviewed in each PR.

**Rationale**: Security bugs are often regressions. A failing test suite is the
primary defence against inadvertently re-introducing a vulnerability.

### IV. Umbraco UX Consistency

All user-facing UI MUST conform to Umbraco's established design language,
component library, and interaction patterns so that the dashboard feels native
to the back-office.

- Use Umbraco's existing UI components (Lit/Angular back-office components,
  block editors, notification service, etc.) before building custom equivalents.
- Typography, spacing, iconography, and colour MUST follow the Umbraco design
  system.
- Navigation, error states, loading states, and empty states MUST match
  Umbraco back-office conventions.
- Accessibility (WCAG 2.1 AA) MUST be maintained for all new UI.

**Rationale**: A dashboard that feels foreign increases cognitive load and
reduces trust. Consistency with the host platform is a usability requirement,
not a preference.

### V. Security by Design

Security considerations MUST be addressed at design time, not as an afterthought.

- All inputs MUST be validated and sanitised at the boundary.
- Authorisation checks MUST be enforced server-side; client-side checks are
  supplemental only.
- Sensitive data (tokens, secrets, PII) MUST NOT be logged or exposed in API
  responses beyond what is operationally necessary.
- Threat modelling MUST be performed for any feature that modifies permissions,
  audit trails, or external integrations.
- Security findings MUST be treated as P1 bugs regardless of the phase they are
  discovered in.

**Rationale**: This is a security product. Any vulnerability in the dashboard
itself undermines confidence in the security posture it is meant to report on.

## Technology Constraints

- **Platform**: Umbraco CMS (latest LTS). All server-side code MUST target the
  .NET version bundled with the target Umbraco LTS release.
- **Language**: C# for back-end; TypeScript, Lit, and Vite for any front-end customisation
  beyond standard Umbraco back-office components.
- **Package management**: NuGet for server-side; npm/pnpm for front-end assets.
  Lock files MUST be committed.
- **Database**: Umbraco's built-in database abstraction (NPoco/EF as provided by
  Umbraco). Direct SQL MUST be avoided unless no abstraction exists; raw SQL
  MUST use parameterised queries exclusively.

## Development Workflow

- All work MUST be feature-branched from `main` and merged via pull request.
- PRs MUST pass all automated tests and a Constitution Check before review.
- Each PR description MUST reference the relevant spec and list the Constitution
  principles verified.
- Breaking changes to existing API contracts MUST be flagged in the PR and
  communicated to dependents before merge.
- The `main` branch MUST always be in a deployable state.

## Governance

This constitution supersedes all other project-level coding conventions. Where
a conflict exists between this document and an external standard, this
constitution takes precedence unless the external standard is a legal or
regulatory requirement.

**Amendment procedure**: Any principle change MUST be proposed via a PR that
updates this file, increments the version, and records the rationale. The PR
requires at least one reviewer approval. Amendments that remove or redefine
existing principles are MAJOR version bumps; new principles or material
expansions are MINOR; clarifications are PATCH.

**Compliance review**: Constitution compliance MUST be checked at PR review time
using the Constitution Check section of each feature's `plan.md`. Non-compliant
code MUST NOT be merged without an explicit documented exception recorded in
`plan.md` under Complexity Tracking.

**Version**: 1.0.0 | **Ratified**: 2026-03-29 | **Last Amended**: 2026-03-29
