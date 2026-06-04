using CloudflarePublicIpUpdaterJob;
using CloudflarePublicIpUpdaterJob.Clients;
using CloudflarePublicIpUpdaterJob.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Refit;
using System.Net;

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

var retryPolicy = HttpPolicyExtensions
	.HandleTransientHttpError()
	.OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
	.WaitAndRetryAsync(
		retryCount: 3,
		sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
		onRetry: (outcome, timespan, retryAttempt, context) =>
		{
			var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
			logger.LogWarning(outcome.Exception, "Retry {RetryAttempt} after {Delay}s.", retryAttempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
		});

var circuitBreakerPolicy = HttpPolicyExtensions
	.HandleTransientHttpError()
	.OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
	.CircuitBreakerAsync(
		handledEventsAllowedBeforeBreaking: 3,
		durationOfBreak: TimeSpan.FromMinutes(1),
		onBreak: (outcome, duration) =>
		{
			var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
			logger.LogError(outcome.Exception, "Circuit breaker opened for {Duration}s.", duration.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
		},
		onReset: () =>
		{
			var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
			logger.LogInformation("Circuit breaker reset.");
		});

builder.Services.AddRefitClient<ICloudflareClient>()
	.ConfigureHttpClient(c =>
	{
		c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("CloudflareSettings:BaseAddress")!);
		c.DefaultRequestHeaders.Add("Accept", "application/json");
		c.DefaultRequestHeaders.Add("X-Auth-Email", builder.Configuration.GetValue<string>("CloudflareSettings:ApiEmail"));
		c.DefaultRequestHeaders.Add("X-Auth-Key", builder.Configuration.GetValue<string>("CloudflareSettings:ApiKey"));
	})
	.AddPolicyHandler(retryPolicy)
	.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddRefitClient<IPublicIpClient>()
	.ConfigureHttpClient(c =>
	{
		c.BaseAddress = new Uri("https://wtfismyip.com");
	})
	.AddPolicyHandler(retryPolicy)
	.AddPolicyHandler(circuitBreakerPolicy);

builder.Services.Configure<JobSettings>(builder.Configuration.GetSection(nameof(JobSettings)));
builder.Services.Configure<CloudflareSettings>(builder.Configuration.GetSection(nameof(CloudflareSettings)));

builder.Services.AddHostedService<ScheduledTask>();

var host = builder.Build();

await host.RunAsync();
