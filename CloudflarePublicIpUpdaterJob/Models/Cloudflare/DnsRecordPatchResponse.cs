using System.Text.Json.Serialization;

namespace CloudflarePublicIpUpdaterJob.Models.Cloudflare;

public class DnsRecordPatchResponse : CloudflareResponse
{
	[JsonPropertyName("result")]
	public DnsRecordPatchResult? Result { get; set; }
}