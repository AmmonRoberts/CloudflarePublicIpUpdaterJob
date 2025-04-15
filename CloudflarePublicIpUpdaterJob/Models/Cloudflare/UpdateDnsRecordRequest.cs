using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class UpdateDnsRecordRequest
{
	[JsonPropertyName("comment")]
	public string? Comment { get; set; }

	[JsonPropertyName("content")]
	public string? Content { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("proxied")]
	public bool? Proxied { get; set; }

	[JsonPropertyName("ttl")]
	public int? Ttl { get; set; }

	[JsonPropertyName("type")]
	public string? Type { get; set; }
}