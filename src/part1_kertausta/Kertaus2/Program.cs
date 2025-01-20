using Microsoft.Extensions.Configuration;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        logger.Information("Hello, world!");
    }
}