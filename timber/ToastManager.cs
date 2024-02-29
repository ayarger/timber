using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;

// a queue records all the msgs awaits for showing
// a record of the current msg

// creates a struct
// check if it is the same with current toast
// if yes, ...
// if not, push to queue
// when a toast leaves the screen, call a function to push an object from queue and shows it on the screen

public static class ToastManager
{
    private static List<ToastMessage.ToastObject> _toastQueue;
    private static ToastMessage.ToastObject currentToast;
    private static bool isShowing;
    static ToastManager()
    {
        _toastQueue = new List<ToastMessage.ToastObject>();
        isShowing = false;
    }

    // Called by other scripts to send a toast message
    public static void ShowToastMessage(Node caller, string content, float duration=3.0f, ToastMessage.ToastType type = ToastMessage.ToastType.Error)
    {
        ToastMessage.ToastObject _newMsg = new ToastMessage.ToastObject(caller, content, duration, type);
        
        if (isShowing)
        {
            if (_newMsg.Equals(currentToast))
            {
                currentToast.numOccurred++;
                EventBus.Publish(new EventToastUpdate(currentToast));
                return; 
            }
            for (int i = 0; i < _toastQueue.Count; i++)
            {
                ToastMessage.ToastObject t = _toastQueue[i];
                if (t.Equals(_newMsg))
                {
                    t.numOccurred++;
                    _toastQueue[i] = t;
                    return;
                }
            }
            _toastQueue.Add(_newMsg);
        }
        else
        {
            isShowing = true;
            ShowToast(_newMsg);
        }
    }

    // Show a new message object
    private static void ShowToast(ToastMessage.ToastObject obj)
    {
        currentToast = obj;
        PackedScene toastScene = (PackedScene)ResourceLoader.Load("res://scenes/ToastMessage.tscn"); // Optionally, can pass in other scenes
        ToastMessage msg = toastScene.Instance() as ToastMessage;
        obj.caller.GetTree().Root.AddChild(msg);
        if (msg != null) msg.ShowMessage(obj);
    }
    
    public static void SetToastVisibility(bool visible)
    {
        if (!visible && _toastQueue.Count > 0)
        {
            var firstElement = _toastQueue[0]; 
            _toastQueue.RemoveAt(0); 
            ShowToast(firstElement);
        }
        else
        {
            isShowing = visible;
        }
    }

}

public class EventToastUpdate
{
    public ToastMessage.ToastObject obj;

    public EventToastUpdate (ToastMessage.ToastObject _obj)
    {
        obj = _obj;
    }
}