using System.Buffers.Text;
using System.ComponentModel;
using ElgatoPlugin_ConnectivityValidator.Supports;

namespace ElgatoPlugin_ConnectivityValidator.Actions;

[StreamDeckAction("com.rwobig93.connectivity.validator")]
public class ConnectivityValidateAction : StreamDeckAction<ConnectivitySettings>
{
    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var stopThread = false;
        var worker = new BackgroundWorker();
        worker.DoWork += async (e, a) =>
        {
            while (!stopThread)
            {
                try
                {
                    var settings = args.Payload.GetSettings<ConnectivitySettings>();
                    await UpdateConnectivityState(settings);
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    stopThread = true;
                }
            }
        };
        await Task.CompletedTask;
        worker.RunWorkerAsync();
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        var settings = args.Payload.GetSettings<ConnectivitySettings>();
        await SetImageAsync(EncodedImages.ImageChecking);
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
                    break;
                case ConnectivityState.Layer3:
                    await SetImageAsync(EncodedImages.ImageLayer3);
                    break;
                case ConnectivityState.DefaultGateway:
                    await SetImageAsync(EncodedImages.ImageNoConnectivity);
                    break;
                case ConnectivityState.NoConnectivity:
                    await SetImageAsync(EncodedImages.ImageNoConnectivity);
                    break;
                default:
                    Console.Write("Default state switch hit, this shouldn't happen!");
                    await SetImageAsync(EncodedImages.ImageChecking);
                    break;
            }

            await SetTitleAsync(currentState.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await SetTitleAsync($"Failure: {ex.Message}");
        }
    }
}