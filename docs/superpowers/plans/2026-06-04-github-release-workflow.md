# GitHub Release Workflow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a manually-triggered GitHub Actions workflow that builds, signs with SSL.com eSignerCKA, and publishes the NuGet package to NuGet.org and GitHub Packages, then creates a GitHub Release with the signed package attached.

**Architecture:** A single `workflow_dispatch` workflow on `windows-latest`. The runner installs Node 22, builds the Vite frontend, packs the .NET project with the caller-supplied version, signs the `.nupkg` via `sslcom/esigner-codesign`, pushes to NuGet.org and GitHub Packages, and creates a tagged GitHub Release.

**Tech Stack:** GitHub Actions, .NET 10 (`dotnet pack`, `dotnet nuget push`), Node.js 22 + Vite, `sslcom/esigner-codesign@v3`, `gh` CLI

---

### Task 1: Create the workflow file

**Files:**
- Create: `.github/workflows/release.yml`

- [ ] **Step 1: Create the directory and file**

```bash
mkdir -p .github/workflows
```

Then create `.github/workflows/release.yml` with this exact content:

```yaml
name: Release

on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Package version (e.g. 1.0.1)'
        required: true
        type: string
      prerelease:
        description: 'Mark as pre-release'
        required: false
        type: boolean
        default: false

permissions:
  contents: write
  packages: write

jobs:
  release:
    runs-on: windows-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22'

      - name: Install frontend dependencies
        working-directory: client
        run: npm ci

      - name: Build frontend
        working-directory: client
        run: npm run build

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Pack NuGet package
        run: |
          dotnet pack src/Umbraco.SecurityDashboard/Umbraco.SecurityDashboard.csproj `
            -c Release `
            /p:Version=${{ inputs.version }} `
            -o artifacts/

      - name: Sign with SSL.com eSignerCKA
        uses: sslcom/esigner-codesign@v3
        with:
          command: sign
          username: ${{ secrets.SSL_COM_USERNAME }}
          password: ${{ secrets.SSL_COM_PASSWORD }}
          credential_id: ${{ secrets.SSL_COM_CREDENTIAL_ID }}
          totp_secret: ${{ secrets.SSL_COM_TOTP_SECRET }}
          file_path: artifacts/BlendInteractive.Umbraco.SecurityDashboard.${{ inputs.version }}.nupkg
          output_path: artifacts/signed/
          malware_block: 'false'

      - name: Publish to NuGet.org
        run: |
          dotnet nuget push "artifacts/signed/*.nupkg" `
            --api-key ${{ secrets.NUGET_API_KEY }} `
            --source https://api.nuget.org/v3/index.json `
            --skip-duplicate

      - name: Publish to GitHub Packages
        run: |
          dotnet nuget push "artifacts/signed/*.nupkg" `
            --api-key ${{ secrets.GITHUB_TOKEN }} `
            --source https://nuget.pkg.github.com/blendinteractive/index.json `
            --skip-duplicate

      - name: Create GitHub Release
        env:
          GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        shell: pwsh
        run: |
          $releaseArgs = @(
            "v${{ inputs.version }}",
            "artifacts/signed/BlendInteractive.Umbraco.SecurityDashboard.${{ inputs.version }}.nupkg",
            "--title", "v${{ inputs.version }}",
            "--generate-notes"
          )
          if ('${{ inputs.prerelease }}' -eq 'true') { $releaseArgs += '--prerelease' }
          gh release create @releaseArgs
```

- [ ] **Step 2: Commit**

```bash
git add .github/workflows/release.yml
git commit -m "Add GitHub Actions release workflow"
```

---

### Task 2: Add the required secrets in GitHub

These must be added before the workflow can run. Navigate to:
`https://github.com/blendinteractive/umbraco-security-dashboard/settings/secrets/actions`

- [ ] **Step 1: Add SSL_COM_USERNAME**
  - Name: `SSL_COM_USERNAME`
  - Value: Your SSL.com account email address

- [ ] **Step 2: Add SSL_COM_PASSWORD**
  - Name: `SSL_COM_PASSWORD`
  - Value: Your SSL.com account password

- [ ] **Step 3: Add SSL_COM_CREDENTIAL_ID**
  - Name: `SSL_COM_CREDENTIAL_ID`
  - Value: Found in SSL.com portal → your certificate → "Credential ID"

- [ ] **Step 4: Add SSL_COM_TOTP_SECRET**
  - Name: `SSL_COM_TOTP_SECRET`
  - Value: Found in SSL.com portal → your certificate → "TOTP Secret" (the static base32 seed, e.g. `JBSWY3DPEHPK3PXP` — not the rotating 6-digit code)

- [ ] **Step 5: Add NUGET_API_KEY**
  - Name: `NUGET_API_KEY`
  - Value: From nuget.org → your account (top right) → API Keys → Create or copy an existing key scoped to `BlendInteractive.Umbraco.SecurityDashboard`

---

### Task 3: Verify the workflow runs

- [ ] **Step 1: Push the branch / ensure main is up to date**

```bash
git push
```

- [ ] **Step 2: Trigger a test run**

Navigate to:
`https://github.com/blendinteractive/umbraco-security-dashboard/actions/workflows/release.yml`

Click **Run workflow**, enter a version (e.g. `1.0.1`), leave prerelease unchecked, and click **Run workflow**.

- [ ] **Step 3: Verify each job step passes**

Watch the live log. Key things to confirm:
- "Build frontend" step completes without errors
- "Pack NuGet package" step produces `BlendInteractive.Umbraco.SecurityDashboard.1.0.1.nupkg`
- "Sign with SSL.com eSignerCKA" step authenticates and produces a signed `.nupkg` in `artifacts/signed/`
- "Publish to NuGet.org" step shows `Your package was pushed` (or `already exists` if re-running)
- "Create GitHub Release" step creates the release at `https://github.com/blendinteractive/umbraco-security-dashboard/releases`

- [ ] **Step 4: Confirm the GitHub Release exists**

Navigate to:
`https://github.com/blendinteractive/umbraco-security-dashboard/releases`

Verify `v1.0.1` is listed with the `.nupkg` attached.

- [ ] **Step 5: Confirm the NuGet.org package is live**

Navigate to:
`https://www.nuget.org/packages/BlendInteractive.Umbraco.SecurityDashboard`

Note: NuGet.org indexing can take up to 30 minutes.
