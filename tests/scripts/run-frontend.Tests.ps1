<#
.SYNOPSIS
    Pester tests for scripts/run-frontend.ps1
#>

BeforeAll {
    $script:scriptPath = Join-Path $PSScriptRoot "..\..\scripts\run-frontend.ps1"
    $script:scriptContent = Get-Content $script:scriptPath -Raw
}

Describe "run-frontend.ps1" {

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

    Context "Directory reference" {
        It "references the chat-web-app directory" {
            $script:scriptContent | Should -Match 'chat-web-app'
        }

        It "changes location to the web app directory" {
            $script:scriptContent | Should -Match 'Set-Location'
        }

        It "computes webAppDir using Join-Path" {
            $script:scriptContent | Should -Match 'Join-Path'
        }
    }

    Context "Dependency installation" {
        It "checks for node_modules before installing" {
            $script:scriptContent | Should -Match 'node_modules'
        }

        It "runs npm install when node_modules is missing" {
            $script:scriptContent | Should -Match 'npm install'
        }

        It "uses Test-Path to check node_modules existence" {
            $script:scriptContent | Should -Match 'Test-Path'
        }
    }

    Context "Dev server" {
        It "runs npm run dev to start the Vite dev server" {
            $script:scriptContent | Should -Match 'npm run dev'
        }

        It "references port 5173" {
            $script:scriptContent | Should -Match '5173'
        }
    }

    Context "Path construction" {
        It "computes scriptDir from MyInvocation.MyCommand.Path" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$MyInvocation\.MyCommand\.Path'
        }

        It "computes repoRoot as parent of scriptDir" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$scriptDir'
        }
    }
}
