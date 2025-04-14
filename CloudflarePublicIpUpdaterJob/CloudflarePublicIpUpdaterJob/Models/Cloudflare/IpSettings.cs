using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class IpSettings
{
	[JsonPropertyName("ipv4_only")]
	public bool? Ipv4Only { get; set; }

	[JsonPropertyName("ipv6_only")]
	public bool? Ipv6Only { get; set; }
}