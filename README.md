# Cloudflare Public IP Updater Job

This project is a .NET 8 console application that runs as a background service to automatically update Cloudflare DNS 'A' records with the current public IP address of the machine it's running on. This is useful for home servers or other devices on a dynamic IP address that need to be accessible via a stable domain name.

In this project, we use [Cloudflare's REST API](https://developers.cloudflare.com/pages/configuration/api/) to check and update DNS records, and we fetch the current public IP address using the best tool out there for getting your Public IP address: [wtfismyip.com](https://wtfismyip.com/). Why do we use such a childish and foul-mouthed service? Because it's hilarious.

## Features

- Automatically detects your current public IP address
- Updates all A records in your Cloudflare zone that have outdated IP addresses
- Configurable check interval
- Structured JSON logging with UTC timestamps
- Built as a long-running background service

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Configuration

The application is configured via the `appsettings.json` file. You will need to provide your Cloudflare API credentials and specify the zone you wish to monitor.

1.  Make a copy of `appsettings.example.json` and rename it to `appsettings.json`.
2.  Edit `appsettings.json` with your specific configuration:

    ```json
    {
      "CloudflareSettings": {
        "BaseAddress": "https://api.cloudflare.com/client/v4/",
        "ApiEmail": "your-cloudflare-email@example.com",
        "ApiKey": "your-cloudflare-global-api-key",
        "ZoneId": "your-cloudflare-zone-id"
      },
      "JobSettings": {
        "TimerScheduleSeconds": 300
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft": "Information",
          "Microsoft.Hosting.Lifetime": "Information"
        }
      }
    }
    ```

#### Configuration Options

*   **`ApiEmail`**: Your Cloudflare account email address.
*   **`ApiKey`**: Your Cloudflare Global API Key (found in your Cloudflare profile).
*   **`ZoneId`**: The Zone ID of the domain you want to monitor. You can find this on the "Overview" page for your domain in the Cloudflare dashboard.
*   **`TimerScheduleSeconds`**: The interval in seconds at which the application will check for IP address changes (default: 3600 seconds/1 hour).

⚠️ **Security Notes**: 
- Keep your API credentials secure and never commit `appsettings.json` to version control
- Add `appsettings.json` to your `.gitignore` file
- For production deployments, prefer environment variables or secure configuration management solutions

## Usage

To run the application, navigate to the `CloudflarePublicIpUpdaterJob` subdirectory and use the `dotnet run` command:

```sh
cd CloudflarePublicIpUpdaterJob
dotnet run
```

### Docker Deployment

To run the application in a Docker container:

1. Build the Docker image:
   ```sh
   docker build -t cloudflare-ip-updater .
   ```

2. Run the container with your appsettings.json file mounted as a volume:
   ```sh
   docker run -d --name ip-updater \
     -v /path/to/your/appsettings.json:/app/appsettings.json \
     cloudflare-ip-updater
   ```

### Application Behavior

The application will:
1. Start and immediately check your current public IP
2. Compare it against all A records in your specified Cloudflare zone
3. Update any A records that don't match your current public IP
4. Continue running and checking at the configured interval

The application will log its activity to the console with structured JSON formatting and UTC timestamps.

## How It Works

The service automatically:
- Fetches your current public IP address using wtfismyip.com
- Retrieves all DNS A records from your specified Cloudflare zone
- Compares each A record's IP address against your current public IP
- Updates any mismatched A records with your current IP address
- Schedules the next check based on your configured interval

## Built With

*   [.NET 8](https://dotnet.microsoft.com/) - The framework used
*   [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) - For running the application as a background service
*   [Refit.HttpClientFactory](https://www.nuget.org/packages/Refit.HttpClientFactory/) - Type-safe REST library for .NET, used for interacting with Cloudflare and public IP APIs

## Project Structure

```
CloudflarePublicIpUpdaterJob/
├── CloudflarePublicIpUpdaterJob/           # Main application directory
│   ├── Clients/                           # HTTP client interfaces
│   ├── Configuration/                     # Configuration classes
│   ├── Models/                           # Data models for API responses
│   ├── Program.cs                        # Application entry point
│   ├── ScheduledTask.cs                  # Main background service logic
│   ├── appsettings.example.json          # Configuration template
│   ├── appsettings.json                  # Application configuration (gitignored)
│   └── CloudflarePublicIpUpdaterJob.csproj
├── Dockerfile                            # Docker containerization
├── .gitignore                           # Git ignore patterns
└── README.md
```

## Configuration Files

- **`appsettings.example.json`**: Template showing the required configuration structure
- **`appsettings.json`**: Application configuration file (should be added to `.gitignore` to prevent credential leakage)


