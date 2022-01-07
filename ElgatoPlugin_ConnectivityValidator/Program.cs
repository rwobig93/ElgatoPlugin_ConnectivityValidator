using ElgatoPlugin_ConnectivityValidator.Supports;

#if DEBUG
    // optional, but recommended
    System.Diagnostics.Debugger.Launch(); 
#endif

ApplicationManagement.InitializeLogger();

// register actions and connect to the Stream Deck
await StreamDeckPlugin.RunAsync();
