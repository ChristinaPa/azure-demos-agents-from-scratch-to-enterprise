<#
.SYNOPSIS
    Pester tests for scripts/run-tests.ps1
#>

BeforeAll {
    $script:scriptPath = Join-Path $PSScriptRoot "..\..\scripts\run-tests.ps1"
    $script:scriptContent = Get-Content $script:scriptPath -Raw
}

Describe "run-tests.ps1" {

    Context "File existence and syntax" {
        It "exists on disk" {
            Test-Path $script:scriptPath | Should -Be $true
        }

        It "is syntactically valid PowerShell" {
            $errors = $null
            [System.Management.Automation.Language.Parser]::ParseFile(
                $script:scriptPath, [ref]$null, [ref]$errors
            ) | Out-Null
            $errors.Count | Should -Be 0
        }
    }

    Context "Parameters" {
        It "has a SkipDotnet switch parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("SkipDotnet") | Should -Be $true
        }

        It "has a SkipPlaywright switch parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("SkipPlaywright") | Should -Be $true
        }

        It "SkipDotnet is a [switch] type" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters["SkipDotnet"].ParameterType | Should -Be ([System.Management.Automation.SwitchParameter])
        }

        It "SkipPlaywright is a [switch] type" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters["SkipPlaywright"].ParameterType | Should -Be ([System.Management.Automation.SwitchParameter])
        }
    }

    Context "Error handling" {
        It "sets ErrorActionPreference to Stop" {
            $script:scriptContent | Should -Match '\$ErrorActionPreference\s*=\s*"Stop"'
        }
    }

    Context "Path construction" {
        It "computes scriptDir from MyInvocation.MyCommand.Path" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$MyInvocation\.MyCommand\.Path'
        }

        It "computes repoRoot as parent of scriptDir" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$scriptDir'
        }

        It "references the correct test project path" {
            $script:scriptContent | Should -Match 'ASE\.Libraries\.Tests\\ASE\.Libraries\.Tests\.csproj'
        }

        It "references the chat-web-app directory for Playwright" {
            $script:scriptContent | Should -Match 'chat-web-app'
        }
    }

    Context "Execution guards" {
        It "guards dotnet test behind -not SkipDotnet check" {
            $script:scriptContent | Should -Match '-not \$SkipDotnet'
        }

        It "guards npx playwright test behind -not SkipPlaywright check" {
            $script:scriptContent | Should -Match '-not \$SkipPlaywright'
        }

        It "calls dotnet test when SkipDotnet is not set" {
            $script:scriptContent | Should -Match 'dotnet test'
        }

        It "calls npx playwright test when SkipPlaywright is not set" {
            $script:scriptContent | Should -Match 'npx playwright test'
        }
    }

    Context "Exit code handling" {
        It "exits with 0 on success" {
            $script:scriptContent | Should -Match 'exit \$exitCode'
        }

        It "sets exitCode to 1 on failure" {
            $script:scriptContent | Should -Match '\$exitCode\s*=\s*1'
        }

        It "checks LASTEXITCODE after dotnet test" {
            $script:scriptContent | Should -Match '\$LASTEXITCODE'
        }
    }
}
