using Godot;
using System.Threading.Tasks;

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
            PackedScene toastScene = (PackedScene)ResourceLoader.Load("res://scenes/ToastMessage.tscn");
            ToastMessage msg = toastScene.Instance() as ToastMessage;

            caller.GetTree().Root.AddChild(msg);

            msg.ShowMessage("No internet connection detected.", 3.0f);
        }

        return isConnected;
    }
}