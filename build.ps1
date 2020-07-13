Param (
    [Parameter(Position = 0)]
    [string]$Arg0,

    [Parameter(ValueFromRemainingArguments = $true)]
    [Object[]]$RemainingArgs
)

# PSScriptRoot isn't set in PowerShell 2
$minPSVer = [version]"3.0"
if (($PSVersionTable.PSVersion -lt $minPSVer)) {
    [Console]::ForegroundColor = 'red'
    [Console]::Error.WriteLine("This script does not support PowerShell $($PSVersionTable.PSVersion).")
    [Console]::Error.WriteLine("Please upgrade to PowerShell $minPSVer or later.")
    [Console]::ResetColor()
    exit
}

# Globals
$NugetVersion       = "5.3.1"
$UseExperimental    = $false
$RootDir            = "${PSScriptRoot}"
$ScriptFile         = "${RootDir}/build.cake"
$BuildDir           = "${RootDir}/_build"
$ToolsDir           = "${BuildDir}/tools"
$PackagesDir        = "${BuildDir}/lib/nuget"
$NugetExe           = "${ToolsDir}/NuGet/${NugetVersion}/nuget.exe"
$PackagesConfigFile = "${RootDir}/packages.config"
$CakeVersion        = (Select-Xml -Xml ([xml](Get-Content $PackagesConfigFile)) -XPath "//package[@id='Cake'][1]/@version").Node.Value
$CakeExe            = "${PackagesDir}/Cake.${CakeVersion}/Cake.exe"

# Download NuGet
$NugetDir = Split-Path "$NugetExe" -Parent
if (!(Test-Path "$NugetDir")) {
    mkdir $nugetDir > $null
}

if (!(Test-Path "$NugetExe")) {
    (New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/v${NugetVersion}/nuget.exe", $NugetExe)
}

# Install build packages
Invoke-Expression "& '${NugetExe}' restore `"${PackagesConfigFile}`" -OutputDirectory `"${PackagesDir}`""

# Build args
$cakeArgs = @()

if ($Arg0) {
    if ($Arg0[0] -eq "-") {
        $cakeArgs += "${Arg0}"
    } else {
        $cakeArgs += "--target=${Arg0}"
    }
}

if ($UseExperimental) {
    $cakeArgs += "--experimental"
}

# Run Cake
Invoke-Expression "& '${CakeExe}' '${ScriptFile}' ${cakeArgs} ${RemainingArgs}"
exit $LASTEXITCODE
