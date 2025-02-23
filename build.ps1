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
$RootDir            = "${PSScriptRoot}"
$ScriptFile         = "${RootDir}/build/Build.csproj"
$BuildDir           = "${RootDir}/_build"
$ToolsDir           = "${BuildDir}/tools"

# Build args
$cakeArgs = @()

if ($Arg0) {
    if ($Arg0[0] -eq "-") {
        $cakeArgs += "${Arg0}"
    } else {
        $cakeArgs += "--target=${Arg0}"
    }
}

# Run Cake
dotnet run --project "${ScriptFile}" -- ${cakeArgs} ${RemainingArgs}
exit $LASTEXITCODE