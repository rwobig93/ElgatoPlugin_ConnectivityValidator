using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ElgatoPlugin_ConnectivityValidator.Actions;
using ElgatoPlugin_ConnectivityValidator.Enums;
using Serilog;

namespace ElgatoPlugin_ConnectivityValidator.Supports;

internal static class NetworkUtilities
{
    internal static async Task<ConnectivityState> GetCurrentConnectivityState(ConnectivitySettings settings)
    {
        var weHaveLayer4Connectivity = await ValidateLayer4Connectivity(settings);
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
        foreach (var address in settings.Layer3ValidationAddresses)
        {
            if (await DoesAddressReturnSuccessCode(address)) return true;
        }

        return false;
    }

    internal static async Task<bool> DoesAddressReturnSuccessCode(string address)
    {
        try
        {
            Log.Verbose("Attempting to validate ping success to address: {Address}", address);
            var pinger = new Ping();
            var reply = await pinger.SendPingAsync(IPAddress.Parse(address));
            Log.Debug("Address ping returned status code: [{Address}] => {ReturnCode}", address, reply.Status);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error attempting to validate Layer3 Address: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    private static async Task<bool> ValidateLayer4Connectivity(ConnectivitySettings settings)
    {
        foreach (var webpage in settings.Layer4ValidationUrls)
        {
            if (await DoesWebpageReturnSuccessStatusCode(webpage)) return true;
        }

        return false;
    }

    internal static async Task<bool> DoesWebpageReturnSuccessStatusCode(string webpageUrl)
    {
        try
        {
            Log.Verbose("Attempting to validate layer4 connectivity to url: {WebsiteUrl}", webpageUrl);
            using var webClient = new HttpClient();
            var response = await webClient.GetAsync(webpageUrl);
            Log.Debug("Website returned status code: [{WebsiteUrl}] => {ReturnCode}", webpageUrl, response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred attempting to validate Layer4 Url: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    private static async Task<bool> ValidateDefaultGatewayConnectivity()
    {
        try
        {
            Log.Verbose("Attempting to validate default gateway connectivity");
            var defaultGateway = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses!)
                .Select(g => g?.Address)
                .Where(a => a?.AddressFamily == AddressFamily.InterNetwork)
                .Where(a => Array.FindIndex(a?.GetAddressBytes() ?? Array.Empty<byte>(), b => b != 0) >= 0)
                .FirstOrDefault(a => a != null);
            if (defaultGateway is not null)
            {
                Log.Verbose("Default gateway was identified: {DefaultGateway}", defaultGateway.ToString());
                return await DoesAddressReturnSuccessCode(defaultGateway.ToString());
            }
            
            Log.Debug("Default gateway wasn't able to be identified");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occurred attempting to get our default gateway: {ErrorMessage}", ex.Message);
        }
        
        return false;
    }
}