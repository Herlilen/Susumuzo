[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet("version", "compile", "test-editmode", "test-playmode")]
    [string]$Command = "compile",

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$AdditionalArguments = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$versionFile = Join-Path $projectRoot "ProjectSettings\ProjectVersion.txt"
$versionLine = Get-Content -LiteralPath $versionFile -TotalCount 1
$versionMatch = [regex]::Match($versionLine, '^m_EditorVersion:\s*(\S+)')

if (-not $versionMatch.Success) {
    throw "Could not read the Unity version from $versionFile"
}

$projectVersion = $versionMatch.Groups[1].Value

function Find-UnityEditor {
    $candidates = [Collections.Generic.List[string]]::new()

    if (-not [string]::IsNullOrWhiteSpace($env:UNITY_EDITOR_PATH)) {
        $candidates.Add($env:UNITY_EDITOR_PATH)
    }

    Get-Process Unity -ErrorAction SilentlyContinue |
        Where-Object { $_.Path -and $_.Path.Contains($projectVersion) } |
        ForEach-Object { $candidates.Add($_.Path) }

    $candidates.Add("D:\Unity\Editors\$projectVersion\Editor\Unity.exe")
    $candidates.Add("D:\Unity\Hub\Editor\$projectVersion\Editor\Unity.exe")
    $candidates.Add((Join-Path $env:ProgramFiles "Unity\Hub\Editor\$projectVersion\Editor\Unity.exe"))

    foreach ($candidate in $candidates | Select-Object -Unique) {
        if (Test-Path -LiteralPath $candidate -PathType Leaf) {
            return [IO.Path]::GetFullPath($candidate)
        }
    }

    throw "Unity $projectVersion was not found. Install it with Unity Hub or set UNITY_EDITOR_PATH."
}

function Assert-ProjectIsAvailable {
    $lockFile = Join-Path $projectRoot "Temp\UnityLockfile"
    if (Test-Path -LiteralPath $lockFile) {
        throw "The project is open in Unity. Close that Editor instance before running '$Command'."
    }
}

function Invoke-UnityEditor([string[]]$Arguments) {
    $process = Start-Process -FilePath $unityEditor -ArgumentList $Arguments -NoNewWindow -Wait -PassThru
    if ($process.ExitCode -ne 0) {
        exit $process.ExitCode
    }
}

$unityEditor = Find-UnityEditor
Write-Host "Unity CLI: $unityEditor"
Write-Host "Project:   $projectRoot"

if ($Command -eq "version") {
    Invoke-UnityEditor @("-version", "-batchmode", "-quit")
    exit 0
}

Assert-ProjectIsAvailable

$commonArguments = @(
    "-batchmode",
    "-nographics",
    "-accept-apiupdate",
    "-projectPath", $projectRoot,
    "-logFile", "-"
)

switch ($Command) {
    "compile" {
        Invoke-UnityEditor ($commonArguments + @("-quit") + $AdditionalArguments)
    }
    "test-editmode" {
        $resultPath = Join-Path $projectRoot "Temp\TestResults-EditMode.xml"
        Invoke-UnityEditor ($commonArguments + @(
            "-runTests",
            "-testPlatform", "EditMode",
            "-testResults", $resultPath
        ) + $AdditionalArguments)
    }
    "test-playmode" {
        $resultPath = Join-Path $projectRoot "Temp\TestResults-PlayMode.xml"
        Invoke-UnityEditor ($commonArguments + @(
            "-runTests",
            "-testPlatform", "PlayMode",
            "-testResults", $resultPath
        ) + $AdditionalArguments)
    }
}
