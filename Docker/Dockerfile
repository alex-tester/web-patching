FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app

COPY . /app

ENV ASPNETCORE_URLS="https://+443;http://+80"
#ENV ASPNETCORE_HTTPS_PORT=443
#ENV ASPNETCORE_Kestrel__Certificates__Default__Password=""
#ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/obs-conf/cert.pfx


ENV InstallationSettings__PatchingNetworkShare="Default image value - please replace with environment variable"
ENV InstallationSettings__UseLocalPatchScript=true
ENV InstallationSettings__LocalPatchingPath="C:\\temp\\nesteddir\\web-patching"
ENV InstallationSettings__PatchingScriptName="patch-initiator.ps1"
ENV InstallationSettings__ScheduledTaskName="Patching Automation"
ENV InstallationSettings__VMwareToolsInstallPath="i come from le dockerfile"

ENV ConnectionStrings__PatchingAutomationDb="Server=SQL03.observicing.net;Database=PatchingAutomation;User Id=patching_automation_db_service;Password=Changeme123#;"

#dotnet Obs.dll


#ENTRYPOINT /obs-conf/launch.sh

ENTRYPOINT ["dotnet", "wsPatching.dll", "--environment=Development"]


