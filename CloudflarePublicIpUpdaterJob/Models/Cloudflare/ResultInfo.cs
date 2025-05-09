using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class ResultInfo
{
	[JsonPropertyName("page")]
	public int Page { get; set; }

	[JsonPropertyName("per_page")]
	public int PerPage { get; set; }

	[JsonPropertyName("count")]
	public int Count { get; set; }

	[JsonPropertyName("total_count")]
	public int TotalCount { get; set; }

	[JsonPropertyName("total_pages")]
	public int TotalPages { get; set; }
}