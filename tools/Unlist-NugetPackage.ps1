#!/usr/bin/env pwsh
<#
.SYNOPSIS
Unlist one or more NuGet package versions from a feed (nuget.org by default).

.DESCRIPTION
Wraps `dotnet nuget delete` with two input modes:

  1. Single — pass -Package and one or more -Version values.
  2. Bulk   — pass -JsonPath pointing at a JSON file shaped like
             [ { "package": "Fallout.Common", "version": "10.3.45" }, ... ].

The NuGet API key comes from -ApiKey if provided, otherwise from the
NUGET_API_KEY environment variable. The script fails fast if neither
is available.

"Unlist" hides a version from search and version-listings; the binary
remains downloadable for anyone with an explicit pin. There is no true
"unpublish" on NuGet.

.PARAMETER Package
Package ID, e.g. 'Fallout.Common'. Required for single-mode.

.PARAMETER Version
One or more versions to unlist. Required for single-mode. Multiple values
go through one `dotnet nuget delete` call each.

.PARAMETER JsonPath
Path to a JSON file containing an array of { package, version } objects.
Required for bulk-mode. Mutually exclusive with -Package/-Version.

.PARAMETER ApiKey
NuGet API key. Falls back to $env:NUGET_API_KEY if omitted.

.PARAMETER Source
NuGet feed URL. Defaults to https://api.nuget.org/v3/index.json.

.EXAMPLE
./Unlist-NugetPackage.ps1 -Package Fallout.Common -Version 10.3.45

.EXAMPLE
./Unlist-NugetPackage.ps1 -Package Fallout.Common -Version 10.3.40,10.3.41,10.3.42 -WhatIf

.EXAMPLE
./Unlist-NugetPackage.ps1 -JsonPath ./batch.json

.EXAMPLE
# API key from env
$env:NUGET_API_KEY = (security find-generic-password -s NugetApiKey -w)
./Unlist-NugetPackage.ps1 -JsonPath ./batch.json
#>
[CmdletBinding(SupportsShouldProcess, DefaultParameterSetName = 'Single')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Single', Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string] $Package,

    [Parameter(Mandatory, ParameterSetName = 'Single', Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string[]] $Version,

    [Parameter(Mandatory, ParameterSetName = 'Bulk')]
    [ValidateNotNullOrEmpty()]
    [string] $JsonPath,

    [Parameter()]
    [string] $ApiKey,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $Source = 'https://api.nuget.org/v3/index.json'
)

$ErrorActionPreference = 'Stop'

if (-not $ApiKey) {
    $ApiKey = $env:NUGET_API_KEY
}
if (-not $ApiKey) {
    throw "API key not provided. Pass -ApiKey <key> or set `$env:NUGET_API_KEY."
}

$items = if ($PSCmdlet.ParameterSetName -eq 'Bulk') {
    if (-not (Test-Path -LiteralPath $JsonPath -PathType Leaf)) {
        throw "JSON file not found: $JsonPath"
    }
    $raw = Get-Content -LiteralPath $JsonPath -Raw
    try {
        $parsed = $raw | ConvertFrom-Json
    } catch {
        throw "Failed to parse JSON at $JsonPath`: $($_.Exception.Message)"
    }
    if ($parsed -isnot [System.Collections.IEnumerable]) {
        throw "JSON at $JsonPath must be an array of { package, version } objects."
    }
    @($parsed)
} else {
    $Version | ForEach-Object {
        [pscustomobject]@{ package = $Package; version = $_ }
    }
}

$total = ($items | Measure-Object).Count
if ($total -eq 0) {
    Write-Host "Nothing to unlist."
    return
}

# Validate shape
$items | ForEach-Object {
    if (-not $_.package -or -not $_.version) {
        throw "Invalid item — every entry must have 'package' and 'version' fields. Offending: $($_ | ConvertTo-Json -Compress)"
    }
}

Write-Host "Unlisting $total package version(s) from $Source"
Write-Host ""

$ok = 0
$fail = 0
$i = 0

foreach ($item in $items) {
    $i++
    $pkg = $item.package
    $ver = $item.version
    $label = "[{0,3}/{1,3}] {2,-44} {3,-10}" -f $i, $total, $pkg, $ver

    if ($PSCmdlet.ShouldProcess("$pkg $ver", "Unlist from $Source")) {
        $output = & dotnet nuget delete $pkg $ver `
            --api-key $ApiKey `
            --source $Source `
            --non-interactive 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "$label ok"
            $ok++
        } else {
            Write-Host "$label FAIL"
            Write-Host ($output | Out-String).TrimEnd()
            $fail++
        }
    } else {
        Write-Host "$label (skipped: -WhatIf)"
    }
}

Write-Host ""
Write-Host "Done. ok=$ok fail=$fail"

if ($fail -gt 0) {
    exit $fail
}
