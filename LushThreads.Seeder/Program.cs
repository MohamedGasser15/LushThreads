using LushThreads.Infrastructure.Configurations;
using LushThreads.Seeder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var options = ParseOptions(args);
var webRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "LushThreads"));
if (!Directory.Exists(webRoot))
    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "LushThreads");

var configuration = new ConfigurationBuilder()
    .SetBasePath(webRoot)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
services.AddSingleton<IConfiguration>(configuration);
services.AddInfrastructureServices(configuration);

using var provider = services.BuildServiceProvider();
await SeedEngine.RunAsync(provider, options);

static SeedOptions ParseOptions(string[] args)
{
    int users = 2000;
    int? products = null;
    int? orders = null;

    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i].Equals("--users", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out var u) && u > 0)
            users = u;
        else if (args[i].Equals("--products", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out var p) && p > 0)
            products = p;
        else if (args[i].Equals("--orders", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out var o) && o > 0)
            orders = o;
    }

    return SeedOptions.FromUserCount(users, products, orders);
}
