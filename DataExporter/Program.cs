using System;
using Avalonia;
using Avalonia.ReactiveUI;
using DataExporter.Helpers;
using Serilog;

namespace DataExporter
{
    class Program
    {
        // Toggle this manually or use args
        static bool _runUi = true; 

        [STAThread]
        public static void Main(string[] args)
        {
            // 1. Check Arguments to decide mode
            if (args.Length > 0 && (args[0] == "--cli" || args[0] == "-c"))
            {
                _runUi = false;
            }

            
            try 
            {
                if (_runUi)
                {
                    try 
                    {
                        Log.Information("Mode: UI (Avalonia)");

                        BuildAvaloniaApp()
                            .StartWithClassicDesktopLifetime(args);
                    }
                    catch (PlatformNotSupportedException ex)
                    {
                        Log.Error("GUI is not supported in this environment. Switching to CLI mode automatically.");
                        Log.Information("To run CLI explicitly, use: dotnet run -- --cli");
                        
                        // Fallback to CLI
                        CliRunner.Run();
                    }
                    catch (Exception ex)
                    {
                         Log.Fatal(ex, "Failed to start UI.");
                    }
                }
                else
                {
                    // 2. Setup Logging
                    var loggerConfig = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day);


                    Log.Logger = loggerConfig.CreateLogger();

                    Log.Information("=== Application Started ===");

                    // Run CLI Logic
                    CliRunner.Run();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application Crash");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
