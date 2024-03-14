using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;

// Todo for future
// important: move it above the black screen
// keyboard short cut
// audio
// Todo for 3.14-3.21
// Tiny fixes and look on other things to do

public static class ToastManager
{
    private static List<ToastMessage.ToastObject> _toastQueue;
    private static List<ToastMessage.ToastObject> _toastHistory;
    public static ToastMessage.ToastObject currentToast;
    private static bool isShowing;
    private static Node _rootNode;

    static ToastManager()
    {
        _toastQueue = new List<ToastMessage.ToastObject>();
        _toastHistory = new List<ToastMessage.ToastObject>();
        isShowing = false;
    }
    
    // TODO: fix this method
    public static void Initialize(Node rootNode)
    {
        _rootNode = rootNode;
        rootNode.SetProcessInput(true);
    }

    public static void HandleInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && ((keyEvent.Scancode == (int)KeyList.M && Input.IsKeyPressed((int)KeyList.Control)) || (keyEvent.Scancode == (int)KeyList.M && Input.IsKeyPressed((int)KeyList.Meta))))
            {
                ShowToastMessage(_rootNode, "Called Message History.");
            }
        }
    }

    // Called by other scripts to send a toast message
    public static void ShowToastMessage(Node caller, string content, float duration=3.0f, ToastMessage.ToastType type = ToastMessage.ToastType.Error)
    {
        ToastMessage.ToastObject _newMsg = new ToastMessage.ToastObject(caller, content, duration, type);
        _toastHistory.Add(_newMsg);
        
        if (isShowing)
        {
            if (_newMsg.Equals(currentToast))
            {
                currentToast.numOccurred++;
            }
            else
            {
                bool found = false;
                for (int i = 0; i < _toastQueue.Count; i++)
                {
                    ToastMessage.ToastObject t = _toastQueue[i];
                    if (t.Equals(_newMsg))
                    {
                        t.numOccurred++;
                        _toastQueue[i] = t;
                        found = true;
                        break;
                    }
                }
                if (!found) _toastQueue.Add(_newMsg);
            }
            EventBus.Publish(new EventToastUpdate(currentToast));
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

    public static int GetNumMsgInQueue()
    {
        int count = 0;
        foreach (var toast in _toastQueue)
        {
            count += toast.numOccurred;
        }

        count += currentToast.numOccurred - 1;
        return count;
    }
    
    public static List<ToastMessage.ToastObject> GetToastHistory()
    {
        return _toastHistory;
    }

    public static void ClearToastHistory()
    {
        _toastHistory.Clear();
    }
    
    public static void ClearToastQueue()
    {
        _toastQueue.Clear();
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