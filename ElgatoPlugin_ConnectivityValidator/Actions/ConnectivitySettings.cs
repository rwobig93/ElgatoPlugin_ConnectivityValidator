using ElgatoPlugin_ConnectivityValidator.Supports;

namespace ElgatoPlugin_ConnectivityValidator.Actions;

public class ConnectivitySettings
{
    public string[] Layer4ValidationUrls { get; set; } = new[] {"https://google.com","https://microsoft.com"};
    public string[] Layer3ValidationAddresses { get; set; } = new[] {"8.8.8.8", "8.8.4.4"};
    public ConnectivityState ConnectionState { get; set; } = ConnectivityState.NoConnectivity;
}