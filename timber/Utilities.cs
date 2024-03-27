using Godot;
using System.Threading.Tasks;
using static ToastManager;

public static class Utilities
{
    public static async Task<bool> IsConnectedToInternet(Node caller)
    {
        var httpClient = new HTTPClient();
        Error result = httpClient.ConnectToHost("google.com", 80); // use google to check connectivity

        if (result != Error.Ok)
        {
            return false;
        }

        ulong timeoutSeconds = 15;
        var startTime = OS.GetUnixTime();
        var endTime = startTime + timeoutSeconds;

        while (httpClient.GetStatus() == HTTPClient.Status.Connecting || httpClient.GetStatus() == HTTPClient.Status.Resolving)
        {
            httpClient.Poll();
            await caller.ToSignal(caller.GetTree().CreateTimer(0.1f), "timeout");

            if (OS.GetUnixTime() >= endTime)
            {
                GD.Print("Connection attempt timed out.");
                httpClient.Close();
                return false;
            }
        }

        bool isConnected = httpClient.GetStatus() == HTTPClient.Status.Connected;
        httpClient.Close();
        
        if (!isConnected)
        {
            SendToast(caller, "No internet connection detected.");
        }

        return isConnected;
    }
}