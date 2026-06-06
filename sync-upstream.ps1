# Sync Sweet Home 3D source from official SVN trunk
$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceDir = Join-Path $ProjectRoot "SweetHome3D-7.5-src"
$SvnUrl = "https://svn.code.sf.net/p/sweethome3d/code/trunk/SweetHome3D"

if (-not (Get-Command svn -ErrorAction SilentlyContinue)) {
    throw "svn not found. Install with: winget install Slik.Subversion"
}

if (Test-Path (Join-Path $SourceDir ".svn")) {
    Write-Host "Updating existing SVN checkout..."
    svn update $SourceDir
} else {
    if (Test-Path $SourceDir) {
        throw "Source directory exists but is not an SVN checkout. Remove or rename it first, or use the ZIP from setup.ps1."
    }
    Write-Host "Checking out trunk from SourceForge..."
    svn checkout $SvnUrl $SourceDir
}

Write-Host "Done. Rebuild with .\build.ps1"
