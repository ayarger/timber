using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;

/// <summary>
/// Example:
/// using static ToastManager;
/// SendToast(callerNode, "Message.", 2.0f, ToastMessage.ToastType.Notice);
/// </summary>

public static class ToastManager
{
    private static List<ToastMessage.ToastObject> _toastQueue;
    private static List<ToastMessage.ToastObject> _toastHistory;
    public static ToastMessage.ToastObject currentToast;
    private static bool isShowing;
    private static Node _rootNode;
    private static CanvasLayer _toastLayer;

    static ToastManager()
    {
        _toastQueue = new List<ToastMessage.ToastObject>();
        _toastHistory = new List<ToastMessage.ToastObject>();
        isShowing = false;
    }
    
    public static void Initialize(Node rootNode)
    {
        _rootNode = rootNode;
        rootNode.SetProcessInput(true);
    }

    public static void HandleInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && ((keyEvent.Scancode == (int)KeyList.M && Input.IsKeyPressed((int)KeyList.Control)) 
                                     || (keyEvent.Scancode == (int)KeyList.M && Input.IsKeyPressed((int)KeyList.Meta))))
            {
                SendToast(_rootNode, "User called message display.", type: ToastMessage.ToastType.Notice);
            }
            // START OF DEBUG
            // else if (keyEvent.Pressed && ((keyEvent.Scancode == (int)KeyList.E && Input.IsKeyPressed((int)KeyList.Control)) 
            //                               || (keyEvent.Scancode == (int)KeyList.E && Input.IsKeyPressed((int)KeyList.Meta))))
            // {
            //     SendToast(_rootNode, "Error test.", type: ToastMessage.ToastType.Error);
            // }
            // else if (keyEvent.Pressed && ((keyEvent.Scancode == (int)KeyList.W && Input.IsKeyPressed((int)KeyList.Control)) 
            //                               || (keyEvent.Scancode == (int)KeyList.W && Input.IsKeyPressed((int)KeyList.Meta))))
            // {
            //     SendToast(_rootNode, "Warning test.", type: ToastMessage.ToastType.Warning);
            // }            
            // else if (keyEvent.Pressed && ((keyEvent.Scancode == (int)KeyList.L && Input.IsKeyPressed((int)KeyList.Control)) 
            //                               || (keyEvent.Scancode == (int)KeyList.L && Input.IsKeyPressed((int)KeyList.Meta))))
            // {
            //     SendToast(_rootNode, "Testing long toast message: This is an apple, I like apples, apples are good for our health. " +
            //                                 "An apple a day, keeps the doctor away. " +
            //                                 "Life is like an ocean, only those who are strong in will can reach the shore." + 
            //                                 "Testing long toast message: This is an apple, I like apples, apples are good for our health. " +
            //                                 "An apple a day, keeps the doctor away. " +
            //                                 "Life is like an ocean, only those who are strong in will can reach the shore.");
            // }
            // END OF DEBUG
        }
    }

    // Called by other scripts to send a toast message
    public static void SendToast(Node caller, string content, ToastMessage.ToastType type = ToastMessage.ToastType.Error, float duration=3.0f)
    {
        ToastMessage.ToastObject _newMsg = new ToastMessage.ToastObject(caller, content, duration, type);
        _toastHistory.Add(_newMsg);
        
        if (isShowing)
        {
            // if (_newMsg.Equals(currentToast))
            // {
            //     currentToast.numOccurred++;
            // }
            // else
            // {
            //     bool found = false;
            //     for (int i = 0; i < _toastQueue.Count; i++)
            //     {
            //         ToastMessage.ToastObject t = _toastQueue[i];
            //         if (t.Equals(_newMsg))
            //         {
            //             t.numOccurred++;
            //             _toastQueue[i] = t;
            //             found = true;
            //             break;
            //         }
            //     }
            //     if (!found) _toastQueue.Add(_newMsg);
            // }
            _toastQueue.Add(_newMsg);
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
        Node root = obj.caller.GetTree().Root;
        if (_toastLayer == null)
        {
            _toastLayer = new CanvasLayer();
            var canvasLayers = root.GetChildren().OfType<CanvasLayer>().ToList();
            int highestLayer = canvasLayers.Any() ? canvasLayers.Max(cl => cl.Layer) + 999 : 1;
            _toastLayer.Layer = highestLayer;
            root.AddChild(_toastLayer);
        }
        
        PackedScene toastScene = (PackedScene)ResourceLoader.Load("res://scenes/ToastMessage.tscn");
        ToastMessage msg = toastScene.Instance() as ToastMessage;
        if (msg != null)
        {
            _toastLayer.AddChild(msg);
            msg.ShowMessage(obj);
            msg.PlayToastSoundEffect();
        }
    }

    
    public static void SetToastVisibility(bool visible)
    {
        if (!visible)
        {
            RemoveToastLayer();
            if (_toastQueue.Count > 0)
            {
                var firstElement = _toastQueue[0]; 
                _toastQueue.RemoveAt(0);
                ShowToast(firstElement);
                return;
            }
        }
        isShowing = visible;
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
    
    private static void RemoveToastLayer()
    {
        if (_toastLayer != null && _toastLayer.GetChildren().Count == 0)
        {
            _toastLayer.QueueFree();
            _toastLayer = null;
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