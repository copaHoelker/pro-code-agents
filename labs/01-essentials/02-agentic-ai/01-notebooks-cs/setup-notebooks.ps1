# C# Agentic AI Notebooks - Setup Script
# Simple sequential setup for running .NET Interactive notebooks

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  C# Agentic AI Notebooks - Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "> Checking .NET SDK..." -ForegroundColor Magenta
$dotnetVersion = dotnet --version 2>$null
if ($dotnetVersion) {
    Write-Host "  [OK] .NET SDK $dotnetVersion installed" -ForegroundColor Green
} else {
    Write-Host "  [ERROR] .NET SDK not found" -ForegroundColor Red
    Write-Host "  Install from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
}

# Install .NET Interactive
Write-Host ""
Write-Host "> Installing .NET Interactive..." -ForegroundColor Magenta
dotnet tool install -g Microsoft.dotnet-interactive 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] .NET Interactive installed" -ForegroundColor Green
} else {
    dotnet tool update -g Microsoft.dotnet-interactive 2>&1 | Out-Null
    Write-Host "  [OK] .NET Interactive updated" -ForegroundColor Green
}

# Register Jupyter kernels
Write-Host ""
Write-Host "> Registering notebook kernels..." -ForegroundColor Magenta
dotnet interactive jupyter install 2>&1 | Out-Null
Write-Host "  [OK] Kernels registered" -ForegroundColor Green

# Check VS Code
Write-Host ""
Write-Host "> Checking VS Code..." -ForegroundColor Magenta
$codeVersion = code --version 2>$null
if ($codeVersion) {
    Write-Host "  [OK] VS Code installed" -ForegroundColor Green
    
    # Install Polyglot Notebooks extension
    Write-Host "  Installing Polyglot Notebooks extension..." -ForegroundColor Cyan
    code --install-extension ms-dotnettools.dotnet-interactive-vscode --force 2>&1 | Out-Null
    Write-Host "  [OK] Extension installed" -ForegroundColor Green
} else {
    Write-Host "  [WARN] VS Code not found - optional" -ForegroundColor Yellow
}

# Create .env file if it doesn't exist
Write-Host ""
Write-Host "> Checking environment configuration..." -ForegroundColor Magenta
$envPath = Join-Path $PSScriptRoot ".env"
if (-not (Test-Path $envPath)) {
    Write-Host "  Creating .env template..." -ForegroundColor Cyan
    @"
# Azure AI Foundry Configuration
PROJECT_ENDPOINT=https://your-project.api.azureml.ms
MODEL=gpt-4o
"@ | Out-File -FilePath $envPath -Encoding UTF8
    Write-Host "  [OK] Created .env file at: $envPath" -ForegroundColor Green
    Write-Host "  [ACTION] Please edit .env and add your credentials" -ForegroundColor Yellow
} else {
    Write-Host "  [OK] .env file exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Edit .env file with your Azure AI Foundry credentials"
Write-Host "  2. Run: code ." -ForegroundColor Gray
Write-Host "  3. Open any .ipynb notebook"
Write-Host "  4. Select '.NET Interactive' kernel"
Write-Host "  5. Run cells!"
Write-Host ""
Write-Host "Available notebooks:" -ForegroundColor Cyan
Get-ChildItem -Path $PSScriptRoot -Filter "*.ipynb" | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
}
Write-Host ""
