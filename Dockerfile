# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet build --no-restore -c Release

# Copy the rest of the source code and build the project
RUN dotnet publish --no-restore -c Release -o /out

# Use the official Azure Functions runtime base image for isolated .NET
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS runtime

# Copy the published output from the build stage
COPY --from=build /out /home/site/wwwroot
COPY --from=build /app/CloudflarePublicIpUpdaterJob/host.json /home/site/wwwroot

ENV AzureWebJobsScriptRoot=/home/site/wwwroot

# Set the function entry point (optional if FunctionApp is set correctly)
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true