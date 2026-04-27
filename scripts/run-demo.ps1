# Demo startup script - runs both backend and frontend
# Prerequisites: dotnet user-secrets must be configured for ASE.EnterpriseApi

Write-Host "Starting backend API..." -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Kill any existing dotnet processes on port 5000
$existing = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($existing) {
    $existing | Select-Object -ExpandProperty OwningProcess -Unique | ForEach-Object {
        Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Milliseconds 1000
}

# Start backend
Push-Location "$PSScriptRoot\..\src\AgentScratchEnterprise"
Start-Process -NoNewWindow powershell -ArgumentList "-Command", "cd '$PWD'; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --project ASE.EnterpriseApi"
Pop-Location

Start-Sleep -Seconds 5

# Start frontend
Write-Host "Starting frontend..." -ForegroundColor Cyan
Push-Location "$PSScriptRoot\..\src\chat-web-app"
Start-Process -NoNewWindow powershell -ArgumentList "-Command", "cd '$PWD'; npm run dev"
Pop-Location

Write-Host ""
Write-Host "Backend:  http://localhost:5000" -ForegroundColor Green
Write-Host "Frontend: http://localhost:5173" -ForegroundColor Green
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
