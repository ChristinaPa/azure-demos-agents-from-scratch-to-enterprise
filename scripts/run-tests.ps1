<#
.SYNOPSIS
    Runs all tests: .NET unit tests and Playwright E2E tests.
.DESCRIPTION
    Runs ASE.Libraries.Tests and ASE.EnterpriseApi.Tests (.NET/xunit) and Playwright E2E tests for the Vue.js app.
    
.PARAMETER SkipDotnet
    Skip .NET unit tests.
    
.PARAMETER SkipPlaywright
    Skip Playwright E2E tests.

.PARAMETER SkipApiTests
    Skip ASE.EnterpriseApi.Tests (useful when Docker is not available to skip Integration-category tests).
#>

param(
    [switch]$SkipDotnet,
    [switch]$SkipPlaywright,
    [switch]$SkipApiTests
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$exitCode = 0

Write-Host "🧪 Running test suite" -ForegroundColor Cyan
Write-Host ""

if (-not $SkipDotnet) {
    Write-Host "--- .NET Unit Tests (ASE.Libraries.Tests) ---" -ForegroundColor Yellow
    $testProject = Join-Path $repoRoot "tests\ASE.Libraries.Tests\ASE.Libraries.Tests.csproj"
    dotnet test $testProject --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ .NET tests FAILED" -ForegroundColor Red
        $exitCode = 1
    } else {
        Write-Host "✅ .NET tests PASSED" -ForegroundColor Green
    }
    Write-Host ""

    if (-not $SkipApiTests) {
        Write-Host "--- .NET API Integration Tests (ASE.EnterpriseApi.Tests) ---" -ForegroundColor Yellow
        $apiTestProject = Join-Path $repoRoot "tests\ASE.EnterpriseApi.Tests\ASE.EnterpriseApi.Tests.csproj"
        # Exclude Docker-dependent tests unless Docker is explicitly available
        dotnet test $apiTestProject --verbosity normal --filter "Category!=Integration"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ API integration tests FAILED" -ForegroundColor Red
            $exitCode = 1
        } else {
            Write-Host "✅ API integration tests PASSED" -ForegroundColor Green
        }
        Write-Host ""
    }
}

if (-not $SkipPlaywright) {
    Write-Host "--- Playwright E2E Tests ---" -ForegroundColor Yellow
    $webAppDir = Join-Path $repoRoot "src\chat-web-app"
    Push-Location $webAppDir
    try {
        if (-not (Test-Path "node_modules")) {
            Write-Host "📦 Installing dependencies..." -ForegroundColor Yellow
            npm install
        }
        npx playwright test
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Playwright tests FAILED" -ForegroundColor Red
            $exitCode = 1
        } else {
            Write-Host "✅ Playwright tests PASSED" -ForegroundColor Green
        }
    } finally {
        Pop-Location
    }
    Write-Host ""
}

if ($exitCode -eq 0) {
    Write-Host "🎉 All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed." -ForegroundColor Red
}

exit $exitCode
