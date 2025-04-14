using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class CloudflareResponse
{
	[JsonPropertyName("errors")]
	public List<object> Errors { get; set; } = [];

	[JsonPropertyName("messages")]
	public List<object> Messages { get; set; } = [];

	[JsonPropertyName("result_info")]
	public ResultInfo? ResultInfo { get; set; }

	[JsonPropertyName("success")]
	public bool Success { get; set; }
}