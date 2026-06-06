# Build Sweet Home 3D from source (Windows)
$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceDir = Join-Path $ProjectRoot "SweetHome3D-7.5-src"
$AntHome = Join-Path $ProjectRoot "tools\apache-ant\apache-ant-1.10.15"

$JavaHome = $env:JAVA_HOME
if (-not $JavaHome) {
    $JavaHome = "C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot"
}

if (-not (Test-Path $JavaHome)) {
    throw "JAVA_HOME not found at $JavaHome. Install JDK 17+ or set JAVA_HOME."
}
if (-not (Test-Path $AntHome)) {
    throw "Apache Ant not found at $AntHome. Run setup.ps1 first."
}
if (-not (Test-Path $SourceDir)) {
    throw "Source directory not found at $SourceDir."
}

$env:JAVA_HOME = $JavaHome
$env:ANT_HOME = $AntHome
$env:Path = "$JavaHome\bin;$AntHome\bin;$env:Path"

Push-Location $SourceDir
try {
    ant @args
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Output JAR: $SourceDir\install\SweetHome3D-7.5.jar" -ForegroundColor Green
