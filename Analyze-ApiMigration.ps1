#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Analyzes TMRazor vs TMRazor Improved API migration coverage.
#>

param(
    [string]$RootPath      = $PSScriptRoot,
    [string]$OutputFile    = "",
    [string]$Module        = "",      
    [switch]$NoColor
)

$OriginalApiPath  = Join-Path $RootPath "Razor\RazorEnhanced"
$ImprovedApiPath  = Join-Path $RootPath "TMRazorImproved\TMRazorImproved.Core\Services\Scripting\Api"

$ModuleMap = [ordered]@{
    "Player"   = @{ Original = "Player.cs";  Improved = "PlayerApi.cs" }
    "Item"     = @{ Original = "Item.cs";    Improved = "Wrappers.cs";   Class = "ScriptItem" }
    "Mobile"   = @{ Original = "Mobile.cs";  Improved = "Wrappers.cs";   Class = "ScriptMobile" }
    "Items"    = @{ Original = "Item.cs";    Improved = "ItemsApi.cs";    Class = "Items" }
    "Mobiles"  = @{ Original = "Mobile.cs";  Improved = "MobilesApi.cs";  Class = "Mobiles" }
    "Gumps"    = @{ Original = "Gumps.cs";   Improved = "GumpsApi.cs"  }
    "Target"   = @{ Original = "Target.cs";  Improved = "TargetApi.cs" }
    "Spells"   = @{ Original = "Spells.cs";  Improved = "SpellsApi.cs" }
    "Skills"   = @{ Original = "Skills.cs";  Improved = "SkillsApi.cs" }
    "Misc"     = @{ Original = "Misc.cs";    Improved = "MiscApi.cs"   }
    "Journal"  = @{ Original = "Journal.cs"; Improved = "JournalApi.cs"}
    "Filters"  = @{ Original = "Filters.cs"; Improved = "FiltersApi.cs"}
    "Friend"   = @{ Original = "Friend.cs";  Improved = "FriendApi.cs" }
    "Statics"  = @{ Original = "Statics.cs"; Improved = "StaticsApi.cs"}
    "Timer"    = @{ Original = "Timer.cs";   Improved = "TimerApi.cs"  }
}

function Write-Color {
    param([string]$Text, [ConsoleColor]$FG = [ConsoleColor]::White, [switch]$NoNewLine)
    if ($NoNewLine) { Write-Host $Text -ForegroundColor $FG -NoNewline }
    else            { Write-Host $Text -ForegroundColor $FG }
}

function Get-PublicMembers {
    param([string]$FilePath, [string]$ClassName)

    $members = @()
    if (-not (Test-Path $FilePath)) { return $members }

    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if (-not $content) { return $members }

    $content = $content -replace '//[^\r\n]*', ''
    $content = $content -replace '(?s)/\*.*?\*/', ''

    if ($ClassName) {
        if ($content -match "(?s)public\s+(?:static\s+)?class\s+$ClassName\b.*?\{(.*)\}") {
             $content = $Matches[1]
        }
    }

    $propPattern = '(?m)^\s*public\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:new\s+)?(?!class|enum|interface|struct|delegate)[\w\[\]<>, ?]+\s+(\w+)\s*(?:\{|=>)'
    foreach ($m in [regex]::Matches($content, $propPattern)) {
        $name = $m.Groups[1].Value
        if ($name -notin @('set','get','this')) { $members += "PROP:$name" }
    }

    $methPattern = '(?m)^\s*public\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:async\s+)?(?:new\s+)?(?!class|enum|interface|struct|delegate)[\w\[\]<>, ?]+\s+(\w+)\s*[(<]'
    foreach ($m in [regex]::Matches($content, $methPattern)) {
        $name = $m.Groups[1].Value
        if ($name -notin @('get','set','if','while','for','foreach','return','new','class','enum','interface','struct')) {
            $members += "METH:$name"
        }
    }

    $seen = @{}; $result = @()
    foreach ($m in $members) {
        $n = $m.Split(':')[1]; if (-not $seen.ContainsKey($n)) { $seen[$n] = $true; $result += $m }
    }
    return $result
}

$outputLines = [System.Collections.Generic.List[string]]::new()
function Out { param([string]$Text = "", [ConsoleColor]$FG = [ConsoleColor]::White) $outputLines.Add($Text); Write-Color $Text -FG $FG }

Out ""
Out ("=" * 80) Cyan
Out "  TMRazor -> TMRazor Improved API Migration Coverage Report" Cyan
Out "  Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" Cyan
Out ("=" * 80) Cyan
Out ""

$summaryRows = @()
$globalTotal = 0; $globalMissing = 0

$modulesToProcess = if ($Module) { $ModuleMap.GetEnumerator() | Where-Object { $_.Key -eq $Module } } else { $ModuleMap.GetEnumerator() }

foreach ($entry in $modulesToProcess) {
    $moduleName = $entry.Key
    $origFile = Join-Path $OriginalApiPath $entry.Value.Original
    $improvedFile = Join-Path $ImprovedApiPath $entry.Value.Improved
    
    $origClass = if ($moduleName -eq "Items") { "Items" } elseif ($moduleName -eq "Mobiles") { "Mobiles" } else { "" }
    $improvedClass = $entry.Value.Class

    $origMembers = Get-PublicMembers $origFile $origClass
    $improvedMembers = Get-PublicMembers $improvedFile $improvedClass

    $improvedNames = $improvedMembers | ForEach-Object { $_.Split(':')[1].ToLower() }
    $missing = $origMembers | Where-Object { $_.Split(':')[1].ToLower() -notin $improvedNames }
    
    $total = $origMembers.Count; $missCount = $missing.Count; $implCount = $total - $missCount
    $pct = if ($total -gt 0) { [math]::Round($implCount * 100.0 / $total) } else { 100 }

    $globalTotal += $total; $globalMissing += $missCount
    $summaryRows += [PSCustomObject]@{ Module=$moduleName; Total=$total; Implemented=$implCount; Missing=$missCount; Coverage="$pct%" }

    Out ("#" * 80) DarkCyan
    Out "  MODULE: $($moduleName.ToUpper()) [$implCount / $total | $pct%]" DarkCyan
    Out "  Original: $origFile ($origClass)"
    Out "  Improved: $improvedFile ($improvedClass)"
    Out ("#" * 80) DarkCyan

    $bw = 60; $f = [math]::Round($pct * $bw / 100); $e = $bw - $f
    $color = if ($pct -ge 80) { "Green" } elseif ($pct -ge 50) { "Yellow" } else { "Red" }
    Out ("  [" + ("#" * $f) + ("-" * $e) + "] $pct%") $color
    Out ""

    if ($missCount -gt 0) {
        Out "  +- MISSING MEMBERS ($missCount) " Red
        $missing | Sort-Object | ForEach-Object { Out "  |  ? $_" Yellow }
        Out "  +---------------------------" Red
    } else { Out "  [ OK ] All members implemented!" Green }
    Out ""
}

Out ("=" * 80) Cyan
Out "  GLOBAL SUMMARY" Cyan
$hdr = "  {0,-10} {1,7} {2,13} {3,9} {4,10}" -f "Module","Total","Implemented","Missing","Coverage"
Out $hdr White
foreach ($row in $summaryRows) {
    Out ("  {0,-10} {1,7} {2,13} {3,9} {4,10}" -f $row.Module, $row.Total, $row.Implemented, $row.Missing, $row.Coverage)
}
Out ("=" * 80) Cyan
Out "  OVERALL: $([math]::Round(($globalTotal-$globalMissing)*100/$globalTotal))%" Green
