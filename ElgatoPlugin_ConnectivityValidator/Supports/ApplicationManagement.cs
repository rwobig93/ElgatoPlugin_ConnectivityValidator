using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace ElgatoPlugin_ConnectivityValidator.Supports;

internal static class ApplicationManagement
{
    internal static LoggingLevelSwitch ActiveLogLevel { get; private set; } = 
        new LoggingLevelSwitch(LogEventLevel.Information);
    
    internal static void InitializeLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File($"{GetRunningApplicationLocationPath()}\\Logs\\{GetRunningApplicationName()}_.log",
                rollingInterval: RollingInterval.Day, levelSwitch: ActiveLogLevel)
            .CreateLogger();
        
        Log.Information("Logger Started");
    }

    internal static Version GetRunningApplicationVersion()
    {
        return Assembly.GetExecutingAssembly()?.GetName()?.Version ?? new Version("0.0.0.0");
    }

    internal static string GetRunningApplicationName()
    {
        return Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "com.rwobig93.connectivity.validator";
    }

    internal static string GetRunningApplicationFullName()
    {
        return Assembly.GetExecutingAssembly()?.GetName()?.FullName ?? "com.rwobig93.connectivity.validator";
    }

    internal static string? GetRunningApplicationLocationPath()
    {
        return Path.GetDirectoryName(AppContext.BaseDirectory) ?? 
               new DirectoryInfo(
                       "%appdata$\\Elgato\\StreamDeck\\Plugins\\com.rwobig93.connectivityvalidator.sdPlugin")
                   .FullName;
    }

    internal static void InitializeConfigSettings()
    {
        var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
        var config = builder.Build();
        LocalConfiguration.LocalConfig = config;
        Log.Information("Initialized Configuration Settings");
    }
}