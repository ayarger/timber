using Godot;
using System;

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
	private Label _numOccurredLabel;
	private string _fullMessage;
	private string _previewMessage;
	private bool _isMouseHovering = false;
	private Vector2 _messageBoxSize;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_messageLabel = GetNode<Label>("Label");
		_messageLabel.Autowrap = true; // Enable autowrap
		_numOccurredLabel = GetNode<Label>("NumOccurred");
		_numOccurredLabel.Autowrap = true;
		_messageBoxSize = _messageLabel.RectSize;

		var panel = GetNode<Panel>("Panel");
		panel.Connect("mouse_entered", this, nameof(OnMouseEntered));
		panel.Connect("mouse_exited", this, nameof(OnMouseExited));

		// Setup mouse filter
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
		
		EventBus.Subscribe<EventToastUpdate>(UpdateToast);

		
	}

	public void ShowMessage(ToastObject msgObj, int previewLength = 50)
	{
		// string message, float duration = 2.0f, 
		_fullMessage = msgObj.content;
		// Generate a preview of the message.
		_previewMessage = _fullMessage.Length <= previewLength ? 
			_fullMessage : _fullMessage.Substring(0, previewLength) + "...";
		_messageLabel.Text = _previewMessage;
		_numOccurredLabel.Text = msgObj.numOccurred.ToString();

		Visible = true;
		_animationPlayer.Play("show_animation");

		// Reset hover state in case of rapid reuse
		_isMouseHovering = false;
		GetTree().CreateTimer(msgObj.duration).Connect("timeout", this, 
														nameof(StartHideAnimation), 
														null, (uint)ConnectFlags.Oneshot);
	}
	
	void UpdateToast(EventToastUpdate e)
	{
		_numOccurredLabel.Text = e.obj.numOccurred.ToString();
		_messageLabel.Text = e.obj.content;
	}

	private void OnMouseEntered()
	{
		_isMouseHovering = true;
		_animationPlayer.Play("expand_animation");
		_messageLabel.Text = _fullMessage; // Show full message
		_messageLabel.RectMinSize = new Vector2(_messageLabel.RectMinSize.x, CalculateLabelHeight(_fullMessage, false));

	}

	private void OnMouseExited()
	{
		_isMouseHovering = false;
		_messageLabel.Text = _previewMessage; // Revert to preview message
		_animationPlayer.Play("shrink_animation");
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
		QueueFree(); // Optionally, remove the toast from the scene tree to clean up
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
}
