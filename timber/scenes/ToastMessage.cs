using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Make several different style draft to choose from?
// Add more selectable styles (eg. warning/notification)
// make text selectable and copiable for player to debug
// make the size according to the length of the text
// TODO:
// queue up req wait until the latest toast to go off
// detect the same msg

public class ToastMessage : Control
{
	public struct ToastObject
	{
		public Node caller;
		public string content;
		public float duration;
		public ToastType type;
		public int numOccurred;

		public ToastObject (Node _caller, string _content, float _duration, ToastType _type)
		{
			caller = _caller;
			content = _content;
			duration = _duration;
			type = _type;
			numOccurred = 1;
		}
		
		public override bool Equals(object obj) 
		{
			if (!(obj is ToastObject)) return false;

			ToastObject msg = (ToastObject) obj;
			return msg.content == content && 
				   msg.type == type && 
				   msg.caller == caller && 
				   msg.duration - duration < 0.01f;
		}
	}
	public enum ToastType
	{
		Warning,
		Error,
		Notification,
	}
	
	private AnimationPlayer _animationPlayer;
	private Label _messageLabel;
	private Label _numOccurredLabelFull;
	private Label _numOccurredLabelShort;
	private Label _numMsgLabelFull;
	private Label _numMsgLabelShort;
	private ScrollContainer _historyScrollVBoxContainer;
	private string _fullMessage;
	private string _previewMessage;
	private int mouseEnterCount = 0;
	private bool _isMouseHovering = false;
	private bool _mouseOnHistory = false;
	private bool _mouseEnterFronHistory = false;
	private Vector2 _messageBoxSize;
	private Timer _visibilityTimer; // Declare the timer member

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_messageLabel = GetNode<Label>("Panel/Label");
		_messageLabel.Autowrap = true; // Enable autowrap
		_messageBoxSize = _messageLabel.RectSize;
		
		_numOccurredLabelFull = GetNode<Label>("Panel/NumOccurred_full");
		_numOccurredLabelFull.Autowrap = true;	
		_numOccurredLabelFull.Visible = false;
		_numOccurredLabelFull.Text = "";
		
		_numMsgLabelFull = GetNode<Label>("Panel/NumMsg_full");
		_numMsgLabelFull.Autowrap = true;
		_numMsgLabelFull.Visible = false;
		_numMsgLabelFull.Text = "";
		
		_numMsgLabelShort = GetNode<Label>("Panel/NumMsg_short");
		_numMsgLabelShort.Autowrap = true;
		_numMsgLabelShort.Text = "";	
		
		_numOccurredLabelShort = GetNode<Label>("Panel/NumOccurred_short");
		_numOccurredLabelShort.Autowrap = true;
		_numOccurredLabelShort.Text = "";
		
		_visibilityTimer = GetNode<Timer>("Timer");
		_visibilityTimer.Connect("timeout", this, nameof(StartHideAnimation));

		var panel = GetNode<Panel>("Panel");
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
		panel.Connect("mouse_entered", this, nameof(OnMouseEntered));
		panel.Connect("mouse_exited", this, nameof(OnMouseExited));

		_historyScrollVBoxContainer = GetNode<ScrollContainer>("Panel/HistoryScrollContainer");
		_historyScrollVBoxContainer.Visible = false;
		_historyScrollVBoxContainer.MouseFilter = MouseFilterEnum.Pass;
		_historyScrollVBoxContainer.Connect("mouse_entered", this, nameof(OnMouseEnterHistory));
		_historyScrollVBoxContainer.Connect("mouse_exited", this, nameof(OnMouseExitHistory));
		
		EventBus.Subscribe<EventToastUpdate>(UpdateToast);
	}

	private void MessageObjContentDisplay(ToastObject msgObj, int previewLength = 50)
	{
		_fullMessage = msgObj.content;
		_previewMessage = _fullMessage.Length <= previewLength ? 
			_fullMessage : _fullMessage.Substring(0, previewLength) + "...";
		_messageLabel.Text = _previewMessage;
		int numMsg = ToastManager.GetNumMsgInQueue();
		if (numMsg > 0) _numMsgLabelShort.Text = numMsg.ToString() + " more";
		if (numMsg > 0) _numMsgLabelFull.Text = numMsg.ToString() + " more message";
		if (msgObj.numOccurred > 1) _numOccurredLabelFull.Text = "Occurred " + (msgObj.numOccurred).ToString() + " times";
		if (msgObj.numOccurred > 1) _numOccurredLabelShort.Text = (msgObj.numOccurred).ToString() + " occurred";
	}
	
	public void ShowMessage(ToastObject msgObj, int previewLength = 50)
	{
		MessageObjContentDisplay(msgObj);
		Visible = true;
		_animationPlayer.Play("show_animation");
		_isMouseHovering = false;
		ResetAndStartTimer(msgObj.duration);
		// GetTree().CreateTimer(msgObj.duration).Connect("timeout", this, 
		// 												nameof(StartHideAnimation), 
		// 												null, (uint)ConnectFlags.Oneshot);
	}

	void UpdateToast(EventToastUpdate e)
	{
		MessageObjContentDisplay(e.obj);
		_messageLabel.Text = e.obj.content;
		ResetAndStartTimer(e.obj.duration);
	}

	private async void OnMouseEntered()
	{
		GD.Print("Mouse enter attempt.");
		mouseEnterCount++;

		await Task.Delay(TimeSpan.FromMilliseconds(100));
		if (mouseEnterCount != 1 || _isMouseHovering)
		{
			return;
		}
		GD.Print("Mouse enter.");

		_isMouseHovering = true;
		_animationPlayer.Play("expand_animation");
		_numMsgLabelShort.Visible = false;
		_numOccurredLabelShort.Visible = false;
		_numMsgLabelFull.Visible = true;
		_numOccurredLabelFull.Visible = true;
		_historyScrollVBoxContainer.Visible = true;
		ShowToastHistoryUI();

		_messageLabel.Visible = false;
		_messageLabel.Text = _fullMessage; // Show full message
		_messageLabel.RectMinSize = new Vector2(_messageLabel.RectMinSize.x, CalculateLabelHeight(_fullMessage, false));

	}

	private async void OnMouseExited()
	{
		GD.Print("Mouse exit attempt.");
		await Task.Delay(TimeSpan.FromMilliseconds(100));
		mouseEnterCount--;
		if (mouseEnterCount != 0)
		{
			return;
		}
		GD.Print("Mouse exit.");
		_isMouseHovering = false;
		_mouseEnterFronHistory = false;
		_messageLabel.Text = _previewMessage; // Revert to preview message
		_numMsgLabelShort.Visible = true;
		_numOccurredLabelShort.Visible = true;
		_numMsgLabelFull.Visible = false;
		_numOccurredLabelFull.Visible = false;
		_historyScrollVBoxContainer.Visible = false;
		_animationPlayer.Play("shrink_animation");

		_messageLabel.Visible = true;
		_messageLabel.RectMinSize = new Vector2(_messageLabel.RectMinSize.x, CalculateLabelHeight(_previewMessage, true));
		
		// Start hiding animation directly or after a delay
		GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(StartHideAnimation), null, (uint)ConnectFlags.Oneshot);

	}

	private void StartHideAnimation()
	{
		if (!_isMouseHovering)
		{
			GetNode<Panel>("Panel").MouseFilter = Control.MouseFilterEnum.Ignore;
			_animationPlayer.Play("hide_animation");
			GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(HideToast), null, (uint)ConnectFlags.Oneshot);
		}
	}

	private void HideToast()
	{
		Visible = false;
		QueueFree(); // Remove the toast from the scene tree to clean up
		ToastManager.SetToastVisibility(false);
	}

	private float CalculateLabelHeight(string message, bool isPreview)
	{
		if (isPreview)
		{
			return _messageBoxSize.y;
		}
		else
		{
			return _messageBoxSize.y * 3;
		}
	}
	
	private void ResetAndStartTimer(float duration)
	{
		_visibilityTimer.Stop(); // Stop the current timer if it's running
		_visibilityTimer.WaitTime = duration;
		_visibilityTimer.OneShot = true;
		_visibilityTimer.Start();
	}

	private void ShowToastHistoryUI()
	{
		// Retrieve toast history
		List<ToastObject> history = ToastManager.GetToastHistory();
		var temp = _historyScrollVBoxContainer.GetNode<VBoxContainer>("VBoxContainer");
		Label labelTemplate = temp.GetNode<Label>("LabelTemplate");

		foreach (Node child in temp.GetChildren())
		{
			temp.RemoveChild(child);
			child.QueueFree();
		}

		float totalHeight = 0;

		foreach (ToastObject toast in history)
		{
			// Duplicate the label template
			Label label = (Label)labelTemplate.Duplicate();

			// Adjust properties for each message
			label.Text = $"{toast.content} - {toast.type.ToString()}";

			// Set the position of the label
			label.RectPosition = new Vector2(0, totalHeight);

			// Add the height of the current label to the total height
			totalHeight += label.RectSize.y;

			// Add some additional spacing between labels
			totalHeight += 5; // Adjust as needed for spacing

			// Add the label to the container
			temp.AddChild(label);
			
		}
	}

	private void OnMouseEnterHistory()
	{
		GD.Print("Mouse enter history.");
		mouseEnterCount++;
	}
	
	private void OnMouseExitHistory()
	{
		GD.Print("Mouse exit history.");
		mouseEnterCount--;
	}
}
