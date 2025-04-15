namespace CloudflarePublicIpUpdaterJob.Configuration;
public class CloudflareSettings
{
	public string BaseAddress { get; set; } = string.Empty;

	public string ApiKey { get; set; } = string.Empty;

	public string ApiEmail { get; set; } = string.Empty;

	public string ZoneId { get; set; } = string.Empty;
}