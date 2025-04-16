using CloudflarePublicIpUpdaterJob.Clients;
using CloudflarePublicIpUpdaterJob.Configuration;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);
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

builder.Services.Configure<CloudflareSettings>(builder.Configuration.GetSection("CloudflareSettings"));

var host = builder.Build();

await host.RunAsync();
