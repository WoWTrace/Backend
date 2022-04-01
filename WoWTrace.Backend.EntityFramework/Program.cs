using Microsoft.Extensions.Hosting;

namespace WowTrace.Backend;

internal class Program
{
    static Task Main(string[] args)
        => CreateHostBuilder(args)
            .Build()
            .RunAsync();

    static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {

        });

}
