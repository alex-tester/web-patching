
function Install-ObsAutoMicrosoftPatches
{
    param( 
    [Parameter(Mandatory=$true)]
    [bool]$IncludeDrivers
    )
    $NoPatches = $false
    $DriverCriteria = " or Type='Driver'"

    #Define update criteria
    $Criteria = "IsInstalled=0 and Type='Software'"

    if ($IncludeDrivers)
    {
        $Criteria += $DriverCriteria
    }

    #Search for relevant updates
    $Searcher = New-Object -ComObject Microsoft.Update.Searcher

    $SearchResult = $Searcher.Search($Criteria).Updates

    #If no patches are returned, initiate a check in, then try again after 5 minutes
    if($SearchResult.count -eq 0)
    {
        #because most companies have wsus
        wuauclt /reportnow
        start-sleep -Seconds 300

        #Search again

        $Searcher = New-Object -ComObject Microsoft.Update.Searcher

        $SearchResult = $Searcher.Search($Criteria).Updates 
    }


    if($SearchResult.count -gt 0)
    {
       
        foreach($item in $SearchResult)
        {
            #$KbNumberRgx = "(KB\d+)"
            #log the patches to be installed
            Write-Output "Preparing to install $item.Title"
        }

    }
    else
    {
        $NoPatches = $true
    }


    if(!$NoPatches)
    {

        #Download updates.
        $Session = New-Object -ComObject Microsoft.Update.Session

        $Downloader = $Session.CreateUpdateDownloader()

        $Downloader.Updates = $SearchResult

        $DownloadResult = $Downloader.Download()


        #Install updates

        $Installer = New-Object -ComObject Microsoft.Update.Installer

        $Installer.Updates = $SearchResult

        $InstallResult = $Installer.Install()
        #script will wait for installation to complete before continuing

        if($InstallResult.HResult -eq 0)
        {
            Write-Output "Patches installed successfully"
        }
        else
        {
            Write-Output "Patches failed to install with error code $InstallResult.HResult"
        }
        
    }
    else
    {
    Write-Output "No updates available for installation"
    }

return $installresult
}

function Search-ObsAutoMicrosoftPatches
{
    param(
    [bool]$IncludeDrivers
    )
    #$NoPatches = $false
    $DriverCriteria = " or Type='Driver'"


    

    #Define update criteria
    $Criteria = "IsInstalled=0 and Type='Software'"
    if ($IncludeDrivers)
    {
        $Criteria += $DriverCriteria
    }
    #Search for relevant updates
    $Searcher = New-Object -ComObject Microsoft.Update.Searcher

    $SearchResult = $Searcher.Search($Criteria).Updates

    return $SearchResult
}

function Download-ObsAutoMicrosoftPatches-Broken
{
    param( 
    [Parameter(Mandatory=$true)]
    $SearchResult
    )
    #Download updates.
    $Session = New-Object -ComObject Microsoft.Update.Session

    $Downloader = $Session.CreateUpdateDownloader()

    $Downloader.Updates = $SearchResult

    $DownloadResult = $Downloader.Download()
}

function Install-ObsAutoMicrosoftPatches-Broken
{
    param( 
    [Parameter(Mandatory=$true)]
    $SearchResult
    )
    $Installer = New-Object -ComObject Microsoft.Update.Installer

    $Installer.Updates = $SearchResult

    $InstallResult = $Installer.Install()
    #script will wait for installation to complete before continuing

    if($InstallResult.HResult -eq 0)
    {
        Write-Output "Patches installed successfully"
    }
    else
    {
        Write-Output "Patches failed to install with error code $InstallResult.HResult"
    }

    return $InstallResult
}

function Log-ObsAutoAvailablePatches
{
    param( 
    [Parameter(Mandatory=$true)]
    $ApiUrl,
    [Parameter(Mandatory=$true)]
    $ExecutionId,
    [Parameter(Mandatory=$true)]
    $ServerId,
    [Parameter(Mandatory=$true)]
    $PatchesAvailable
    )

    if (($PatchesAvailable | Measure-Object).Count -gt 0)
    {
        
        $KbNumberRgx = "(KB\d+)"

        #$r = ([regex]'([^\\]+$)').matches($MyInvocation.ScriptName)
        foreach ($p in $PatchesAvailable)
        {
            $KbNumber = "unknown"
            $r = ([regex]$KbNumberRgx).Matches($p.title)
            if ($r.value) { $KbNumber = $r.value }

            $AvailablePatchLog = $null
            $AvailablePatchLog = @{
                ServerId = $ServerId
                PatchingExecutionId = $ExecutionId
                KbNumber = $KbNumber
                Title = $p.Title
            } | ConvertTo-Json
            #suppress output
            Invoke-RestMethod -Method Post -Uri $ApiUrl/PatchingAvailablePatches -Body $AvailablePatchLog -ContentType "application/json"
            #$AvailablePatchLog

        }
    }

}




function Convert-ObsAutoPatchResultCodeToName
{
    param( [Parameter(Mandatory=$true)]
    [int] $ResultCode
    )
    switch($ResultCode)
    {
        0
        {
        $Result = "Not Started"
        }
        1
        {
        $Result = "Pending Reboot"
        }
        2
        {
        $Result = "Succeeded"
        }
        3
        {
        $Result = "Succeeded With Errors"
        }
        4
        {
        $Result = "Failed"
        }
        5
        {
        $Result = "Canceled"
        }
    }
    return $Result
}

function Get-ObsAutoPatchResults
{

    param( 
    [Parameter(Mandatory=$true)]
    $ExecutionId,
    [Parameter(Mandatory=$true)]
    $ServerId
    )

    $HotFixes = Get-HotFix | Where-Object {$_.InstalledOn -ge (Get-Date).AddHours(-24)}
    $session = New-Object -ComObject Microsoft.Update.Session
    #$search  = $session.CreateUpdateSearcher()
    #$result = $search.Search('IsInstalled=1')
    #Query last 1000 installed patches
    $HistoryFromCom = $session.QueryHistory("",0,1000)
    #$PatchesFromCom = $result.Updates

    $Output = @()
    foreach ($hotfix in $hotfixes)
    {
        $KBTitle = $null
        $Result = $null
        $Details = @()

        #Check patch history for KB Title
        $KBFromGetHotFix = $hotfix.HotFixID
        foreach ($History in $HistoryFromCom)
        {
            $KBFromCom = [regex]::Match($History.Title,'(KB[0-9]{6,7})').value
            if ($KBFromGetHotFix -eq $KBFromCom)
            {
                $KBTitle = $History.Title
                $Result = Convert-ObsAutoPatchResultCodeToName -ResultCode $History.ResultCode
                Break
            }
        }

        #Fall back to description from get-hotfix
        if (!$KBTitle)
        {
            $KBTitle = $HotFix.Description
        }
        #default to succeeded if no results are found. It won't list in Get-Hotfix if it wasn't successful.
        #The goal is to identify patches with pending reboot status.
        if (!$Result)
        {
            $Result = "Succeeded"
        }


        $Details = @{
        Title = $KBTitle
        KbNumber = $HotFix.HotFixID
        Status = $Result
        PatchingExecutionId = $ExecutionId
        ServerId = $ServerId
        }
    
        $output += New-Object -TypeName PSObject -Property $Details
    }
    return $Output
}
