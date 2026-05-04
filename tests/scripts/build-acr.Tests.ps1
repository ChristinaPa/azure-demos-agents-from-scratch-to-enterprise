<#
.SYNOPSIS
    Pester tests for scripts/build-acr.ps1
#>

BeforeAll {
    $script:scriptPath = Join-Path $PSScriptRoot "..\..\scripts\build-acr.ps1"
    $script:scriptContent = Get-Content $script:scriptPath -Raw

    $errors = $null
    $script:ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:scriptPath, [ref]$null, [ref]$errors
    )
}

Describe "build-acr.ps1" {

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
        It "has an AcrName parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("AcrName") | Should -Be $true
        }

        It "AcrName is mandatory" {
            $cmd = Get-Command $script:scriptPath
            $mandatory = $cmd.Parameters["AcrName"].Attributes |
                Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } |
                Select-Object -ExpandProperty Mandatory
            $mandatory | Should -Contain $true
        }

        It "has a ResourceGroup parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("ResourceGroup") | Should -Be $true
        }

        It "ResourceGroup is optional" {
            $cmd = Get-Command $script:scriptPath
            $mandatory = $cmd.Parameters["ResourceGroup"].Attributes |
                Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } |
                Select-Object -ExpandProperty Mandatory
            $mandatory | Should -Not -Contain $true
        }

        It "has a BackendImage parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("BackendImage") | Should -Be $true
        }

        It "has a FrontendImage parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("FrontendImage") | Should -Be $true
        }

        It "has a NoPush switch parameter" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters.ContainsKey("NoPush") | Should -Be $true
        }

        It "NoPush is a [switch] type" {
            $cmd = Get-Command $script:scriptPath
            $cmd.Parameters["NoPush"].ParameterType | Should -Be ([System.Management.Automation.SwitchParameter])
        }
    }

    Context "Default values" {
        It "BackendImage defaults to 'ase-enterprise-api'" {
            $script:scriptContent | Should -Match 'BackendImage\s*=\s*"ase-enterprise-api"'
        }

        It "FrontendImage defaults to 'vue-search-app'" {
            $script:scriptContent | Should -Match 'FrontendImage\s*=\s*"vue-search-app"'
        }

        It "ResourceGroup defaults to empty string" {
            $script:scriptContent | Should -Match 'ResourceGroup\s*=\s*""'
        }
    }

    Context "Tag appending logic" {
        It "appends :latest to BackendImage when no tag is present" {
            $script:scriptContent | Should -Match 'BackendImage.*:.*latest'
        }

        It "appends :latest to FrontendImage when no tag is present" {
            $script:scriptContent | Should -Match 'FrontendImage.*:.*latest'
        }

        It "checks for existing tag using -notlike '*:*'" {
            $script:scriptContent | Should -Match '-notlike\s+[''"]?\*:\*[''"]?'
        }
    }

    Context "Error handling" {
        It "sets ErrorActionPreference to Stop" {
            $script:scriptContent | Should -Match '\$ErrorActionPreference\s*=\s*"Stop"'
        }

        It "verifies az CLI is available before proceeding" {
            $script:scriptContent | Should -Match 'Get-Command\s+"az"'
        }

        It "exits with code 1 when backend build fails" {
            $script:scriptContent | Should -Match 'exit 1'
        }
    }

    Context "Build commands" {
        It "uses az acr build for backend" {
            $script:scriptContent | Should -Match '"acr",\s*"build"'
        }

        It "passes --registry flag" {
            $script:scriptContent | Should -Match '"--registry"'
        }

        It "passes --image flag" {
            $script:scriptContent | Should -Match '"--image"'
        }

        It "references the backend Dockerfile" {
            $script:scriptContent | Should -Match 'ASE\.EnterpriseApi\\Dockerfile'
        }

        It "references the frontend Dockerfile via Join-Path" {
            # The path is built dynamically: Join-Path $frontendContext "Dockerfile"
            $script:scriptContent | Should -Match 'Join-Path \$frontendContext "Dockerfile"'
        }

        It "supports optional --resource-group flag" {
            $script:scriptContent | Should -Match '"--resource-group"'
        }

        It "supports --no-push flag when NoPush is set" {
            $script:scriptContent | Should -Match '"--no-push"'
        }
    }

    Context "Path construction" {
        It "computes repoRoot from script location" {
            $script:scriptContent | Should -Match 'Split-Path -Parent \$scriptDir'
        }

        It "backend context is in src\AgentScratchEnterprise" {
            $script:scriptContent | Should -Match 'AgentScratchEnterprise'
        }

        It "frontend context is in src\chat-web-app" {
            $script:scriptContent | Should -Match 'chat-web-app'
        }
    }
}
