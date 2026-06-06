# Run a locally built Sweet Home 3D JAR (Windows)
$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceDir = Join-Path $ProjectRoot "SweetHome3D-7.5-src"
$Jar = Join-Path $SourceDir "install\SweetHome3D-7.5.jar"

$JavaHome = $env:JAVA_HOME
if (-not $JavaHome) {
    $JavaHome = "C:\Program Files\Microsoft\jdk-17.0.19.10-hotspot"
}

if (-not (Test-Path $Jar)) {
    throw "Built JAR not found. Run .\build.ps1 first."
}

$Java = Join-Path $JavaHome "bin\java.exe"
$NativeLibs = Join-Path $SourceDir "lib\java3d-1.6\windows\amd64"

$Args = @(
    "-Xmx2048m"
    "-Djava.library.path=$NativeLibs"
    "--add-opens=java.desktop/java.awt=ALL-UNNAMED"
    "--add-opens=java.desktop/sun.awt=ALL-UNNAMED"
    "--add-opens=java.desktop/com.apple.eio=ALL-UNNAMED"
    "--add-opens=java.desktop/com.apple.eawt=ALL-UNNAMED"
    "-jar"
    $Jar
)

Start-Process -FilePath $Java -ArgumentList $Args -WorkingDirectory (Split-Path $Jar)
