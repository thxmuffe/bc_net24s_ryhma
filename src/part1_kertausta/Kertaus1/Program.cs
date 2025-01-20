using System.Diagnostics;
using System;   

internal class Program
{
    private const int MaximumNumberOfCalls = 500;

    private static void Main(string[] args)
    {
        // Try to parse first argument as number
        int.TryParse(args.FirstOrDefault("0"), out int num);

        Console.WriteLine($"starting with arg: {num}");

        num++;
        
        if (num > MaximumNumberOfCalls) {
            Console.WriteLine($"Exceeded MaximumNumberOfCalls {MaximumNumberOfCalls}");
            Environment.Exit(0);
        }

        using (var p = new Process()) {
            // Get current program executable
            p.StartInfo.FileName =  System.Reflection.Assembly.GetExecutingAssembly().Location;

            // Remove .dll from the end of the file path (macos stuff)
            p.StartInfo.FileName = p.StartInfo.FileName.Substring(0, p.StartInfo.FileName.Length - 4);

            p.StartInfo.Arguments = num.ToString();
            p.StartInfo.UseShellExecute = true; // needed in macos
            p.Start();

            p.WaitForExit(); // Important
        }
        
        Console.WriteLine("Ending -- " + num.ToString());
    }
}