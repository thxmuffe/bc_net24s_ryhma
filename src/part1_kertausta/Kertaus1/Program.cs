using System.Diagnostics;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("start " + String.Join(", ", args));

        // Try to parse first argument as number
        if (int.TryParse(args.FirstOrDefault(), out int num))
        {
            num++;

            if (num > 10) {
                Console.WriteLine("Reached 10!");
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
            
        }
        else
        {
            Console.WriteLine("No or faulty arguments give.");
        }

        Console.WriteLine("Ending -- " + num.ToString());
    }
}