# Feature Specification: Webhook Notifications for Vulnerability Scan Results

**Feature Branch**: `004-webhook-notifications`  
**Created**: 2026-05-05  
**Status**: Draft  

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure Webhook Endpoint (Priority: P1)

An administrator wants to integrate the security dashboard's scan results with an external monitoring system, alerting service, or custom workflow. They add a webhook URL to the site's configuration file, and from that point forward every vulnerability scan automatically notifies the external endpoint.

**Why this priority**: Without the ability to configure the endpoint, no webhook can be sent. This is the foundational story.

**Independent Test**: An administrator can add a webhook URL to the appsettings file, restart the application, trigger a scan, and observe the external endpoint receives the correct payload — delivering full integration value on its own.

**Acceptance Scenarios**:

1. **Given** a webhook URL is present in the appsettings file, **When** the application starts, **Then** the system recognises the configured endpoint and uses it for all subsequent scans.
2. **Given** no webhook URL is configured, **When** a vulnerability scan runs, **Then** the scan completes normally and no outbound webhook request is attempted.
3. **Given** an invalid or malformed URL is configured, **When** the application starts, **Then** the system logs a clear configuration warning and disables webhook dispatch without crashing.

---

### User Story 2 - Receive Scan Results via Webhook (Priority: P1)

An external service (monitoring tool, Slack integration, incident-management platform, etc.) receives a structured notification each time a vulnerability scan completes, containing enough information to determine whether action is needed.

**Why this priority**: This is the core value delivery — external systems need scan outcomes in a machine-readable format.

**Independent Test**: With a webhook URL configured and pointing to a request-capture tool (e.g., a test endpoint), triggering a scan produces a POST request whose body contains the site URL, the exposure status, and the affected package list.

**Acceptance Scenarios**:

1. **Given** a webhook URL is configured and the site has no vulnerable packages, **When** a scan completes, **Then** a POST request is sent to the endpoint with status "Not Exposed" and an empty affected packages list.
2. **Given** a webhook URL is configured and the site has one or more vulnerable packages, **When** a scan completes, **Then** a POST request is sent to the endpoint with status "Exposed" and a list of each affected package name and its installed version.
3. **Given** a webhook URL is configured, **When** the POST request is sent, **Then** the payload includes the primary URL of the site.

---

### User Story 3 - Graceful Failure Handling (Priority: P2)

The external webhook endpoint is temporarily unavailable. The scan and the rest of the application continue to function normally; the failure is recorded so administrators can investigate.

**Why this priority**: Resilience is important but secondary to core functionality — the scan should never be blocked by a non-responsive external service.

**Independent Test**: Configuring an unreachable webhook URL and running a scan results in the scan completing with its normal outcome while an error entry appears in the application log.

**Acceptance Scenarios**:

1. **Given** a webhook URL is configured and the endpoint is unreachable, **When** a scan completes and the POST request fails, **Then** the scan result is still recorded normally and the application does not crash or stall.
2. **Given** a webhook request fails, **When** the failure occurs, **Then** the system writes a descriptive error entry to the application log (including the URL and nature of the failure).
3. **Given** a webhook request returns a non-success HTTP status, **When** that response is received, **Then** the system logs the status code and treats the delivery as failed without retrying.

---

### Edge Cases

- A slow webhook endpoint delays scan completion synchronously up to the configured timeout (default 10 seconds); after timeout the scan is recorded as complete and the dispatch is logged as failed.
- How does the system handle a webhook URL that resolves but returns a redirect?
- What if the site's primary URL cannot be determined from configuration?

## Clarifications

### Session 2026-05-05

- Q: Should the webhook request include a configurable shared secret or signature header so the receiving endpoint can authenticate the request? → A: Optional — a secret header is included only when the administrator configures one; omitted otherwise.
- Q: Should each affected package entry in the payload include anything beyond package name and installed version? → A: Yes — name, installed version, advisory reference (URL or CVE/GHSA identifier), and severity level.
- Q: Should the webhook POST be dispatched synchronously (blocking until response or timeout) or asynchronously (fire-and-forget after scan result is persisted)? → A: Synchronous — the POST completes or times out before the scan pipeline finishes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Administrators MUST be able to specify a webhook endpoint URL in the application's appsettings configuration file without modifying code.
- **FR-001a**: Administrators MAY optionally configure a shared secret value alongside the webhook URL; when configured, the system MUST include it as a request header (e.g., `X-Webhook-Secret`) on every outbound POST. When not configured, no secret header is sent.
- **FR-002**: The system MUST send a POST request to the configured webhook URL each time a vulnerability scan completes.
- **FR-003**: The POST payload MUST include the site's primary URL.
- **FR-004**: The POST payload MUST include the overall exposure status, expressed as either "Exposed" or "Not Exposed".
- **FR-005**: The POST payload MUST include a list of affected packages; each entry MUST contain the package name, the installed version, an advisory reference (URL or CVE/GHSA identifier), and a severity level. The list MUST be empty when no vulnerabilities are found.
- **FR-006**: The system MUST skip webhook dispatch entirely when no URL is configured, without producing errors or warnings during normal operation.
- **FR-007**: The system MUST complete the vulnerability scan and persist its results regardless of whether the webhook request succeeds or fails.
- **FR-008**: The system MUST log an error (including the endpoint URL and failure reason) when a webhook dispatch attempt fails.
- **FR-009**: The system MUST validate that a configured webhook URL is well-formed at startup and log a warning and disable dispatch if validation fails.
- **FR-010**: The webhook POST is dispatched synchronously — the system waits for the endpoint to respond (or time out) before the scan pipeline finishes. The request MUST time out after a configured period (defaulting to 10 seconds) and be treated as a failure if no response is received within that window.

### Key Entities

- **Webhook Configuration**: Represents the administrator-supplied settings for the outbound webhook — URL, an optional shared secret, and any global dispatch options (e.g., timeout).
- **Scan Notification Payload**: The data object sent to the webhook endpoint — site URL, exposure status, and the list of affected packages (each with name and installed version).
- **Affected Package**: A package found to have an applicable vulnerability — identified by name, installed version, advisory reference (URL or CVE/GHSA identifier), and severity level.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An administrator can configure the webhook endpoint by editing a single configuration file entry, with no code changes or redeployment required beyond an application restart.
- **SC-002**: The webhook notification is dispatched synchronously as part of the scan pipeline; under normal network conditions the POST completes within 5 seconds, after which the scan is considered finished. A slow or unresponsive endpoint delays scan completion only up to the configured timeout (default 10 seconds), at which point the scan is recorded as complete and the webhook attempt is logged as failed.
- **SC-003**: A scan completes and its results are persisted even when the webhook endpoint is unreachable or returns an error.
- **SC-004**: The POST payload received by the webhook endpoint contains all three required fields (site URL, status, affected packages) in a consistent, machine-readable format.
- **SC-005**: 100% of completed scans where a valid webhook URL is configured result in a dispatch attempt being logged (success or failure).

## Assumptions

- The existing vulnerability scan mechanism (built in prior features) is the trigger point; this feature adds a side-effect to that existing flow rather than introducing a new scan trigger.
- The site's primary URL is already available from the application's existing configuration (e.g., a base URL setting).
- The webhook payload is delivered as JSON over HTTPS/HTTP. The receiving endpoint's payload validation is its own responsibility; however, the system supports an optional shared secret header to allow receivers to authenticate the request origin.
- No retry logic is implemented beyond the initial attempt; failed deliveries are logged only.
- A single webhook endpoint per installation is supported; multiple endpoints are out of scope.
- The webhook timeout defaults to a reasonable value (e.g., 10 seconds) and is not user-configurable in this iteration.
