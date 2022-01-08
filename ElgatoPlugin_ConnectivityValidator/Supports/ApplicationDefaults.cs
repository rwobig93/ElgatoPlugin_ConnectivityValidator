namespace ElgatoPlugin_ConnectivityValidator.Supports;

public static class ApplicationDefaults
{
    public static readonly string[] Layer4ValidationUrls = { "https://google.com", "https://microsoft.com" };
    public static readonly string[] Layer3ValidationAddresses = { "8.8.8.8", "8.8.4.4" };
    public const string LoggingLevel = "Information";
    
    public static readonly string PathConfigurationFile = Path.Join(ApplicationManagement.GetRunningApplicationLocationPath(), "appsettings.json");
}