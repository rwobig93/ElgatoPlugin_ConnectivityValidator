using ElgatoPlugin_ConnectivityValidator.Actions;
using Serilog;
using Serilog.Events;
using Newtonsoft.Json;

namespace ElgatoPlugin_ConnectivityValidator.Supports;

internal class LocalConfiguration
{
    private string[]? WebsitesToCheck { get; }
    private string[]? AddressesToCheck { get; }
    private string? LogLevel { get; }
    
    public LocalConfiguration(string[]? websitesToCheck = null, string[]? addressesToCheck = null, string? logLevel = null)
    {
        WebsitesToCheck = websitesToCheck;
        AddressesToCheck = addressesToCheck;
        LogLevel = logLevel;
    }

    private static LocalConfiguration RunningConfiguration { get; set; } = new(
        ApplicationDefaults.Layer4ValidationUrls, 
        ApplicationDefaults.Layer3ValidationAddresses, 
        ApplicationDefaults.LoggingLevel);

    internal static async Task UpdateRunningSettingsFromLocalSettings(ConnectivitySettings runningSettings)
    {
        Log.Debug("Attempting to update running settings from local settings");
        await LoadConfigurationFile();
        UpdateLoggerLogLevel();
        await UpdateLayer4Websites(runningSettings);
        await UpdateLayer3Addresses(runningSettings);

        Log.Debug("Updated running settings from local settings");
    }

    private static async Task LoadConfigurationFile()
    {
        try
        {
            Log.Verbose("Attempting to load the local configuration file: {ConfigFilePath}", 
                ApplicationDefaults.PathConfigurationFile);
            
            using var reader = new StreamReader(ApplicationDefaults.PathConfigurationFile);
            var fileContent = await reader.ReadToEndAsync();
            RunningConfiguration = JsonConvert.DeserializeObject<LocalConfiguration>(fileContent) ?? 
                                   new LocalConfiguration();
            
            Log.Debug("Loaded local configuration file: {ConfigFilePath}", 
                ApplicationDefaults.PathConfigurationFile);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occurred attempting to read the configuration file: {ErrorMessage}", 
                ex.Message);
        }
    }

    private static void UpdateLoggerLogLevel()
    {
        try
        {
            var localLogLevel = RunningConfiguration.LogLevel ?? "Information";
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
            if (RunningConfiguration.AddressesToCheck is null)
            {
                Log.Debug("AddressesToCheck is null from running configuration, using defaults: " +
                          "{DefaultAddresses}", string.Join(", ", ApplicationDefaults.Layer3ValidationAddresses));
            }

            foreach (var address in RunningConfiguration.AddressesToCheck!)
            {
                if (await NetworkUtilities.DoesAddressReturnSuccessCode(address)) continue;

                addressValidationFailed = true;
                Log.Warning(
                    "Address entered doesn't return a success status code, using defaults. " +
                        "| Entered: {FailedAddress}",
                    address);
                Log.Warning("  Default we're using for now: {DefaultAddresses}",
                    string.Join(", ", ApplicationDefaults.Layer3ValidationAddresses));
            }

            if (runningSettings.Layer3ValidationAddresses != RunningConfiguration.AddressesToCheck && !addressValidationFailed)
            {
                Log.Debug("Attempting to update modified AddressesToCheck");
                runningSettings.Layer3ValidationAddresses = RunningConfiguration.AddressesToCheck;
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
            if (RunningConfiguration.WebsitesToCheck is null)
            {
                Log.Debug("WebsitesToCheck is null from running configuration, using defaults: " +
                          "{DefaultWebpages}", string.Join(", ", ApplicationDefaults.Layer4ValidationUrls));
            }
        
            foreach (var website in RunningConfiguration.WebsitesToCheck!)
            {
                if (await NetworkUtilities.DoesWebpageReturnSuccessStatusCode(website)) continue;
            
                websiteValidationFailed = true;
                Log.Warning(
                    "Webpage entered doesn't return a success status code, using defaults. " +
                    "| Entered: {FailedWebpage}", website);
                Log.Warning("  Default we're using for now: {DefaultWebpages}",
                    string.Join(", ", ApplicationDefaults.Layer4ValidationUrls));
            }

            if (runningSettings.Layer4ValidationUrls != RunningConfiguration.WebsitesToCheck && !websiteValidationFailed)
            {
                Log.Debug("Attempting to update modified WebsitesToCheck");
                runningSettings.Layer4ValidationUrls = RunningConfiguration.WebsitesToCheck;
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