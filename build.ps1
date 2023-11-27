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
$NugetVersion       = "6.8.0"
$UseExperimental    = $false
$RootDir            = "${PSScriptRoot}"
$ScriptFile         = "${RootDir}/build.cake"
$BuildDir           = "${RootDir}/_build"
$ToolsDir           = "${BuildDir}/tools"
$NugetExe           = "${ToolsDir}/NuGet/${NugetVersion}/nuget.exe"

# Download NuGet
$NugetDir = Split-Path "$NugetExe" -Parent
if (!(Test-Path "$NugetDir")) {
    mkdir $nugetDir > $null
}

if (!(Test-Path "$NugetExe")) {
    # Enable TLS1.2 for WebClient
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor [System.Net.SecurityProtocolType]::Tls12 -bor [System.Net.SecurityProtocolType]::Tls13
    (New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/v${NugetVersion}/nuget.exe", $NugetExe)
}

# Install build packages
dotnet tool install --global Cake.Tool

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
dotnet cake --verbosity Minimal "${ScriptFile}" ${cakeArgs} ${RemainingArgs}
exit $LASTEXITCODE
