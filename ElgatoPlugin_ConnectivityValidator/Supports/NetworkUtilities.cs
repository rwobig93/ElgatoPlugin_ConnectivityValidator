using System.Net;
using System.Net.NetworkInformation;
using ElgatoPlugin_ConnectivityValidator.Actions;

namespace ElgatoPlugin_ConnectivityValidator.Supports;

public static class NetworkUtilities
{
    public static async Task<ConnectivityState> GetCurrentConnectivityState(ConnectivitySettings settings)
    {
        var webClient = new HttpClient();
        
        var weHaveLayer4Connectivity = await ValidateLayer4Connectivity(settings, webClient);
        if (weHaveLayer4Connectivity)
            return ConnectivityState.Layer4;
        
        var weHaveLayer3Connectivity = await ValidateLayer3Connectivity(settings);
        if (weHaveLayer3Connectivity)
            return ConnectivityState.Layer3;
        
        var weHaveDefaultGatewayConnectivity = await ValidateDefaultGatewayConnectivity();
        return weHaveDefaultGatewayConnectivity ? ConnectivityState.DefaultGateway : ConnectivityState.NoConnectivity;
    }

    private static async Task<bool> ValidateLayer3Connectivity(ConnectivitySettings settings)
    {
        try
        {
            foreach (var address in settings.Layer3ValidationAddresses)
            {
                var pinger = new Ping();
                var reply = await pinger.SendPingAsync(new IPAddress(Convert.ToByte(address)));
                if (reply.Status == IPStatus.Success)
                    return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return false;
    }

    private static async Task<bool> ValidateLayer4Connectivity(ConnectivitySettings settings, HttpClient webClient)
    {
        try
        {
            foreach (var webpage in settings.Layer4ValidationUrls)
            {
                var response = await webClient.GetAsync(webpage);
                if (response.IsSuccessStatusCode)
                    return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return false;
    }

    private static async Task<bool> ValidateDefaultGatewayConnectivity()
    {
        try
        {
            var defaultGateway = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                // .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                // .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .FirstOrDefault(a => a != null);
            if (defaultGateway is not null)
            {
                var pinger = new Ping();
                var reply = await pinger.SendPingAsync(defaultGateway);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        
        return false;
    }
}