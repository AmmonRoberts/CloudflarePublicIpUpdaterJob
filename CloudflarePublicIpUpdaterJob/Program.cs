using CloudflarePublicIpUpdaterJob;
using CloudflarePublicIpUpdaterJob.Clients;
using CloudflarePublicIpUpdaterJob.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(config =>
{
	config.AddDebug();
	config.AddJsonConsole(options =>
	{
		options.UseUtcTimestamp = true;
		options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
	});
});

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

builder.Services.AddHttpClient();

builder.Services.AddRefitClient<ICloudflareClient>()
	.ConfigureHttpClient(c =>
	{
		c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("CloudflareSettings:BaseAddress")!);
		c.DefaultRequestHeaders.Add("Accept", "application/json");
		c.DefaultRequestHeaders.Add("X-Auth-Email", builder.Configuration.GetValue<string>("CloudflareSettings:ApiEmail"));
		c.DefaultRequestHeaders.Add("X-Auth-Key", builder.Configuration.GetValue<string>("CloudflareSettings:ApiKey"));
	});

builder.Services.AddRefitClient<IPublicIpClient>()
	.ConfigureHttpClient(c =>
	{
		c.BaseAddress = new Uri("https://wtfismyip.com");
	});

builder.Services.Configure<JobSettings>(builder.Configuration.GetSection(nameof(JobSettings)));
builder.Services.Configure<CloudflareSettings>(builder.Configuration.GetSection(nameof(CloudflareSettings)));

builder.Services.AddHostedService<ScheduledTask>();

var host = builder.Build();

await host.RunAsync();
