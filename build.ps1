Function Info($msg) {
    Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
    Write-Host `n`n
    Write-Error $msg
    exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
    if(-Not $?) {
        Error "${msg}. Error code: $LastExitCode"
    }
}

Function GetVersion() {
    $gitCommand = Get-Command -Name git

    try { $tag = & $gitCommand describe --exact-match --tags HEAD 2>$null } catch { }
    if(-Not $?) {
        Info "The commit is not tagged. Use 'v0.0' as a version instead"
        $tag = "v0.0"
    }

    return "$($tag.Substring(1))"
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot"
$buildDir = "$root/build"
$version = GetVersion

Info "Build project. Version: '$version'"
dotnet build `
  --configuration Release `
  /property:OsType="Windows" `
  /property:Platform="Any CPU" `
  /property:Version=$version `
  "$root/GraphVizProxy.sln"
CheckReturnCodeOfPreviousCommand "build failed"

Info "Run tests"
dotnet test `
  --configuration Release `
  --no-build `
  "$root/GraphVizProxy.sln"
CheckReturnCodeOfPreviousCommand "tests failed"

Info "Create Nuget package"
dotnet pack `
  --no-build `
  --configuration Release `
  /property:Version=$version `
  "$root/GraphVizProxy.sln"
CheckReturnCodeOfPreviousCommand "Nuget package creation failed"

Info "Copy the Nuget package into the publish directory"
Remove-Item "$buildDir/Publish" -Force -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path "$buildDir/Publish" > $null
Copy-Item "$buildDir/Release/GraphVizProxy/GraphVizProxy.*.nupkg" "$buildDir/Publish"
