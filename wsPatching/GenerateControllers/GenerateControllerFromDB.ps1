function Get-ScriptDirectory {
    if ($psise) {
        Split-Path $psise.CurrentFile.FullPath
    }
    else {
        $global:PSScriptRoot
    }
}

#Need to fix adding role section
$sqlScriptPath = Get-ScriptDirectory

$CurrentPath = Get-ScriptDirectory
$ParentPath = $(New-Object System.IO.DirectoryInfo $CurrentPath).Parent.FullName
$ControllerPath = "$ParentPath\Controllers\DatabaseControllers"
$ModelsPath = "$ParentPath\Models"



$datasource="SQL03.observicing.net"
$user="cc\" + $env:USERNAME
$database="PatchingAutomation"
$connectionString = “Server=$dataSource;uid=$user;Database=$database;Integrated Security=True;”
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = $connectionString
$word="TableName"
$connection.Open()
$query = “SELECT * FROM sys.tables WHERE name NOT LIKE 'sysdiag%'  ORDER BY name”
$command = $connection.CreateCommand()
$command.CommandText = $query

$result = $command.ExecuteReader()
$table = new-object “System.Data.DataTable”
$table.Load($result)

$TableNames =$table | Select-Object -Property name 

#Getting Current Controllers
$ControllerObjects = Get-ChildItem $ControllerPath | Select-Object -Property name


foreach($tName in $TableNames){
    
    $controllerFound = $false

    #uncomment this line and comment out the next line if the UseDatabaseNames switch is used in .netcore 2.1
    #$fixedname = $tName.name
    $fixedname = $tName.name.Replace("_","")

    foreach ($Controller in $ControllerObjects)
    {
        if ($Controller.Name.ToLower().ToString().Replace("controller.cs","") -eq $fixedname.ToString().ToLower())
        {
            $controllerFound = $true
        }
    }

    if ($controllerFound -eq $false)
    {
        $CrudTemplate = Get-Content -Path "$CurrentPath\CRUDTemplate.txt"

        
        $CrudFileValue = $CrudTemplate.Replace($word,$fixedname)
        

        "Creating Base CRUD Controller for $($fixedname)"
        $CrudFileValue | Out-File  -filepath "$CurrentPath\$($fixedname)Controller.cs"
    }
}