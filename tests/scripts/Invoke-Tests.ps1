<#
.SYNOPSIS
    Runs all Pester tests for the PowerShell scripts.
.DESCRIPTION
    Discovers and executes all *.Tests.ps1 files in the same directory.
    Exits with code 1 if any tests fail.
#>

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Import-Module Pester -MinimumVersion 5.0

$config = New-PesterConfiguration
$config.Run.Path = $scriptDir
$config.Output.Verbosity = "Detailed"

$result = Invoke-Pester -Configuration $config

if ($result.FailedCount -gt 0) {
    Write-Error "❌ $($result.FailedCount) Pester test(s) failed."
    exit 1
}

Write-Host "✅ All $($result.PassedCount) Pester tests passed." -ForegroundColor Green
