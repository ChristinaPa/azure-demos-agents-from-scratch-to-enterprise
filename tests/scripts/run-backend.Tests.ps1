<#
.SYNOPSIS
    Pester tests for scripts/run-backend.ps1
#>

BeforeAll {
    $script:scriptPath = Join-Path $PSScriptRoot "..\..\scripts\run-backend.ps1"
    $script:scriptContent = Get-Content $script:scriptPath -Raw
}

Describe "run-backend.ps1" {

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

    Context "Error handling" {
        It "sets ErrorActionPreference to Stop" {
            $script:scriptContent | Should -Match '\$ErrorActionPreference\s*=\s*"Stop"'
        }

        It "wraps dotnet run in try/finally for clean shutdown" {
            $script:scriptContent | Should -Match 'try\s*\{'
            $script:scriptContent | Should -Match 'finally\s*\{'
        }
    }

    Context "Environment configuration" {
        It "sets ASPNETCORE_ENVIRONMENT to Development" {
            $script:scriptContent | Should -Match 'ASPNETCORE_ENVIRONMENT.*=.*"Development"'
        }
    }

    Context "Project reference" {
        It "references the correct .csproj file" {
            $script:scriptContent | Should -Match 'ASE\.EnterpriseApi\.csproj'
        }

        It "references the ASE.EnterpriseApi project path" {
            $script:scriptContent | Should -Match 'AgentScratchEnterprise.*ASE\.EnterpriseApi'
        }

        It "runs dotnet run with --project flag" {
            $script:scriptContent | Should -Match 'dotnet run --project'
        }
    }

    Context "Port configuration" {
        It "binds to port 5066" {
            $script:scriptContent | Should -Match '5066'
        }

        It "uses --urls flag to configure the listening address" {
            $script:scriptContent | Should -Match '--urls'
        }

        It "sets the URL to http://localhost:5066" {
            $script:scriptContent | Should -Match 'http://localhost:5066'
        }
    }

    Context "Path construction" {
        It "computes scriptDir from MyInvocation.MyCommand.Path" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$MyInvocation\.MyCommand\.Path'
        }

        It "computes repoRoot as parent of scriptDir" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$scriptDir'
        }

        It "constructs apiProject path using Join-Path" {
            $script:scriptContent | Should -Match 'Join-Path'
        }
    }
}
