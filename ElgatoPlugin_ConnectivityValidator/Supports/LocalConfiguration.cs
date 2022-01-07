using System.Collections;
using ElgatoPlugin_ConnectivityValidator.Actions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace ElgatoPlugin_ConnectivityValidator.Supports;

internal static class LocalConfiguration
{
    internal static IConfigurationRoot LocalConfig { get; set; } = new ConfigurationManager();

    internal static async Task UpdateRunningSettingsFromLocalSettings(ConnectivitySettings runningSettings)
    {
        Log.Debug("Attempting to update running settings from local settings");
        // TODO: Need to read from appsettings.json as it isn't being updated in real-time, otherwise it'll only
        //       load on startup
        UpdateLoggerLogLevel(runningSettings);
        await UpdateLayer4Websites(runningSettings);
        await UpdateLayer3Addresses(runningSettings);

        Log.Debug("Updated running settings from local settings");
    }

    private static void UpdateLoggerLogLevel(ConnectivitySettings runningSettings)
    {
        try
        {
            var localLogLevel = LocalConfig["LogLevel"] ?? "Information";
            var levelLower = localLogLevel.ToLower();
            LogEventLevel localConfigLogLevel;
            switch (levelLower)
            {
                case "verbose":
                    localConfigLogLevel = LogEventLevel.Verbose;
                    break;
                case "debug":
                    localConfigLogLevel = LogEventLevel.Debug;
                    break;
                case "information":
                    localConfigLogLevel = LogEventLevel.Information;
                    break;
                case "warning":
                    localConfigLogLevel = LogEventLevel.Warning;
                    break;
                case "error":
                    localConfigLogLevel = LogEventLevel.Error;
                    break;
                case "fatal":
                    localConfigLogLevel = LogEventLevel.Fatal;
                    break;
                default:
                    localConfigLogLevel = LogEventLevel.Information;
                    Log.Warning("LogLevel entered is invalid, using default | Entered: {LogLevelEntered}" +
                                " | Using: {LogLevelUsed}", localLogLevel, "Information");
                    break;
            }

            if (ApplicationManagement.ActiveLogLevel.MinimumLevel == localConfigLogLevel) return;
            
            ApplicationManagement.ActiveLogLevel.MinimumLevel = localConfigLogLevel;
            Log.Information("LogLevel setting changed, updated to: {LogLevelUpdated}", 
                localLogLevel);

        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occurred attempting to update LogLevel setting: {ErrorMessage}", 
                ex.Message);
        }
    }

    private static async Task UpdateLayer3Addresses(ConnectivitySettings runningSettings)
    {
        try
        {
            var addressValidationFailed = false;
            var addressesToCheck = LocalConfig["AddressesToCheck"]?.Replace(" ", "")
                .Split(",") ?? ApplicationDefaults.Layer3ValidationAddresses;

            foreach (var address in addressesToCheck)
            {
                if (await NetworkUtilities.DoesAddressReturnSuccessCode(address)) continue;

                addressValidationFailed = true;
                Log.Warning(
                    "Address entered doesn't return a success status code, using defaults. " +
                        "| Entered: {FailedAddress}",
                    address);
                Log.Warning("  Default we're using for now: {DefaultAddress}",
                    string.Join(", ", ApplicationDefaults.Layer3ValidationAddresses));
            }

            if (runningSettings.Layer3ValidationAddresses != addressesToCheck && !addressValidationFailed)
            {
                Log.Debug("Attempting to update modified AddressesToCheck");
                runningSettings.Layer3ValidationAddresses = addressesToCheck;
                Log.Information("AddressesToCheck changed, updating: {Layer3ValidationAddresses}", 
                    runningSettings.Layer3ValidationAddresses);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occurred attempting to update AddressesToCheck setting: {ErrorMessage}", 
                ex.Message);
        }
    }

    private static async Task UpdateLayer4Websites(ConnectivitySettings runningSettings)
    {
        try
        {
            var websiteValidationFailed = false;
            var websitesToCheck = LocalConfig["WebsitesToCheck"]?.Replace(" ", "")
                                      .Split(",") ?? ApplicationDefaults.Layer4ValidationUrls;
        
            foreach (var website in websitesToCheck)
            {
                if (await NetworkUtilities.DoesWebpageReturnSuccessStatusCode(website)) continue;
            
                websiteValidationFailed = true;
                Log.Warning(
                    "Webpage entered doesn't return a success status code, using defaults. " +
                    "| Entered: {FailedWebpage}", website);
                Log.Warning("  Default we're using for now: {DefaultWebpages}",
                    string.Join(", ", ApplicationDefaults.Layer4ValidationUrls));
            }

            if (runningSettings.Layer4ValidationUrls != websitesToCheck && !websiteValidationFailed)
            {
                Log.Debug("Attempting to update modified WebsitesToCheck");
                runningSettings.Layer4ValidationUrls = websitesToCheck;
                Log.Information("WebsitesToChange changed, updating: {Layer4ValidationUrls}",
                    runningSettings.Layer4ValidationUrls);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occurred attempting to update WebsitesToCheck setting: {ErrorMessage}", 
                ex.Message);
        }
    }
}