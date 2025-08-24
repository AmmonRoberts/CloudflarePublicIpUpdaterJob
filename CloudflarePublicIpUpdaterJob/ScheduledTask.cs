using CloudflarePublicIpUpdaterJob.Clients;
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

	public ScheduledTask(ILogger<ScheduledTask> logger, IOptions<JobSettings> jobSettings, ICloudflareClient cloudflareClient, IPublicIpClient publicIpClient)
	{
		_logger = logger;
		_jobSettings = jobSettings.Value;
		_cloudflareClient = cloudflareClient;
		_publicIpClient = publicIpClient;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogDebug("Timed Hosted Service running.");

		await CheckAndUpdatePublicIp();

		using PeriodicTimer timer = new(TimeSpan.FromSeconds(_jobSettings.TimerScheduleSeconds));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				_logger.LogInformation("Starting timer. Next job execution will be in {Seconds} seconds at {NextSceheduledTime}.", _jobSettings.TimerScheduleSeconds, DateTime.UtcNow.AddSeconds(_jobSettings.TimerScheduleSeconds));

				await CheckAndUpdatePublicIp();
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "An error ocurred while attempting to check and/or update public IP address. Timed Hosted Service is stopping.");

			throw;
		}
	}

	private async Task CheckAndUpdatePublicIp()
	{
		int count = Interlocked.Increment(ref _executionCount);

		_logger.LogDebug("Scheduled job has run {JobCount} times.", count);

		try
		{
			var response = await _cloudflareClient.GetADnsRecordsAsync("68b52b0bd16901afb03de0fda9634419");

			var publicIpResponse = await _publicIpClient.GetPublicIpJson();

			var ipRegex = new Regex(@"^(\d{1,3}\.){3}\d{1,3}$");

			foreach (var record in response.Result)
			{
				if (ipRegex.IsMatch(record.Content!) && record.Content != publicIpResponse.YourFuckingIPAddress)
				{
					_logger.LogInformation("Found an A DNS record with an IP address value that doesn't match the current public IP address.");

					var patchRequest = new UpdateDnsRecordRequest
					{
						Content = publicIpResponse.YourFuckingIPAddress,
					};

					await _cloudflareClient.UpdateADnsRecordsAsync("68b52b0bd16901afb03de0fda9634419", record.Id!, patchRequest);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while updating DNS records.");

			throw;
		}
	}
}