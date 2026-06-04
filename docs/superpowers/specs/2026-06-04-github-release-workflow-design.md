# GitHub Release Workflow Design

**Date:** 2026-06-04
**Scope:** Add a GitHub Actions workflow that builds, signs, and publishes the NuGet package.

---

## Overview

A single manually-triggered GitHub Actions workflow that:
1. Builds the Vite frontend and packs the NuGet package from source
2. Signs the `.nupkg` using SSL.com eSignerCKA
3. Publishes the signed package to NuGet.org and GitHub Packages
4. Creates a GitHub Release with the signed `.nupkg` attached

---

## Trigger

`workflow_dispatch` (manual trigger via GitHub UI) with two inputs:

| Input | Type | Required | Default | Description |
|---|---|---|---|---|
| `version` | string | yes | — | Version to stamp on the package, e.g. `1.0.1` |
| `prerelease` | boolean | no | `false` | Mark the GitHub Release as a pre-release |

---

## Runner

`windows-latest` — required for eSignerCKA (Windows CNG integration).

---

## Job Steps

### 1. Checkout
Standard shallow clone (`fetch-depth: 1`). `gh release create` creates the tag at HEAD, so full history is not needed.

### 2. Setup Node.js
Node 22, with `npm ci` run in the `client/` directory to install frontend dependencies.

### 3. Build frontend
`npm run build` in `client/` — outputs to `src/Umbraco.SecurityDashboard/wwwroot/App_Plugins/SecurityDashboard/dist/`.

### 4. Setup .NET 10
Uses `actions/setup-dotnet` with `dotnet-version: '10.x'`.

### 5. Pack NuGet package
```
dotnet pack src/Umbraco.SecurityDashboard/Umbraco.SecurityDashboard.csproj \
  -c Release \
  /p:Version=${{ inputs.version }} \
  -o artifacts/
```
Produces `artifacts/BlendInteractive.Umbraco.SecurityDashboard.${{ inputs.version }}.nupkg`.

### 6. Sign with SSL.com eSignerCKA
Uses `sslcom/esigner-codesign` action. Reads four secrets (see below). Writes the signed package to `artifacts/signed/`.

### 7. Publish to NuGet.org
```
dotnet nuget push artifacts/signed/*.nupkg \
  --api-key ${{ secrets.NUGET_API_KEY }} \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
```

### 8. Publish to GitHub Packages
```
dotnet nuget push artifacts/signed/*.nupkg \
  --api-key ${{ secrets.GITHUB_TOKEN }} \
  --source https://nuget.pkg.github.com/blendinteractive/index.json \
  --skip-duplicate
```

### 9. Create GitHub Release
Uses `gh release create` to tag `v${{ inputs.version }}`, attach the signed `.nupkg`, and set the pre-release flag based on the `prerelease` input. The release is created as a draft if desired, but defaults to published.

---

## Secrets

Five secrets must be added to the repository (`Settings → Secrets and variables → Actions`):

| Secret name | Where to find it |
|---|---|
| `SSL_COM_USERNAME` | SSL.com account email |
| `SSL_COM_PASSWORD` | SSL.com account password |
| `SSL_COM_CREDENTIAL_ID` | SSL.com portal → certificate → Credential ID |
| `SSL_COM_TOTP_SECRET` | SSL.com portal → certificate → TOTP Secret (static base32 seed — not the rotating code) |
| `NUGET_API_KEY` | nuget.org → Account → API Keys |

`GITHUB_TOKEN` is provided automatically by GitHub Actions — no setup needed.

---

## File to create

`.github/workflows/release.yml`

---

## Out of scope

- CI on PRs / push to main (not part of this workflow)
- Automatic version bumping in the `.csproj`
- Changelog generation
