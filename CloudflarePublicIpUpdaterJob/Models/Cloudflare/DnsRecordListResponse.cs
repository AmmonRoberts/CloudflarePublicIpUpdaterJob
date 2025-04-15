using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class DnsRecordListResponse : CloudflareResponse
{
	[JsonPropertyName("result")]
	public List<DnsRecordResult> Result { get; set; } = [];
}