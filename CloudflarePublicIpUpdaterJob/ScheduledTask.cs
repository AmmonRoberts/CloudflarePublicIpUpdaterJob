using CloudflarePublicIpUpdaterJob.Clients;
using CloudflarePublicIpUpdaterJob.Models.Cloudflare;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CloudflarePublicIpUpdaterJob;

public class ScheduledTask : BackgroundService
{
	private readonly ILogger<ScheduledTask> _logger;
	private readonly ICloudflareClient _cloudflareClient;
	private readonly IPublicIpClient _publicIpClient;

	private int _executionCount;

	public ScheduledTask(ILogger<ScheduledTask> logger, ICloudflareClient cloudflareClient, IPublicIpClient publicIpClient)
	{
		_logger = logger;
		_cloudflareClient = cloudflareClient;
		_publicIpClient = publicIpClient;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Timed Hosted Service running.");

		// When the timer should have no due-time, then do the work once now.
		await CheckAndUpdatePublicIp();

		using PeriodicTimer timer = new(TimeSpan.FromSeconds(15));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				await CheckAndUpdatePublicIp();
			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("Timed Hosted Service is stopping.");
		}
	}

	private async Task CheckAndUpdatePublicIp()
	{
		int count = Interlocked.Increment(ref _executionCount);

		_logger.LogError("Timed Hosted Service is working. Count: {Count}", count);


		try
		{
			var response = await _cloudflareClient.GetADnsRecordsAsync("68b52b0bd16901afb03de0fda9634419");

			var publicIpresponse = await _publicIpClient.GetPublicIpJson();

			var ipRegex = new Regex(@"^(\d{1,3}\.){3}\d{1,3}$");

			foreach (var record in response.Result)
			{
				if (ipRegex.IsMatch(record.Content!) && record.Content != publicIpresponse.YourFuckingIPAddress)
				{
					var patchRequest = new UpdateDnsRecordRequest
					{
						Content = publicIpresponse.YourFuckingIPAddress,
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