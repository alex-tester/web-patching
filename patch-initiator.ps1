function Get-ScriptDirectory {
    if ($psise) {
        Split-Path $psise.CurrentFile.FullPath
    }
    else {
        $global:PSScriptRoot
    }
}

$ApiUrl = "http://localhost/patchingautomation/api"

$CurrentDir = Get-ScriptDirectory

# load functions
. "$CurrentDir\patchingfunctions.ps1"

. "$CurrentDir\patch-runner.ps1" -IgnoreSchedule -ApiUrl $ApiUrl