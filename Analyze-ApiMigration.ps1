#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Analyzes TMRazor vs TMRazor Improved API migration coverage.

.DESCRIPTION
    Scans original RazorEnhanced C# API files and TMRazor Improved API files,
    extracts all public methods/properties, compares them, and outputs a
    wireframe-style report showing missing APIs by category.

.EXAMPLE
    ./Analyze-ApiMigration.ps1
    ./Analyze-ApiMigration.ps1 -OutputFile report.txt
    ./Analyze-ApiMigration.ps1 -Module Player
#>

param(
    [string]$RootPath      = $PSScriptRoot,
    [string]$OutputFile    = "",
    [string]$Module        = "",      # Filter to a single module (e.g. "Player")
    [switch]$NoColor
)

# ─────────────────────────────────────────────────────────────────────────────
# Paths
# ─────────────────────────────────────────────────────────────────────────────
$OriginalApiPath  = Join-Path $RootPath "Razor\RazorEnhanced"
$ImprovedApiPath  = Join-Path $RootPath "TMRazorImproved\TMRazorImproved.Core\Services\Scripting\Api"

# ─────────────────────────────────────────────────────────────────────────────
# Module mapping: OriginalClass -> ImprovedFile
# ─────────────────────────────────────────────────────────────────────────────
$ModuleMap = [ordered]@{
    "Player"   = @{ Original = "Player.cs";  Improved = "PlayerApi.cs"  }
    "Items"    = @{ Original = "Item.cs";    Improved = "ItemsApi.cs"   }
    "Mobiles"  = @{ Original = "Mobile.cs";  Improved = "MobilesApi.cs" }
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

# ─────────────────────────────────────────────────────────────────────────────
# Color helpers
# ─────────────────────────────────────────────────────────────────────────────
function Write-Color {
    param([string]$Text, [ConsoleColor]$FG = [ConsoleColor]::White, [switch]$NoNewLine)
    if ($NoColor -or -not $Host.UI.SupportsVirtualTerminal) {
        if ($NoNewLine) { Write-Host $Text -NoNewline } else { Write-Host $Text }
    } else {
        if ($NoNewLine) { Write-Host $Text -ForegroundColor $FG -NoNewline }
        else            { Write-Host $Text -ForegroundColor $FG }
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Extract public members (methods + properties) from a C# file
# Returns a list of strings: "METHOD name" or "PROP name"
# ─────────────────────────────────────────────────────────────────────────────
function Get-PublicMembers {
    param([string]$FilePath)

    $members = @()
    if (-not (Test-Path $FilePath)) { return $members }

    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if (-not $content) { return $members }

    # Remove single-line comments
    $content = $content -replace '//[^\r\n]*', ''
    # Remove block comments
    $content = $content -replace '(?s)/\*.*?\*/', ''

    # ── Properties: public [static] [virtual] [override] Type Name { get
    $propPattern = '(?m)^\s*public\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:new\s+)?(?!class|enum|interface|struct|delegate)[\w\[\]<>, ?]+\s+(\w+)\s*\{'
    $propMatches = [regex]::Matches($content, $propPattern)
    foreach ($m in $propMatches) {
        $name = $m.Groups[1].Value
        if ($name -notin @('set','get','this')) {
            $members += "PROP:$name"
        }
    }

    # ── Methods: public [static] [virtual] [override] [async] ReturnType Name(
    $methPattern = '(?m)^\s*public\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:async\s+)?(?:new\s+)?(?!class|enum|interface|struct|delegate)[\w\[\]<>, ?]+\s+(\w+)\s*[(<]'
    $methMatches = [regex]::Matches($content, $methPattern)
    foreach ($m in $methMatches) {
        $name = $m.Groups[1].Value
        # Skip property accessors and noise words
        if ($name -notin @('get','set','if','while','for','foreach','return','new','class','enum','interface','struct')) {
            $members += "METH:$name"
        }
    }

    # Deduplicate names (keep type prefix for display, but deduplicate by name only)
    $seen   = @{}
    $result = @()
    foreach ($m in $members) {
        $n = $m.Split(':')[1]
        if (-not $seen.ContainsKey($n)) {
            $seen[$n] = $true
            $result  += $m
        }
    }
    return $result
}

# ─────────────────────────────────────────────────────────────────────────────
# Build output lines (also written to file if requested)
# ─────────────────────────────────────────────────────────────────────────────
$outputLines = [System.Collections.Generic.List[string]]::new()

function Out {
    param([string]$Text = "", [ConsoleColor]$FG = [ConsoleColor]::White)
    $outputLines.Add($Text)
    Write-Color $Text -FG $FG
}

function OutRaw([string]$Text) { Out $Text }

# ─────────────────────────────────────────────────────────────────────────────
# HEADER
# ─────────────────────────────────────────────────────────────────────────────
$line80 = "=" * 80
$stamp  = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

Out ""
Out $line80 Cyan
Out "  TMRazor  ->  TMRazor Improved   API Migration Coverage Report" Cyan
Out "  Generated: $stamp" Cyan
Out $line80 Cyan
Out ""

# Filter modules if requested
$modulesToProcess = if ($Module) {
    $ModuleMap.GetEnumerator() | Where-Object { $_.Key -eq $Module }
} else {
    $ModuleMap.GetEnumerator()
}

# ─────────────────────────────────────────────────────────────────────────────
# Per-module analysis
# ─────────────────────────────────────────────────────────────────────────────
$globalTotal   = 0
$globalMissing = 0
$summaryRows   = @()

foreach ($entry in $modulesToProcess) {
    $moduleName   = $entry.Key
    $origFile     = Join-Path $OriginalApiPath  $entry.Value.Original
    $improvedFile = Join-Path $ImprovedApiPath  $entry.Value.Improved

    $origMembers     = Get-PublicMembers $origFile
    $improvedMembers = Get-PublicMembers $improvedFile

    # Names only (lowercase) for comparison
    $improvedNames = $improvedMembers | ForEach-Object { $_.Split(':')[1].ToLower() }

    $missing = $origMembers | Where-Object {
        $name = $_.Split(':')[1].ToLower()
        $name -notin $improvedNames
    }
    $implemented = $origMembers | Where-Object {
        $name = $_.Split(':')[1].ToLower()
        $name -in $improvedNames
    }

    $total       = $origMembers.Count
    $missCount   = $missing.Count
    $implCount   = $implemented.Count
    $pct         = if ($total -gt 0) { [math]::Round($implCount * 100.0 / $total) } else { 100 }

    $globalTotal   += $total
    $globalMissing += $missCount

    $summaryRows += [PSCustomObject]@{
        Module      = $moduleName
        Total       = $total
        Implemented = $implCount
        Missing     = $missCount
        Coverage    = "$pct%"
    }

    # ── Module header ──────────────────────────────────────────────────────
    $bar    = "#" * 80
    Out ""
    Out $bar DarkCyan
    Out "  MODULE: $($moduleName.ToUpper())   [$implCount / $total implemented  |  $pct% coverage]" DarkCyan
    Out "  Original : $origFile"
    Out "  Improved : $improvedFile"
    if (-not (Test-Path $origFile))     { Out "  [!] Original file NOT FOUND"  Yellow }
    if (-not (Test-Path $improvedFile)) { Out "  [!] Improved file NOT FOUND"  Yellow }
    Out $bar DarkCyan

    # ── Coverage bar ──────────────────────────────────────────────────────
    $barWidth = 60
    $filled   = [math]::Round($pct * $barWidth / 100)
    $empty    = $barWidth - $filled
    $coverBar = "[" + ("█" * $filled) + ("░" * $empty) + "]  $pct%"
    $barColor = if ($pct -ge 80) { [ConsoleColor]::Green }
                elseif ($pct -ge 50) { [ConsoleColor]::Yellow }
                else { [ConsoleColor]::Red }
    Out "  $coverBar" $barColor
    Out ""

    if ($missing.Count -eq 0) {
        Out "  ✔  All members implemented!" Green
    } else {
        # ── Wireframe table of missing members ────────────────────────────
        Out "  ┌─ MISSING MEMBERS ($missCount) ─────────────────────────────────────────────┐" Red

        $missingProps  = $missing | Where-Object { $_.StartsWith("PROP:") } | ForEach-Object { $_.Substring(5) } | Sort-Object
        $missingMeths  = $missing | Where-Object { $_.StartsWith("METH:") } | ForEach-Object { $_.Substring(5) } | Sort-Object

        if ($missingProps.Count -gt 0) {
            Out "  │  ┌─[ PROPERTIES ($($missingProps.Count)) ]" Yellow
            foreach ($p in $missingProps) {
                Out "  │  │  ○ $p" Yellow
            }
            Out "  │  └─────────────────────────────────────────────────────────────────" Yellow
        }

        if ($missingMeths.Count -gt 0) {
            Out "  │  ┌─[ METHODS ($($missingMeths.Count)) ]" Magenta
            foreach ($m in $missingMeths) {
                Out "  │  │  ○ $m" Magenta
            }
            Out "  │  └─────────────────────────────────────────────────────────────────" Magenta
        }

        Out "  └─────────────────────────────────────────────────────────────────────────┘" Red
    }
    Out ""
}

# ─────────────────────────────────────────────────────────────────────────────
# GLOBAL SUMMARY TABLE
# ─────────────────────────────────────────────────────────────────────────────
$globalImpl = $globalTotal - $globalMissing
$globalPct  = if ($globalTotal -gt 0) { [math]::Round($globalImpl * 100.0 / $globalTotal) } else { 100 }

Out ""
Out $line80 Cyan
Out "  GLOBAL SUMMARY" Cyan
Out $line80 Cyan
Out ""

# Column widths
$colW = @{ Module=10; Total=7; Impl=13; Miss=9; Cov=10 }
$hdr  = "  {0,-$($colW.Module)} {1,$($colW.Total)} {2,$($colW.Impl)} {3,$($colW.Miss)} {4,$($colW.Cov)}" -f "Module","Total","Implemented","Missing","Coverage"
$sep  = "  " + ("-" * ($colW.Module + $colW.Total + $colW.Impl + $colW.Miss + $colW.Cov + 4))

Out $hdr White
Out $sep White

foreach ($row in $summaryRows) {
    $pctNum = [int]($row.Coverage -replace '%','')
    $color  = if ($pctNum -ge 80) { [ConsoleColor]::Green }
              elseif ($pctNum -ge 50) { [ConsoleColor]::Yellow }
              else { [ConsoleColor]::Red }
    $line = "  {0,-$($colW.Module)} {1,$($colW.Total)} {2,$($colW.Impl)} {3,$($colW.Miss)} {4,$($colW.Cov)}" `
            -f $row.Module, $row.Total, $row.Implemented, $row.Missing, $row.Coverage
    Out $line $color
}

Out $sep White

$totalLine = "  {0,-$($colW.Module)} {1,$($colW.Total)} {2,$($colW.Impl)} {3,$($colW.Miss)} {4,$($colW.Cov)}" `
             -f "TOTAL", $globalTotal, $globalImpl, $globalMissing, "$globalPct%"
$totColor  = if ($globalPct -ge 80) { [ConsoleColor]::Green }
             elseif ($globalPct -ge 50) { [ConsoleColor]::Yellow }
             else { [ConsoleColor]::Red }
Out $totalLine $totColor
Out ""

# Global bar
$barWidth  = 60
$filled    = [math]::Round($globalPct * $barWidth / 100)
$empty     = $barWidth - $filled
$globalBar = "[" + ("█" * $filled) + ("░" * $empty) + "]  $globalPct% overall coverage"
$barColor  = if ($globalPct -ge 80) { [ConsoleColor]::Green }
             elseif ($globalPct -ge 50) { [ConsoleColor]::Yellow }
             else { [ConsoleColor]::Red }
Out "  $globalBar" $barColor
Out ""
Out $line80 Cyan
Out ""

# ─────────────────────────────────────────────────────────────────────────────
# Optional file output
# ─────────────────────────────────────────────────────────────────────────────
if ($OutputFile) {
    $outputLines | Set-Content -Path $OutputFile -Encoding UTF8
    Write-Host ""
    Write-Host "Report saved to: $OutputFile" -ForegroundColor Green
}

