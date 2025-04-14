using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class DnsRecordPatchResult
{
	[JsonPropertyName("comment")]
	public string? Comment { get; set; }

	[JsonPropertyName("content")]
	public string? Content { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("proxied")]
	public bool? Proxied { get; set; }

	[JsonPropertyName("settings")]
	public IpSettings? Settings { get; set; }

	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = [];

	[JsonPropertyName("ttl")]
	public int? Ttl { get; set; }

	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("proxiable")]
	public bool? Proxiable { get; set; }
}