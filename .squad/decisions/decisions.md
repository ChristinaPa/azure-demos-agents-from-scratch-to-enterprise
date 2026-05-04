# Decisions Log

## Decision: ASE.EnterpriseApi Integration Tests approach

**Date:** 2025-07-17  
**Author:** Morpheus  
**Status:** Accepted

### Context

`ASE.EnterpriseApi.Tests` existed with only options-validation unit tests. The project needed real HTTP-level integration tests covering the `/basic/*` and `/health` endpoints.

### Decisions

#### 1. WebApplicationFactory is the primary integration test mechanism

`Program` already declares `public partial class Program { }`, making `WebApplicationFactory<Program>` usable without any changes to the API project. A custom `ApiWebApplicationFactory` class overrides `ConfigureWebHost` to inject in-memory configuration, satisfying the `ValidateOnStart()` checks for both `CorsOptions` and `SearchOptions`.

**Rejected alternative:** Spinning up a real server with `dotnet run` in tests — fragile, slow, and port-sensitive.

#### 2. TestContainers added but gated as `[Trait("Category", "Integration")]`

Since the API has no external dependencies (no DB, no Redis), TestContainers adds limited value for fast feedback. However it provides an end-to-end smoke test of the Docker image itself. The test is excluded from the default `run-tests.ps1` run using `--filter "Category!=Integration"`. Running it requires Docker Desktop.

**Testcontainers 4.x API note:** `ImageFromDockerfileBuilder` must be used to build from a Dockerfile — the `ContainerBuilder` class does NOT expose `WithDockerfileDirectory`. After calling `await image.CreateAsync()`, the image tag is passed to `ContainerBuilder.WithImage(image)`.

#### 3. `run-tests.ps1` runs API tests by default, Docker tests opt-in

The script now runs both `ASE.Libraries.Tests` and `ASE.EnterpriseApi.Tests` (excluding `Category=Integration`) in the `-not $SkipDotnet` block. A new `-SkipApiTests` switch lets callers opt out of the API tests (e.g., in environments without the API's config).

### Consequences

- 20 tests total in `ASE.EnterpriseApi.Tests` (14 new + 6 pre-existing), all green.
- `run-tests.ps1` covers both .NET test projects without requiring Docker.
- TestContainers test validates the Docker build end-to-end when Docker is available.

---

## Decision: Documentation Audit — Test Count Sync

**Date:** 2025-07-16  
**Author:** Neo (Lead/Architect)  
**Status:** Applied

### Context

Documentation had drifted from the actual codebase state. Specific issues found:

1. `README.md` badge showed 51 tests; actual total is 65 (51 ASE.Libraries.Tests + 14 ASE.EnterpriseApi.Tests).
2. `docs/getting-started.md` "Running Tests" expected output showed stale count of 17.
3. `docs/getting-started.md` "Next Steps" linked to `examples.md` and `api-reference.md`, neither of which exists.
4. `docs/README.md` "Contributing" section linked to `../CONTRIBUTING.md`, which does not exist at repo root.
5. `docs/test-summary.md` showed 51 total and had no coverage of `ASE.EnterpriseApi.Tests`.
6. `docs/testing.md` was already correct (65 .NET, 22 Playwright) — no changes needed.

### Decisions Made

#### Playwright test count: 22 (not 12)

`decisions.md` contained an entry saying "12 E2E tests". `docs/testing.md` said 22. Grep of `src/chat-web-app/tests/search.spec.ts` confirmed **22 `test(` calls**. The decisions.md entry is stale. `testing.md` is authoritative.

#### Broken links: remove, not stub

`examples.md` and `api-reference.md` do not exist and are not planned in the immediate roadmap. Removing broken links is preferable to leaving them for readers to discover. Same decision applied to `CONTRIBUTING.md`.

### Changes Applied

| File | Change |
|------|--------|
| `README.md` | Badge: `51%20passed` → `65%20passed` |
| `docs/getting-started.md` | Expected output: 17 → 51; removed 2 broken Next Steps links |
| `docs/README.md` | Contributing: removed broken `CONTRIBUTING.md` link |
| `docs/test-summary.md` | Overall totals 51 → 65; added `ASE.EnterpriseApi.Tests` section (CorsOptionsTests 6, SearchOptionsTests 8); updated Test Categories table |
| `docs/testing.md` | No change (already correct) |
