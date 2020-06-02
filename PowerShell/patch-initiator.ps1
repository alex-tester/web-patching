function Get-ScriptDirectory {
    if ($psise) {
        Split-Path $psise.CurrentFile.FullPath
    }
    else {
        $global:PSScriptRoot
    }
}

$ApiUrl = "http://dev-11/patchingautomation/api"

$CurrentDir = Get-ScriptDirectory

# load functions
. "$CurrentDir\patchingfunctions.ps1"

. "$CurrentDir\patch-runner.ps1" -ApiUrl $ApiUrl