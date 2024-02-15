using Godot;
using System;

// TODO: Add more selectable styles eg warning
// TODO: make text selectable and copiable

public class ToastMessage : Control
{
	private AnimationPlayer _animationPlayer;
	private Label _messageLabel;
	private string _fullMessage;
	private string _previewMessage;
	private bool _isMouseHovering = false;
	private Vector2 _messageBoxSize;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_messageLabel = GetNode<Label>("Label");
		_messageLabel.Autowrap = true; // Enable autowrap
		_messageBoxSize = _messageLabel.RectSize;

		var panel = GetNode<Panel>("Panel");
		panel.Connect("mouse_entered", this, nameof(OnMouseEntered));
		panel.Connect("mouse_exited", this, nameof(OnMouseExited));

		// Setup mouse filter
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
	}

	public void ShowMessage(string message, float duration = 2.0f, int previewLength = 50)
	{
		_fullMessage = message;
		// Generate a preview of the message.
		_previewMessage = message.Length <= previewLength ? message : message.Substring(0, previewLength) + "...";
		_messageLabel.Text = _previewMessage;

		Visible = true;
		_animationPlayer.Play("show_animation");

		// Reset hover state in case of rapid reuse
		_isMouseHovering = false;

		GetTree().CreateTimer(duration).Connect("timeout", this, nameof(StartHideAnimation), null, (uint)ConnectFlags.Oneshot);
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
