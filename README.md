# web-patching

**Synopsis:** This project is a template for automating the installation of Microsoft Updates against all currently supported versions of Windows written using the .NET Core 3.1 Framework. The basic components include:
- Web front end for managing patch schedules
- RESTful back end used by managed systems to ascertain if they are within a patch window
- PowerShell scripts for creating the database schema, installing the scheduled tasks on managed systems, and installing available patches
- Docker image available for deployment of the web services. 

This project requires a SQL Server instance.

All authorization/role configuration has been removed from the public version of this project for simplification, but you can easily adapt the code to use your favorite authorization provider.



# Configuration
## PowerShell
The scripts in the PowerShell folder should be copied to a central location which is accessible by DOMAIN COMPUTERS security principal if you want managed systems to use the centralized patching scripts for execution. 
### InstallPatchingAutomation.ps1
- Configure the $APIUrl variable in the script, or use the -OverrideApiUrl and -APIUrl parameters when performing an installation against a remote system. 
- The web service must be deployed before initiating a client installation so that the settings defined by the web service can be retrieved.
### patch-initiator.ps1
- The $APIUrl variable must be configured here as well.
## Web Service
### IIS
Configure appsettings.json for your environment. The most important options are the database connection string, "UseLocalPatchScript" to define whether the scripts are executed from a local or file share path, and the local/file share paths where the scripts will be located. The VMwareToolsInstallPath is optional and refers to the network share location where the VMware Tools Installer is located.
### Docker
The Docker image is available on DockerHub, or you can build your own.
[https://hub.docker.com/r/tester010/wspatching/](https://hub.docker.com/r/tester010/wspatching/)

You can get the image by executing **docker pull tester010/wspatching:release**

Modify the docker-environment-vars file from the **Docker** folder to define the application settings outlined above and start the container using the parameter **--env-file docker-environment-vars** to configure the settings for your environment.

By default the Docker image serves http requests on port 80, but can be overridden using standard ASPNETCORE environment variables.

# Deployment
Open the **CreateDatabaseSchema.ps1** file from the **PowerShell** folder, and configure the variables for your SQL Instance/Database path. Execute the script to create the database schema required by the application.

Deploy the application via IIS or Docker using the configuration options mentioned above, and test that the application settings are correct by issuing an HTTP GET request to http://[ApplicationBaseUrl]/api/GetPatchingInitializationSettings

The response should contain the options specified by appsettings.json or the environment variables provided to Docker.

Execute the **InstallPatchingAutomation.ps1** script and provide a parameter for **-Computerlist @("SERVER01","SERVER02")** or **-ComputerListFile \\Path\To\File.txt**

The scheduled task will be installed to computers and will either be executed from a local path, or file share depending on the settings configured in the web application.

Once the scheduled task is deployed to a server, it will be available in the web interface and you can configure a patch schedule and view the upcoming patch dates on the calendar.

