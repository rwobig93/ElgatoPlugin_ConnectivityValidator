using ElgatoPlugin_ConnectivityValidator.Supports;
using Serilog;

#if DEBUG
    // optional, but recommended
    System.Diagnostics.Debugger.Launch(); 
#endif

ApplicationManagement.InitializeLogger();
Log.Information("[{ApplicationName}] => Version {Version}", ApplicationManagement.GetRunningApplicationName(), ApplicationManagement.GetRunningApplicationVersion());

// register actions and connect to the Stream Deck
await StreamDeckPlugin.RunAsync();
