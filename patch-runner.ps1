<#
.SYNOPSIS
Downloads and Installs all available Microsoft Patches
.DESCRIPTION
Uses Microsoft.Update COM Object methods to search for available updates, download, and install them.
.EXAMPLE
Install-MicrosoftPatches
#>
param(
[switch]$IgnoreSchedule,
[Parameter(Mandatory = $true)]
[ValidateNotNullOrEmpty()]
[string]$ApiUrl
)

$ValidatePatchSchedule = $false
$InstallPatches = $true


#$ApiUrl = "http://api.observicing.net/api" #TODO publish to "production"



$HostRecord = Invoke-RestMethod -Method Get -Uri "$ApiUrl/GetNextPatchDateByHostName/$($env:COMPUTERNAME)"


#verify the api returned a object valid
$success = ($null -ne $HostRecord)


if (!$IgnoreSchedule)
{
    if ($success)
    {
        #verify next occurrence is valid datetime
        #$PatchSchedule = Invoke-RestMethod -Method Get -Uri "$ApiUrl/Scheduling/$($HostRecord.id)/GetPatchingSchedule"

        try
        {
            $success = (Get-Date $HostRecord.occurrences[0]).GetType() -eq [System.DateTime]
        }
        catch [System.Management.Automation.RuntimeException] #not a valid date object
        {
            $success = $false
        }
    }
}



if ($success)
{
    if (!$IgnoreSchedule)
    {
        #Get the first occurence in the schedule
        #$NextPatchDate = (Get-Date $PatchSchedule.occurrences[0])
        $NextPatchDate = (get-date $HostRecord.nextPatchDate).ToUniversalTime()


        #Verify we're executing within 30 minutes of the patching window
        if (($NextPatchDate -lt (Get-Date).AddMinutes(30)) -and ($NextPatchDate -gt (Get-Date).AddMinutes(30)))
        {
            $InstallPatches = $true 
        }
        else
        {
            #TODO log here - furthermore: TODO pick a new logging platform
            $success = $false
        }
    }
    else
    {
        $InstallPatches = $true
    }
}



if ($success -and $InstallPatches)
{
    $PatchExecutionBody = @{
    ServerId = $HostRecord.id
    } | ConvertTo-Json
    #TODO record additional info in the execution table - total available, total completed, end time
    #### now including list of available patches associated with execution id
    $Execution = Invoke-RestMethod -Method Post -Uri "$ApiUrl/PatchingExecution" -Body $PatchExecutionBody -ContentType "application/json"
    $success = $null -ne $Execution
}

if ($success -and $InstallPatches)
{
    #possibly return the hresult of this function to executionrecord



    #Install-MicrosoftPatches


    $PatchesAvailable = Search-ObsAutoMicrosoftPatches -IncludeDrivers $true

    $PatchesAvailableCount = ($PatchesAvailable | Measure-Object).Count

    if ($PatchesAvailableCount -gt 0)
    {
        Log-ObsAutoAvailablePatches -ApiUrl $ApiUrl -ExecutionId $Execution.id -ServerId $HostRecord.id -PatchesAvailable $PatchesAvailable

        #$DownloadResult = Download-ObsAutoMicrosoftPatches -SearchResult $PatchesAvailable


        #$InstallResult = Install-ObsAutoMicrosoftPatches -SearchResult $PatchesAvailable

        $InstallResult = Install-ObsAutoMicrosoftPatches -IncludeDrivers $true


        $Results = Get-ObsAutoPatchResults -ExecutionId $Execution.id -ServerId $HostRecord.id

        foreach ($r in $Results)
        {

            $ReqBody = $r | ConvertTo-Json
            #update results table
            Invoke-RestMethod -Method post -Uri $ApiUrl/PatchingResults -Body $ReqBody -ContentType "application/json"

        }
    }
}
