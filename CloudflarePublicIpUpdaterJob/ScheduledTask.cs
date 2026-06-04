using CloudflarePublicIpUpdaterJob.Clients;
using CloudflarePublicIpUpdaterJob.Configuration;
using CloudflarePublicIpUpdaterJob.Models.Cloudflare;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace CloudflarePublicIpUpdaterJob;

public class ScheduledTask : BackgroundService
{
	private readonly ILogger<ScheduledTask> _logger;
	private readonly JobSettings _jobSettings;
	private readonly ICloudflareClient _cloudflareClient;
	private readonly IPublicIpClient _publicIpClient;

	private int _executionCount;
	private int _consecutiveFailures;
	private DateTime? _lastSuccessfulCheck;

	public ScheduledTask(ILogger<ScheduledTask> logger, IOptions<JobSettings> jobSettings, ICloudflareClient cloudflareClient, IPublicIpClient publicIpClient)
	{
		_logger = logger;
		_jobSettings = jobSettings.Value;
		_cloudflareClient = cloudflareClient;
		_publicIpClient = publicIpClient;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Timed Hosted Service starting with {Seconds}s interval.", _jobSettings.TimerScheduleSeconds);

		await CheckAndUpdatePublicIp();

		using PeriodicTimer timer = new(TimeSpan.FromSeconds(_jobSettings.TimerScheduleSeconds));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				_logger.LogInformation("Starting scheduled check. Next execution at {NextScheduledTime} UTC.", DateTime.UtcNow.AddSeconds(_jobSettings.TimerScheduleSeconds));

				await CheckAndUpdatePublicIp();

				if (_consecutiveFailures > 0)
				{
					_logger.LogWarning("Health: {ConsecutiveFailures} consecutive failures. Last success: {LastSuccess}.", _consecutiveFailures, _lastSuccessfulCheck?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never");
				}
			}
		}
		catch (OperationCanceledException e)
		{
			_logger.LogInformation(e, "Timed Hosted Service is shutting down gracefully.");
		}
		catch (Exception e)
		{
			_logger.LogError(e, "An error occurred while attempting to check and/or update public IP address. Timed Hosted Service is stopping.");

			throw;
		}
	}

	private async Task CheckAndUpdatePublicIp()
	{
		int count = Interlocked.Increment(ref _executionCount);

		_logger.LogDebug("Scheduled job execution #{JobCount}.", count);

		try
		{
			var response = await _cloudflareClient.GetADnsRecordsAsync("68b52b0bd16901afb03de0fda9634419");

			var publicIpResponse = await _publicIpClient.GetPublicIpJson();

			var ipRegex = new Regex(@"^(\d{1,3}\.){3}\d{1,3}$");

			bool recordsUpdated = false;

			foreach (var record in response.Result)
			{
				if (ipRegex.IsMatch(record.Content!) && record.Content != publicIpResponse.YourFuckingIPAddress)
				{
					_logger.LogInformation("DNS record {RecordId} needs update: {OldIp} -> {NewIp}.", record.Id, record.Content, publicIpResponse.YourFuckingIPAddress);

					var patchRequest = new UpdateDnsRecordRequest
					{
						Content = publicIpResponse.YourFuckingIPAddress,
					};

					await _cloudflareClient.UpdateADnsRecordsAsync("68b52b0bd16901afb03de0fda9634419", record.Id!, patchRequest);

					_logger.LogInformation("Successfully updated DNS record {RecordId}.", record.Id);

					recordsUpdated = true;
				}
			}

			if (!recordsUpdated)
			{
				_logger.LogDebug("All DNS records are current. Public IP: {PublicIp}.", publicIpResponse.YourFuckingIPAddress);
			}

			_consecutiveFailures = 0;
			_lastSuccessfulCheck = DateTime.UtcNow;
		}
		catch (Exception e)
		{
			_consecutiveFailures++;

			_logger.LogError(e, "Error during IP check/update (failure #{FailureCount}). Will retry on next scheduled run.", _consecutiveFailures);

			if (_consecutiveFailures >= 10)
			{
				_logger.LogWarning("High consecutive failure count ({FailureCount}). Check network connectivity and API credentials.", _consecutiveFailures);
			}
		}
	}
}