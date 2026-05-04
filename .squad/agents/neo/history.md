# Neo — History

## Project Context

- **Project:** azure-demos-agents-from-scratch-to-enterprise
- **Stack:** Vue 3 + TypeScript + Vite + Vue Router 4 + Tailwind CSS
- **User:** Bojan Vrhovnik
- **Goal:** Vue.js search UI calling `GET {baseUrl}/basic/search?query={query}` which returns `[{sourceName, sourceLink, text}]`
- **API Base URL:** `VITE_API_BASE_URL` env var (default: https://localhost:5066)
- **Source location:** `src/vue-app/`

## Learnings

### 2025-07-16: Created scripts/ directory with three operational scripts
**What:**
- `scripts/run-all.ps1` — launches backend (.NET 10) and frontend (Vite) in separate PowerShell windows
- `scripts/build-acr.ps1` — builds both Docker images via `az acr build` (cloud build, no local Docker needed); accepts `-AcrName`, `-ResourceGroup`, `-BackendTag`, `-FrontendTag`, `-ViteApiBaseUrl`, `-NoPush`
- `scripts/run-tests.ps1` — runs .NET xunit tests (`tests/ASE.Libraries.Tests`) and Playwright E2E tests (`src/chat-web-app`); supports `-SkipDotnet` / `-SkipPlaywright`

**Key decisions:**
- ACR build uses cloud-side build context; backend context is `src/AgentScratchEnterprise/` with Dockerfile at `ASE.EnterpriseApi/Dockerfile`
- Frontend image receives `VITE_API_BASE_URL` as a build-arg so the API URL is baked in at image build time
- `run-all.ps1` delegates to `run-backend.ps1` and `run-frontend.ps1` (sibling scripts, expected to be created by other agents)

### 2025-07-16: Documentation audit — synced docs to actual test counts

**What:**
- Updated `README.md` badge: 51 → 65 total tests
- Updated `docs/getting-started.md`: expected output block 17 → 51; removed two broken "Next Steps" links (`examples.md`, `api-reference.md` — files don't exist)
- Updated `docs/README.md`: removed broken `../CONTRIBUTING.md` link (file doesn't exist at repo root)
- Updated `docs/test-summary.md`: overall totals 51 → 65; added full section for `ASE.EnterpriseApi.Tests` (14 tests: CorsOptionsTests=6, SearchOptionsTests=8); updated Test Categories table
- `docs/testing.md`: no change needed — already correctly states 65 .NET tests and 22 Playwright tests; grep confirmed 22 `test(` calls in `search.spec.ts` (decisions.md stating "12" is stale)

**Key decisions:**
- Playwright test count: trust `search.spec.ts` (22 matches) over stale decisions.md entry (12)
- Broken doc links removed rather than stubbed to avoid misleading readers
