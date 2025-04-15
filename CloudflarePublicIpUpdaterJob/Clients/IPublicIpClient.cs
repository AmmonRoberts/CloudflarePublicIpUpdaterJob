using CloudflarePublicIpUpdaterJob.Models;
using Refit;

namespace CloudflarePublicIpUpdaterJob.Clients;

public interface IPublicIpClient
{
	[Get("/json")]
	Task<PublicIpInfo> GetPublicIpJson();
}