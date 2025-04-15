using CloudflarePublicIpUpdaterJob.Clients;
using CloudflarePublicIpUpdaterJob.Models.Cloudflare;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CloudflarePublicIpUpdaterJob;

public class DnsRecordUpdaterFunction
{
	private readonly ILogger<DnsRecordUpdaterFunction> _logger;
	private readonly ICloudflareClient _cloudflareClient;
	private readonly IPublicIpClient _publicIpClient;

	public DnsRecordUpdaterFunction(ILogger<DnsRecordUpdaterFunction> logger, ICloudflareClient cloudflareClient, IPublicIpClient publicIpClient)
	{
		_logger = logger;
		_cloudflareClient = cloudflareClient;
		_publicIpClient = publicIpClient;
	}

	[Function("DnsRecordUpdaterFunction")]
	public async Task RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
	{
		_logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

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