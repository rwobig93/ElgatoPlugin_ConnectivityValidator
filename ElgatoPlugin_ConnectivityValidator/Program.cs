#if DEBUG
// optional, but recommended
System.Diagnostics.Debugger.Launch(); 
#endif
    
// register actions and connect to the Stream Deck
await StreamDeckPlugin.RunAsync();