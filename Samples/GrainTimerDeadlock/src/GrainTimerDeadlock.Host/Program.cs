using GrainTimerDeadlock.Grains.Contracts;
using GrainTimerDeadlock.Grains.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

using var host = Host.CreateDefaultBuilder()
	.UseOrleans(b =>
	{
		_ = b.UseLocalhostClustering()
			.Configure<ClusterOptions>(o =>
			{
				o.ClusterId = "dev";
				o.ServiceId = "grain-timers-deadlocks";
			})
			.Configure<GrainCollectionOptions>(o =>
			{
				o.CollectionQuantum = TimeSpan.FromSeconds(1);
				o.ClassSpecificCollectionAge[typeof(TestGrain).FullName!] = TestGrain.Interval;
			})
			.ConfigureLogging(l => l.AddConsole());
	}).Build();

await host.StartAsync();

var client = host.Services.GetService<IClusterClient>();
var grain = client!.GetGrain<ITestGrain>(0);
await grain.DoSomethingElseAsync();

Console.WriteLine("\nPress any key to terminate the process...\n");
Console.ReadKey();

await host.StopAsync();
