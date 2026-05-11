# Quickstart: Security Audit Log (007)

## What This Feature Does

Adds an immutable, append-only audit log to the Security Dashboard. Every overall vulnerability state change (automatic scan or manual mitigation action) produces a timestamped entry. Manual actions are always logged regardless of whether they change the overall state. The back-office exposes a paginated history view in the Security Dashboard section.

## Prerequisites

- Features 001–006 implemented and passing
- Umbraco 17 running locally (`demo/` project)
- `client/` pnpm build working (`pnpm --prefix client build`)

## Running the Demo

```bash
# From repo root
cd demo
dotnet run
```

The migration `SecurityDashboard-1.2.0` runs automatically on startup and creates the `SecurityDashboard_AuditLog` table.

## Seeding Test Audit Entries (Development Only)

There is no seeder script. Use the existing demo flow to generate entries:

1. Open the Umbraco back-office and navigate to the Security Dashboard section.
2. Trigger a vulnerability check via the existing "Run Check" mechanism — if the state changes, an entry appears.
3. Mark a vulnerability as mitigated (POST to `/advisories/{ghsaId}/mitigations`) — an entry is always written.
4. Remove the mitigation (DELETE) — another entry is written.
5. Refresh the audit history view to see entries in reverse-chronological order.

## Verifying the API Manually

```bash
# Get a token first (replace with your test user credentials)
TOKEN=$(curl -s -X POST http://localhost:44339/umbraco/management/api/v1/security/user-login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin1234"}' | jq -r '.tokenResponse.accessToken')

# Fetch first page of audit log
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:44339/umbraco/management/api/v1/security-dashboard/audit-log?skip=0&take=25"
```

## Frontend Development

```bash
pnpm --prefix client dev
```

The audit log section appears below the advisory list in the main Security Dashboard element.

## Key Files

| File | Role |
|------|------|
| `src/.../Migrations/AddAuditLogTable.cs` | Creates `SecurityDashboard_AuditLog` table and index |
| `src/.../Models/Db/AuditLogRecord.cs` | NPoco POCO |
| `src/.../Services/IAuditLogRepository.cs` | Repository interface |
| `src/.../Services/AuditLogRepository.cs` | NPoco implementation |
| `src/.../Controllers/SecurityDashboardController.cs` | `GET audit-log` endpoint; audit + webhook on mitigation changes |
| `src/.../Services/VulnerabilityService.cs` | `GetCurrentOverallStatusAsync()`; audit + conditional webhook on scan |
| `client/src/components/audit-log.element.ts` | Paginated Lit UI component |
