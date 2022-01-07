using ElgatoPlugin_ConnectivityValidator.Enums;
using ElgatoPlugin_ConnectivityValidator.Supports;

// ReSharper disable ClassNeverInstantiated.Global

namespace ElgatoPlugin_ConnectivityValidator.Actions;

public class ConnectivitySettings
{
    public string[] Layer4ValidationUrls { get; set; } = ApplicationDefaults.Layer4ValidationUrls;
    public string[] Layer3ValidationAddresses { get; set; } = ApplicationDefaults.Layer3ValidationAddresses;
}