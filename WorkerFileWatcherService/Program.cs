using FileWatcherService;
using WorkerFileWatcherService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions();
        services.AddSingleton<IWatcherService, WatcherService>();
        services.AddHostedService<Worker>();
    })
    .Build();

var watcher = host.Services.GetService<IWatcherService>();
var config = host.Services.GetService<IConfiguration>();
var folder = config.GetValue<string>("folder") ?? "";
var file = config.GetValue<string>("file") ?? "";
if (watcher!=null)
    watcher.Watch((message) => Console.WriteLine(message),
                 folder,
                 file);
await host.RunAsync();
