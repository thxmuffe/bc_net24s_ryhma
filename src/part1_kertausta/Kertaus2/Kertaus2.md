# Kertaus2: Nugets

## Take program from previous example (kertaus1) and add logging to it

- Install Visual Studio Code extension: Visual NuGet (Full Stack Spider)
- You will need several Nuget packages
- Seems to be modern day best practise to disentangle nugets into discreet packets

- You need these 7 Nugets:

Serilog
Serilog.Sinks.File
Serilog.Sinks.Console
Serilog.Settings.Configuration


Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Configuration.EnvironmentVariables

Useful links:

Using Serilog
- [https://mbarkt3sto.hashnode.dev/logging-to-a-file-using-serilog]

- [https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration]


Esimerkki appsettins.json (muista vaihtaa CopyAlways)
{
    "Serilog": {
        "Using":  [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
        "MinimumLevel": "Debug",
        "WriteTo": [
          { "Name": "Console" },
          { "Name": "File", "Args": { "path": "Logs/log.txt" } }
        ]
      }
  }
