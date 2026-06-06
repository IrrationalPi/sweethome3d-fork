# One-time setup: download tools and Sweet Home 3D 7.5 source
$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ToolsDir = Join-Path $ProjectRoot "tools"
$AntDir = Join-Path $ToolsDir "apache-ant"
$SrcZip = Join-Path $ProjectRoot "SweetHome3D-7.5-src.zip"
$SrcDir = Join-Path $ProjectRoot "SweetHome3D-7.5-src"

New-Item -ItemType Directory -Force -Path $ToolsDir | Out-Null

if (-not (Test-Path $AntDir)) {
    Write-Host "Downloading Apache Ant..."
    $AntZip = Join-Path $env:TEMP "apache-ant.zip"
    Invoke-WebRequest -Uri "https://archive.apache.org/dist/ant/binaries/apache-ant-1.10.15-bin.zip" -OutFile $AntZip
    Expand-Archive -Path $AntZip -DestinationPath $AntDir -Force
}

if (-not (Test-Path $SrcDir)) {
    Write-Host "Downloading Sweet Home 3D 7.5 source (~48 MB)..."
    curl.exe -L -o $SrcZip "https://prdownloads.sourceforge.net/sweethome3d/SweetHome3D-7.5-src.zip"
    if ((Get-Item $SrcZip).Length -lt 40MB) {
        throw "Source download looks too small. Check network or download manually."
    }
    Expand-Archive -Path $SrcZip -DestinationPath $ProjectRoot -Force
}

Write-Host "Setup complete."
Write-Host "Install JDK 17 if needed: winget install Microsoft.OpenJDK.17"
Write-Host "Then build: .\build.ps1"
Write-Host "Then run:   .\run.ps1"
