<#
.SYNOPSIS
Downloads and Installs all available Microsoft Patches
.DESCRIPTION
Uses Microsoft.Update COM Object methods to search for available updates, download, and install them.
.NOTES
  Version:        1.1
  Author:         https://github.com/alex-tester
  Creation Date:  2020.05.28
  Purpose/Change: Initial public upload
  -Need to implement vmware tools
  -Need to enforce end time limit (outside of scheduled task limit)
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
#Set reboot to false
$RebootWhenFinished = $false

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

            #Check if server patched within past 60 mins - quit if so
            $RecentExec = Invoke-RestMethod -Method Get -Uri "$ApiUrl/GetMostRecentExecutionByHostName/$($env:COMPUTERNAME)"

            $RecentExecTime = (Get-Date $RecentExec.createdOn)

            $ExecTooSoon = ((Get-Date).AddMinutes(-60) -lt $RecentExecTime)

            if ($ExecTooSoon)
            {
                Exit #Byeeeeeeeee
            }
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
        #if (($NextPatchDate -lt (Get-Date).AddMinutes(30)) -and ($NextPatchDate -gt (Get-Date).AddMinutes(30)))
        if (($NextPatchDate -le (Get-Date).AddMinutes(15)) -and ($NextPatchDate -gt (Get-Date).AddMinutes(-15)))
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

        #Pre-Patch Script Logic 
        if ($HostRecord.enablePrePatchScript)
        {
            
            if ($null -ne $HostRecord.prePatchScript)
            {
                $preScript = [scriptblock]::Create($HostRecord.prePatchScript)
                Invoke-Command -ScriptBlock {. $preScript}
            }

        }

        $InstallResult = Install-ObsAutoMicrosoftPatches -IncludeDrivers $true


        $Results = Get-ObsAutoPatchResults -ExecutionId $Execution.id -ServerId $HostRecord.id

        foreach ($r in $Results)
        {

            $ReqBody = $r | ConvertTo-Json
            #update results table
            Invoke-RestMethod -Method post -Uri $ApiUrl/PatchingResults -Body $ReqBody -ContentType "application/json"

        }

        
        #Post-patch script logic (should add this as scheduled task after reboot)
        if ($HostRecord.enablePostPatchScript)
        {
            
            if ($null -ne $HostRecord.postPatchScript)
            {
                $postScript = [scriptblock]::Create($HostRecord.postPatchScript)
                Invoke-Command -ScriptBlock {. $postScript}
            }

        }

    }

    

    #Check if reboot required
    $RebootRequired = $InstallResult.RebootRequired

    #Check if reboot allowed
    $RebootAllowed = $HostRecord.rebootAfterPatch

    if ($RebootRequired -and $RebootAllowed)
    {
        $RebootWhenFinished = $true
    }


    #Update vmware tools if configured
    if ($HostRecord.updateVmwareTools)
    {
        #get vm tools installer location
        $PatchInstallSettings = Invoke-RestMethod -Method Get -Uri "$ApiUrl/getpatchinginitializationsettings"
        $vmToolsPath = ($PatchInstallSettings | Where-Object {$_.key -eq "VMwareToolsInstallPath"}).value
        $vmToolsLocalPath = "C:\temp\vmtoolssetup.exe"
        $vmToolsParam = '/S /v "/qn REBOOT=R"'

        #https://packages.vmware.com/tools/esx/6.7u3/windows/x64/
        Copy-Item -Path $vmToolsPath -Destination $vmToolsLocalPath -Confirm:$false -Force
        Unblock-File $vmToolsLocalPath -Confirm:$false
        Start-Process -FilePath $vmToolsLocalPath -ArgumentList $vmToolsParam -Wait
        Start-Sleep -Seconds 30
        Remove-Item $vmToolsLocalPath -Confirm:$false -Force

    }
}



if ($RebootWhenFinished)
{
    Restart-Computer -Confirm:$false -Force
}