using Godot;
using System;

public class ToastMessage : Control
{
	private AnimationPlayer _animationPlayer;
	private bool _isMouseHovering = false;

	public override void _Ready()
	{
		var colorRect = GetNode<ColorRect>("ColorRect");
		colorRect.Connect("mouse_entered", this, nameof(OnMouseEntered));
		colorRect.Connect("mouse_exited", this, nameof(OnMouseExited));
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	
	private void OnMouseEntered()
	{
		_isMouseHovering = true;
		// GD.Print("Mouse entered!");
	}

	private void OnMouseExited()
	{
		_isMouseHovering = false;
		// GD.Print("Mouse left!");
		GetTree().CreateTimer(1.0f).Connect("timeout", this, nameof(StartHideAnimation));
	}


	public void ShowMessage(string message, float duration = 2.0f)
	{
		Label messageLabel = GetNode<Label>("Label");
		messageLabel.Text = message;

		Visible = true;
		// Play the show animation
		_animationPlayer.Play("show_animation");

		// After the show animation, wait for the duration and then start the hide animation
		GetTree().CreateTimer(duration).Connect("timeout", this, nameof(StartHideAnimation));
	}

	private void StartHideAnimation()
	{
		if (!_isMouseHovering)
		{
			_animationPlayer.Play("hide_animation");
			// Ensure this connection is made only once or properly managed to avoid duplicates
			_animationPlayer.Connect("animation_finished", this, nameof(HideToast), new Godot.Collections.Array { "hide_animation" }, (uint)ConnectFlags.Oneshot);
		}
	}

	private void HideToast(string animName)
	{
		if (animName == "hide_animation" && !_isMouseHovering)
		{
			Visible = false;
			// Since we're using Oneshot connection, it's automatically disconnected
		}
	}
}
