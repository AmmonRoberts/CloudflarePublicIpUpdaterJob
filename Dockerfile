# Use the official .NET SDK image to build the app.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./

RUN dotnet restore
RUN dotnet build --no-restore -c Release

# Publish the application.
RUN dotnet publish --no-restore -c Release -o /out

# Use the official .NET runtime image.
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Copy the published output from the build stage.
COPY --from=build /out /app

WORKDIR /app

# Set the entry point for the application.
ENTRYPOINT ["dotnet", "CloudflarePublicIpUpdaterJob.dll"]