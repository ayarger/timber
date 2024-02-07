using Godot;
using System;

public class ToastMessage : Control
{
	public void ShowMessage(string message, float duration = 2.0f)
	{
		Label messageLabel = GetNode<Label>("Label");
		messageLabel.Text = message;

		Visible = true;

		GetTree().CreateTimer(duration).Connect("timeout", this, nameof(HideToast));
	}

	private void HideToast()
	{
		Visible = false;
	}
}
