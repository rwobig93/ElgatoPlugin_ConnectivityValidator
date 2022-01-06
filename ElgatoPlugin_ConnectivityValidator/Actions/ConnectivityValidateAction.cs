using ElgatoPlugin_ConnectivityValidator.Supports;

namespace ElgatoPlugin_ConnectivityValidator.Actions;

[StreamDeckAction("com.rwobig93.connectivity.validator")]
public class ConnectivityValidateAction : StreamDeckAction<ConnectivitySettings>
{
    protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
    {
        var settings = args.Payload.GetSettings<ConnectivitySettings>();
        await SetImageAsync("Images/checking");
        await UpdateConnectivityState(settings);
    }

    protected override async Task OnKeyDown(ActionEventArgs<KeyPayload> args)
    {
        var settings = args.Payload.GetSettings<ConnectivitySettings>();
        await SetImageAsync("Images/checking");
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
                    await SetImageAsync("Images/layer4");
                    break;
                case ConnectivityState.Layer3:
                    await SetImageAsync("Images/layer3");
                    break;
                case ConnectivityState.DefaultGateway:
                    await SetImageAsync("Images/defaultgateway");
                    break;
                case ConnectivityState.NoConnectivity:
                    await SetImageAsync("Images/noconnectivity");
                    break;
                default:
                    Console.Write("Default state switch hit, this shouldn't happen!");
                    await SetImageAsync("Images/noconnectivity");
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