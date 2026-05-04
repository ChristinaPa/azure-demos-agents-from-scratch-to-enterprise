<#
.SYNOPSIS
    Pester tests for scripts/run-all.ps1
#>

BeforeAll {
    $script:scriptPath = Join-Path $PSScriptRoot "..\..\scripts\run-all.ps1"
    $script:scriptContent = Get-Content $script:scriptPath -Raw
}

Describe "run-all.ps1" {

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
    }

    Context "Process launching" {
        It "launches the backend script" {
            $script:scriptContent | Should -Match 'run-backend'
        }

        It "launches the frontend script" {
            $script:scriptContent | Should -Match 'run-frontend'
        }

        It "uses Start-Process to launch scripts in separate windows" {
            $script:scriptContent | Should -Match 'Start-Process'
        }

        It "passes -NoExit so windows stay open after the script runs" {
            $script:scriptContent | Should -Match '-NoExit'
        }

        It "uses powershell as the process to start" {
            $script:scriptContent | Should -Match 'Start-Process powershell'
        }
    }

    Context "Timing and sequencing" {
        It "pauses between starting backend and frontend" {
            $script:scriptContent | Should -Match 'Start-Sleep'
        }
    }

    Context "Port references" {
        It "references backend port 5066" {
            $script:scriptContent | Should -Match '5066'
        }

        It "references frontend port 5173" {
            $script:scriptContent | Should -Match '5173'
        }
    }

    Context "Path construction" {
        It "computes scriptDir from MyInvocation.MyCommand.Path" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$MyInvocation\.MyCommand\.Path'
        }

        It "builds backend script path from scriptDir" {
            $script:scriptContent | Should -Match 'Join-Path \$scriptDir'
        }
    }
}
