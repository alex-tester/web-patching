param(
    [Parameter(Mandatory = $true, ParameterSetName = 'ByList')]
    [ValidateNotNullOrEmpty()]
    $ComputerList,

    [Parameter(Mandatory = $true, ParameterSetName = 'ByFile')]
    [ValidateNotNullOrEmpty()]
    $ComputerListFile,

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [ValidateSet("cle-wsus01", "pit-wsus01")]
    $PatchServer,

    #Add to any security group required for WSUS
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [ValidateSet("GS-Cleveland-Servers", "GS-Pittsburgh-Servers")]
    $GPOAccessGroup,

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [switch]$DoNotChangeExecutionPolicy,

    [Parameter(Mandatory = $false, ParameterSetName = 'ByList')]
    [Parameter(Mandatory = $false, ParameterSetName = 'ByFile')]
    [Parameter(Mandatory = $false, ParameterSetName = 'OverrideApi')]
    [ValidateNotNullOrEmpty()]
    [switch]$OverrideApiUrl,

    [Parameter(Mandatory = $false, ParameterSetName = 'ByList')]
    [Parameter(Mandatory = $false, ParameterSetName = 'ByFile')]
    [Parameter(Mandatory = $true, ParameterSetName = 'OverrideApi')]
    [ValidateNotNullOrEmpty()]
    [string]$ApiUrl

<#
    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [switch]$AddDirectlyToGPO

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [switch]$PromptForCredentials#>

)


#temporarily forcing always prompt for credentials
$PromptForCredentials = $true

$ApiUrl = "http://dev-11/patchingautomation/api"



function clean-installenvironment{
    write-verbose "$computer :: Cleaning install environment. Removing pssessions and psdrives"
    $drivenames = @('connection', 'psremoteinstall')
    $drives = get-psdrive
    foreach ($drivename in $drivenames) {
        foreach ($drive in $drives) {
            if ($drive.name -eq $drivename) {
                Remove-PSDrive -Name $drivename -Force -confirm:$false
            }
        }
    }

    #Removing PSSessions
    get-pssession | remove-pssession -confirm:$false
}

clean-installenvironment

function Get-ScriptDirectory {
    if ($psise) {
        Split-Path $psise.CurrentFile.FullPath
    }
    else {
        $global:PSScriptRoot
    }
}


$CurrentDir = Get-ScriptDirectory

# load functions
. "$CurrentDir\patchingfunctions.ps1"



#Get Patch Installation Settings from API
try
{
    $PatchInstallSettings = Invoke-RestMethod -Method Get -Uri "$ApiUrl/getpatchinginitializationsettings"
}
catch
{
    throw "Failed to get patch settings from API"
}

if (-not $PatchInstallSettings)
{
    throw "Patch install settings not returned from API"
}

#Patch settings variables
$UseLocalPatchScript = [System.Convert]::ToBoolean(($PatchInstallSettings | Where-Object {$_.key -eq "UseLocalPatchScript"}).value)
$LocalPatchingPath = ($PatchInstallSettings | Where-Object {$_.key -eq "LocalPatchingPath"}).value
$PatchingNetworkShare = ($PatchInstallSettings | Where-Object {$_.key -eq "PatchingNetworkShare"}).value
$PatchingScriptName = ($PatchInstallSettings | Where-Object {$_.key -eq "PatchingScriptName"}).value
$ScheduledTaskName = ($PatchInstallSettings | Where-Object {$_.key -eq "ScheduledTaskName"}).value

$PatchingScriptTaskPath = $null
if ($UseLocalPatchScript)
{
    $PatchingScriptTaskPath = $LocalPatchingPath
}
else
{
    $PatchingScriptTaskPath = $PatchingNetworkShare
}

$FullPatchScriptPath = Join-Path -Path $PatchingScriptTaskPath -ChildPath $PatchingScriptName


$ScheduledTaskCreateDate = Get-Date -Format "yyyy-MM-dd"
$ScheduledTaskCreateTime = Get-Date -Format "hh:mm:ss"
$ScheduledTaskExecTime = Get-Date -Date ((Get-Date).AddMinutes(5)) -Format "hh:mm:ss" #First execution five minutes after installation
$FullyQualifiedUserName = $ENV:USERDOMAIN + "\" + $ENV:USERNAME

$PatchingScheduledTaskXml = Get-Content -Path "$CurrentDir\PatchingAutomationExecutorScheduledTask.xml"
$PatchingScheduledTaskXml = $PatchingScheduledTaskXml.replace("#TASKCREATEDATE#",$ScheduledTaskCreateDate)
$PatchingScheduledTaskXml = $PatchingScheduledTaskXml.replace("#TASKCREATETIME#",$ScheduledTaskCreateTime)
$PatchingScheduledTaskXml = $PatchingScheduledTaskXml.replace("#TASKEXECTIME#",$ScheduledTaskExecTime)
$PatchingScheduledTaskXml = $PatchingScheduledTaskXml.replace("#TASKAUTHOR#",$FullyQualifiedUserName)
$PatchingScheduledTaskXml = $PatchingScheduledTaskXml.replace("#PATCHSCRIPTFILE#",$FullPatchScriptPath)


$PSExecRemoteLoc = "$CurrentDir\psexec.exe"
$PSExecLocalLoc = "c:\temp"
$PSExec = "c:\temp\psexec.exe"
if(-not(test-path $PSExecLocalLoc))
{
    new-item -Path $PSExecLocalLoc -ItemType directory -Force
}
copy-item $PSExecRemoteLoc $PSExecLocalLoc -Force -Confirm:$false


#Import list of computers from file if specified
if ($ComputerListFile) {
    write-verbose "Importing computer list file"
    $computerlist = get-content $computerlistfile
}


#Set credentials to use for main connection to remote computer
$message = "Setting credentials for connection to remote computer"
write-verbose $message

if($PromptForCredentials)
{
    $cred = get-credential
    $user = $cred.UserName
    $pass = ($cred.GetNetworkCredential()).password
}
else
{

}

#Enabling credSSP on the local machine (running this script)
$enablecredsspclient = Enable-WSManCredSSP -Role client -DelegateComputer * -Force
if ($?) {
    write-verbose "CredSSP client enabled"
}
else {
    write-verbose "Cred-SSP client failed to enable"
}
$enablecredsspserver = Enable-WSManCredSSP -Role Server -Force
if ($?) {
    write-verbose "CredSSP server enabled"
}
else {
    write-verbose "Cred-SSP server failed to enable"
}




#Start looping through all the computers


$count = 0
write-verbose "Starting to loop through remote computers"
foreach ($computer in $ComputerList) {

write-host -ForegroundColor Yellow "Starting $computer... please wait."
$percent = ($count / ($computerlist | Measure-Object).count) *100
Write-Progress -Activity "Installing scheduled task on $Computer" -PercentComplete $percent -Status "$percent %"
$count ++

    $log = "$currentdir\logs\Installer\$computer.log"
    

    write-verbose "$computer :: Start"

    #Test connection
    $message = "$computer :: Testing connection"
    write-verbose $message
    
    $connection = Test-Connection -ComputerName $computer -ErrorAction SilentlyContinue
    if(!$connection)
    {
        $message = "$computer :: Not reachable. Moving on to next computer in list."
        write-verbose $message
        
        write-host $message -ForegroundColor DarkYellow
        clean-installenvironment
        continue
    }


        $message = "$computer :: Making general PSDrive connection"
        write-verbose $message
        
  
    $drive = New-PSDrive -Name connection -PSProvider FileSystem -Root \\$computer\c$ -Credential $cred
    if(!$?)
    {
        $message = "$computer :: Failed to make PSDrive connection. Most likely a rights issue with the remote machine."
        write-verbose $message
        write-host -ForegroundColor DarkYellow $message
        clean-installenvironment
        continue

    }

    
    

    #Set script execution policy
    if (-not($DoNotChangeExecutionPolicy)) {
            $message = "$computer :: Setting powershell execution policy to Bypass"
            write-verbose $message
            <#
            $Reg = [Microsoft.Win32.RegistryKey]::OpenRemoteBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, $computer) 
            $RegKey = $Reg.OpenSubKey("SOFTWARE\\Microsoft\\Powershell\\1\\ShellIds\\Microsoft.PowerShell", $true)
            $regkey.SetValue("ExecutionPolicy", "Bypass")
            #>
            Invoke-Command -ComputerName $computer -Credential $cred -ScriptBlock {
            $Reg = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64) 
            $RegKey = $Reg.OpenSubKey("SOFTWARE\\Microsoft\\Powershell\\1\\ShellIds\\Microsoft.PowerShell", $true)
            $regkey.SetValue("ExecutionPolicy", "Bypass")
            if ($?) 
            {
                $message = "$computer :: Successfully set execution policy to bypass"
                write-verbose $message
       
            }
            else 
            {
                $message = "$computer :: Failed to set execution policy to bypass"
                write-verbose $message 

            }
        }
    }



    #Check for enabled psremoting
    $message = "$computer :: Checking if PSRemoting is enabled"
    write-verbose $message
   

    #$s = new-pssession -ComputerName $computer -Credential $cred -Authentication Credssp -ErrorAction SilentlyContinue
    $s = new-pssession -ComputerName $computer -Credential $cred -ErrorAction SilentlyContinue
    if (!$?) 
    {
        $message = "$computer :: PSRemoting and/or CredSSP is not enabled"
        write-verbose $message
        

        $message = "$computer :: Creating PSDrive for PS Remote enable"
        write-verbose $message
        
 
        $drive = New-PSDrive -Name psremoteinstall -PSProvider FileSystem -Root \\$computer\c$ -Credential $cred

        $remotepath = "psremoteinstall:\temp"

        #copy files for PSRemoting enable
        if (!(test-path $remotepath)) 
        {
            $message = "$computer :: Creating directory $remotepath"
            write-verbose $message
       
         
            new-item -Path $remotepath -ItemType directory -Force -Confirm:$false
        }
        copy-item -path "$CurrentDir\enablepsremoting.ps1" -Destination $remotepath
        #Copy-Item -Path "$CurrentDir\initiatepsremoting.bat" -Destination $remotepath


        $message = "$computer :: Removing psremoteinstall PSDrive"
        write-verbose $message


        
        Get-PSDrive psremoteinstall | Remove-PSDrive -Force -confirm:$false

        #enable PSRemoting
        
        $message = "$computer :: Using PSExec to enable PSRemoting"
        write-verbose $message
       
        start-process -FilePath $PSExec -ArgumentList "-accepteula -h -s \\$computer c:\temp\initiatepsremoting.bat" -Wait

        #test again

        $message = "$computer :: Re-testing PSRemoting"
        write-verbose $message
      

        #No longer using credssp
        #$s = new-pssession -ComputerName $computer -Credential $cred -Authentication Credssp
        $s = new-pssession -ComputerName $computer -Credential $cred 
        if (!$?) 
        {
            $message = "$computer :: PSRemoting failed to enable. No more work can be done for this computer."
            write-verbose $message
           
     
            clean-installenvironment
            continue
        }
        else 
        {
            $message = "$computer :: PSRemoting was successfully enabled"
            write-verbose $message

           
        }
    }
    else 
    {
        $message = "$Computer :: PSRemoting is already enabled"
        write-verbose $message


    }

    <#
    No longer enabling credssp unless necessary
    #make CredSSP Connection
    $message = "$computer :: making CredSSP PSRemote session"
    write-verbose $message
 
 

    $s = New-PSSession -Authentication Credssp -ComputerName $Computer -Credential $cred
    if ($?) 
    {
        $message = "$computer :: PSRemote CredSSP session successfully created"
        write-verbose $message
        Remove-PSSession $s
       

    }
    else 
    {
        $message = "$computer :: Failed to create PSRemote CredSSP session. No more work can be done for this computer"
        write-verbose $message
      

        clean-installenvironment
        continue
    }

    #>
    $drive = New-PSDrive -Name psremoteinstall -PSProvider FileSystem -Root \\$computer\c$ -Credential $cred

    $remotepath = "psremoteinstall:\temp"

    #copy script files and execute locally 
    if ($UseLocalPatchScript)
    {
        $localInstalldrive = New-PSDrive -Name localinstall -PSProvider FileSystem -Root "\\$($computer)\$($LocalPatchingPath.Substring('','1'))`$" -Credential $cred

        $localInstallPsDrive = "localinstall:"

        $localInstallParsedPath = Join-Path -Path $localInstallPsDrive -ChildPath $LocalPatchingPath.Substring('3')

        if (!(test-path $localInstallParsedPath)) 
        {
            $message = "$computer :: Creating directory $localInstallParsedPath"
            write-verbose $message
       
         
            $newDirOutput = new-item -Path $localInstallParsedPath -ItemType directory -Force -Confirm:$false
        }

        Copy-Item -Path "$CurrentDir\patch-initiator.ps1" -Destination $localInstallParsedPath -Force -Confirm:$false
        Copy-Item -Path "$CurrentDir\patch-runner.ps1" -Destination $localInstallParsedPath -Force -Confirm:$false
        Copy-Item -Path "$CurrentDir\patchingfunctions.ps1" -Destination $localInstallParsedPath -Force -Confirm:$false

        Get-PSDrive $localInstalldrive | Remove-PSDrive

    }

    #copy files for PSRemoting enable
    if (!(test-path $remotepath)) 
    {
        $message = "$computer :: Creating directory $remotepath"
        write-verbose $message
       
         
        $NewRemoteDirOutput = new-item -Path $remotepath -ItemType directory -Force -Confirm:$false
    }

    
    $RemoteXmlFile = "$remotepath\PatchingAutomationExecutor.xml"


    $PatchingScheduledTaskXml | Out-File $RemoteXmlFile -Force #-Encoding ascii

    Invoke-Command -ComputerName $computer -Credential $cred -ScriptBlock {

    $tasks = Get-ScheduledTask

    $SchTaskExists = $tasks | Where-Object {$_.taskname -eq $using:ScheduledTaskName}

    if ($SchTaskExists)
    {
        Unregister-ScheduledTask -InputObject $SchTaskExists -Confirm:$false
    }

    #Should convert to register-scheduledtask
    schtasks.exe /create /XML "C:\temp\PatchingAutomationExecutor.xml" /RU SYSTEM /TN $using:ScheduledTaskName /F

    try
    {
        $userString = $ENV:USERDOMAIN + "%%" + $ENV:USERNAME
        Write-Verbose "calling url: $using:ApiUrl/GetOrCreateServerRecordByHostName/$($ENV:COMPUTERNAME)/$userString"
        $PopulateDatabaseCommand = Invoke-RestMethod -Method Get -Uri "$using:ApiUrl/GetOrCreateServerRecordByHostName/$($ENV:COMPUTERNAME)/$userString"
        Write-Verbose "Successfully created or located server in database"
    }
    catch
    {
        Write-Warning "Failed creating new server in database"
    }

    }

    #post work cleanup
    $filesToCleanup = @("PatchingAutomationExecutor.xml", "enablepsremoting.ps1", "initiatepsremoting.bat")

    foreach ($file in $filesToCleanup)
    {
        $remoteFileToClear = Join-Path -Path $remotepath -ChildPath $file

        if ((test-path $remoteFileToClear)) 
        {
            $message = "$computer :: Removing temp file $remoteFileToClear"
            write-verbose $message
       
         
            Remove-Item -Path $remoteFileToClear -Confirm:$false -Force
        }

    }

    Get-PSDrive psremoteinstall | Remove-PSDrive -Force -confirm:$false

     <#

    #Stop wuauserv service on server
    $message = "$computer :: Stopping wuauserv service"
    write-verbose $message
   
    
     
    Invoke-Command -Session $s -ScriptBlock {stop-service wuauserv -Force -Confirm:$false}
    if(!$?)
    {
        $message = "$computer :: Failed to stop wuauserv service"

        write-verbose $message
       

        Write-host -ForegroundColor red -Object $message

    }


    #Delete reg entries with GUIDs
        $message = "$computer :: Deleting WSUS Guid registry entries"
        write-verbose $message


    invoke-command -Session $s -ScriptBlock {
        $regkey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate"
        $properties = @('PingID', "AccountDomainSid", "SusClientId", "SusClientIDValidation")
        foreach ($item in $properties) {
            $removereg = Remove-ItemProperty -Path $regkey -Name $item -Force -Confirm:$false -ErrorAction SilentlyContinue | out-null
        }

    }


   

    #Enable server in group policy

    #Adding to group
    $message = "$computer :: Getting domain credentials for group policy work"
    write-verbose $message

    $domaincred = $cred


    $message = "$computer :: Deleting WSUS Guid registry entries"
    write-verbose $message
    Add-EseAutoPatchLogEntry -log $log -text $message



    $message = "$computer :: Checking if already in group"
    write-verbose $message

 
    $groupmembers = Get-ADGroupMember -Credential $domaincred -Identity $GPOAccessGroup
    $member = $null
    $member = $groupmembers | where {$_.name -eq $computer}
    if ($member) {
        
       $message = "$computer :: Already a member of the AD group $GPOAccessGroup"
        write-verbose $message


   

    }
    else {

           $message = "$computer :: Adding to $GPOAccessGroup to allow access to WSUS group policy"
        write-verbose $message
       


        $ADComputer = Get-ADComputer $Computer -Credential $domaincred     
        $groupData = Get-ADGroup $GPOAccessGroup -Credential $domaincred
        $addToGroup = Add-ADGroupMember -Identity $groupData -Members $ADComputer -Credential $cred


    }

    #Wait for Group policy/AD/group to replicate
    $timetowait = 60
    $message = "$computer :: Pausing $timetowait seconds to allow for AD replication"

    write-verbose $message
    start-sleep -Seconds $timetowait

    #Refresh local kerberos tickets to allow GPO to apply
    $message = "$computer :: Refreshing Kerberos tickets"

    write-verbose $message

    Invoke-Command -Session $s -ScriptBlock {cmd /c "klist -li 0x3e7 purge" |out-null}
    if(!$?)
    {
        $message = "$computer :: Failed to refresh kerberos tickets"
        
        write-verbose $message

    }
    else
    {
        $message = "$computer :: Successfully refreshed kerberos tickets"
     
        write-verbose $message
    }



    #Run gpupdate remotely
    $message = "$computer :: Running gpupdate /force"
   
    write-verbose $message

    
    $gpupdate = Invoke-Command -ComputerName $Computer -Credential $cred -ScriptBlock {cmd /c gpupdate /force}
    $gpupdate = Invoke-Command -ComputerName $Computer -Credential $cred -ScriptBlock {cmd /c gpupdate /force}





    #start wuauserv
    $message = "$computer :: Starting Wuauserv service"
 
    write-verbose $message
    Invoke-Command -Session $s -ScriptBlock {start-service wuauserv -Confirm:$false}

    #reset authorization token
    $message = "$computer :: Resetting WSUS authorization token"
   
    write-verbose $message
    invoke-command -Session $s -ScriptBlock {cmd /c wuauclt.exe /resetauthorization}

    #force report in to console
    $message = "$computer :: Forcing report in to console using PSRemoting"
    Add-EseAutoPatchLogEntry -log $log -text $message
    write-verbose $message
    


    #Forcing report in using psexec (works more often)

    $message = "$computer :: Invoking scannow.ps1 using PSExec to attempt to force report in to console"
  
    write-verbose $message

    if (!(test-path -Path "\\$computer\c$\temp")) {
        write-verbose "$computer :: Creating c:\temp"
        new-item -Path "\\$computer\c$\temp" -ItemType directory -Force
    }

    write-verbose "$computer :: Creating script to initiate scannow"
    "echo . | powershell.exe \\$patchserver\scripts$\ScanNow.ps1" | out-file -Encoding ascii -Force "\\$computer\c$\temp\initiatescannow.bat"
        
    write-verbose "$computer :: Starting scannow using PSExec with highest authentication"
    start-process -FilePath $PSExec -ArgumentList "-accepteula -h -s \\$computer c:\temp\initiatescannow.bat" -Wait



    #Force report in using the wuauclt.exe 
    $message = "$computer :: Trying to make WU agent report in using wuauclt.exe"
   
    write-verbose $message
    invoke-command -Session $s -ScriptBlock {cmd /c wuauclt.exe /detectnow /reportnow}



    #Check for existence in console
    start-sleep 20
        $message = "Getting wsus server $patchserver on port 8530"
  
    write-verbose $message

    
    $wsusserverobject = Get-WsusServer -Name $patchserver -PortNumber 8530
    $wsuscomputerlist = Get-WsusComputer -UpdateServer $wsusserverobject -All

    #"Computer list:"
    #$wsuscomputerlist

    $computeradded = $false
    foreach ($item in $wsuscomputerlist) {
        if ($item.FullDomainName.contains($computer)) {
            $computeradded = $true
            
        }
    }

    if ($computeradded) {
            $message = "$computer added successfully"
  
    write-verbose $message

        write-host -ForegroundColor Green -Object $message

    }
    else {


                $message = "$Computer is not showing in the console yet! Please allow an hour and check the WSUS console to see if it added"
    
    write-verbose $message

        write-host -ForegroundColor red -Object $message
    }
    #Removing PSDrive connections

                    $message = "$Computer is not showing in the console yet! Please allow an hour and check the WSUS console to see if it added"
  
    write-verbose $message


    $message = "$Computer :: Removing PSDrive connections and PSSessions for this server"
  
    write-verbose $message

  


    clean-installenvironment

    #>




}

Remove-Item $PSExec -Force -Confirm:$false

