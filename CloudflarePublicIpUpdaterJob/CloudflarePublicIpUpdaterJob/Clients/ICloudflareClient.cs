using CloudflarePublicIpUpdaterJob.Models.Cloudflare;
using Refit;

namespace CloudflarePublicIpUpdaterJob.Clients;

public interface ICloudflareClient
{
	[Get("/zones/{zoneId}/dns_records?type=a")]
	Task<DnsRecordListResponse> GetADnsRecordsAsync(string zoneId);

	[Patch("/zones/{zoneId}/dns_records/{dnsRecordId}")]
	Task<DnsRecordPatchResponse> UpdateADnsRecordsAsync(string zoneId, string dnsRecordId, [Body] UpdateDnsRecordRequest updateDnsRecordRequest);
}