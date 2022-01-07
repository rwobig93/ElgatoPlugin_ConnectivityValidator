using System.Buffers.Text;
using System.ComponentModel;
using ElgatoPlugin_ConnectivityValidator.Enums;
using ElgatoPlugin_ConnectivityValidator.Supports;
using Serilog;

namespace ElgatoPlugin_ConnectivityValidator.Actions;

[StreamDeckAction("com.rwobig93.connectivity.validator")]
public class ConnectivityValidateAction : StreamDeckAction<ConnectivitySettings>
{
    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        Log.Information("[{ApplicationName}] => Version {Version} | Application Started!", ApplicationManagement.GetRunningApplicationName(), ApplicationManagement.GetRunningApplicationVersion());
        
        var stopThread = false;
        var settings = args.Payload.GetSettings<ConnectivitySettings>();
        await LocalConfiguration.UpdateRunningSettingsFromLocalSettings(settings);
        var worker = new BackgroundWorker();
        Log.Debug("Attempting to start core worker thread");
        
        worker.DoWork += async (e, a) =>
        {
            Log.Debug("Successfully started core worker thread");
            while (!stopThread)
            {
                try
                {
                    Log.Verbose("Running core connectivity validation loop");
                    settings = args.Payload.GetSettings<ConnectivitySettings>();
                    await UpdateConnectivityState(settings);
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Failure occurred during core connectivity loop: {ErrorMessage}", ex.Message);
                    stopThread = true;
                }
            }
        };
        
        await Task.CompletedTask;
        worker.RunWorkerAsync();
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        Log.Debug("Elgato Stream Deck key was pressed, attempting action");
        var settings = args.Payload.GetSettings<ConnectivitySettings>();
        await SetImageAsync(EncodedImages.ImageChecking);
        await LocalConfiguration.UpdateRunningSettingsFromLocalSettings(settings);
        await UpdateConnectivityState(settings);
    }

    private async Task UpdateConnectivityState(ConnectivitySettings settings)
    {
        try
        {
            var currentState = await NetworkUtilities.GetCurrentConnectivityState(settings);

            switch (currentState)
            {
                case ConnectivityState.Layer4:
                    await SetImageAsync(EncodedImages.ImageLayer4);
                    await SetTitleAsync("Internet");
                    break;
                case ConnectivityState.Layer3:
                    await SetImageAsync(EncodedImages.ImageLayer3);
                    await SetTitleAsync("No DNS");
                    break;
                case ConnectivityState.DefaultGateway:
                    await SetImageAsync(EncodedImages.ImageNoConnectivity);
                    await SetTitleAsync("Gateway Only");
                    break;
                case ConnectivityState.NoConnectivity:
                    await SetImageAsync(EncodedImages.ImageNoConnectivity);
                    await SetTitleAsync("No Connectivity");
                    break;
                default:
                    Console.Write("Default state switch hit, this shouldn't happen!");
                    await SetImageAsync(EncodedImages.ImageChecking);
                    await SetTitleAsync("Failure!");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failure occurred attempting to update our current connectivity state: {ErrorMessage}", ex.Message);
            await SetTitleAsync($"Failure: {ex.Message}");
        }
    }
}