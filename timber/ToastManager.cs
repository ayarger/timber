using System;
using Godot;
using System.Threading.Tasks;

public static class ToastManager
{
    public enum ToastType
    {
        Warning,
        Error,
        Notification,
    }
    
    public static void ShowToastMessage(Node caller, string content, float duration=3.0f, ToastType type = ToastType.Error)
    {
        PackedScene toastScene = (PackedScene)ResourceLoader.Load("res://scenes/ToastMessage.tscn"); // Optionally, can pass in other scenes
        ToastMessage msg = toastScene.Instance() as ToastMessage;
        caller.GetTree().Root.AddChild(msg);
        msg.ShowMessage(content, duration);
    }
}