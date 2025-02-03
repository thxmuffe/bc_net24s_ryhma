using System.Diagnostics;
using System;
using Microsoft.Extensions.Configuration;
using Serilog;

internal class Program
{
    private const int MaximumNumberOfCalls = 5;

    // all dotnet programs starts from Main-named method
    // command line arguments = args variable

    private static void Main(string[] args)
    {

        // Käytä Microsoftin nugetteja
        // ja lue konfiguraatio tiedosto (appsettings.json)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Käytä Serilog nugettia konfiguroidaksesi
        // Serilogin käyttämään appsettings.json-tiedostosta
        // luettua konfiguraatiota
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        
        // Try to parse first argument as number
        int.TryParse(args.FirstOrDefault("0"), out int num);

        logger.Information($"starting with arg: {num}");

        num++;
        
        if (num > MaximumNumberOfCalls) {
            Console.WriteLine($"Exceeded MaximumNumberOfCalls {MaximumNumberOfCalls}");
            return;
        }

        var p = new Process();
        
        // Get current program executable
        p.StartInfo.FileName =  System.Reflection.Assembly.GetExecutingAssembly().Location;

        // Remove .dll from the end of the file path (macos stuff)
        p.StartInfo.FileName = p.StartInfo.FileName.Substring(0, p.StartInfo.FileName.Length - 4);


        p.StartInfo.Arguments = num.ToString();
        p.StartInfo.UseShellExecute = true; // needed in macos
        p.Start();

        p.WaitForExit(); // Important
        
        logger.Warning("Ending -- " + num.ToString());
    }
}