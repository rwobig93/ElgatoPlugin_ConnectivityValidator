using System.Reflection;
using Serilog;

namespace ElgatoPlugin_ConnectivityValidator.Supports;

internal static class ApplicationManagement
{
    internal static void InitializeLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File($"{GetRunningApplicationLocationPath()}\\Logs\\{GetRunningApplicationFullName()}.log",
                rollingInterval: RollingInterval.Day)
            .MinimumLevel.Information()
            .CreateLogger();
        
        Log.Information("Logger Started");
    }

    internal static Version? GetRunningApplicationVersion()
    {
        return Assembly.GetExecutingAssembly()?.GetName()?.Version;
    }

    internal static string? GetRunningApplicationName()
    {
        return Assembly.GetExecutingAssembly()?.GetName()?.Name;
    }

    internal static string? GetRunningApplicationFullName()
    {
        return Assembly.GetExecutingAssembly()?.GetName()?.FullName;
    }

    internal static string GetRunningApplicationLocationPath()
    {
        return Assembly.GetExecutingAssembly().Location;
    }
}