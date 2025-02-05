using Godot;
using System;
using System.Collections.Generic;
using Amazon.CloudFront.Model;


public class TempCurrencyManager : Node
{
	private static int currency = 100;
	private static RichTextLabel currencyLabel;
	private static bool isUpdating = false;
	private static Tween tweenNode;
	
	public override void _Ready()
	{
		currencyLabel = GetParent().GetNode<RichTextLabel>("CanvasLayer/CurrencyLabel");
		currencyLabel.Text = $"${currency}";
		tweenNode = GetNodeOrNull<Tween>("Tween");
	}

	public static void IncreaseMoney(int amount)
	{
		currency += amount;
		UpdateLabel(new Color(0,1,0));
	}
	
	public static void DecreaseMoney(int amount)
	{
		currency -= amount;
		UpdateLabel(new Color(1,0,0));
	}
	
	public static void ChangeMoney(int amount)
	{
		currency = amount;
		UpdateLabel(new Color(0,0,1));
	}

	public static int GetCurrencyAmount()
	{
		return currency;
	}

	public static bool CheckCurrencyGreaterThan(Node caller, int amount, bool sendNotice=false)
	{
		if (amount > currency)
		{
			if (sendNotice) 
				ToastManager.SendToast(caller, "You do not possess enough amount of money for this action.", ToastMessage.ToastType.Warning, 1f);
			return false;
		}

		return true;
	}

	private static void UpdateLabel(Color color)
	{
		if (currencyLabel != null && tweenNode != null && !isUpdating)
		{
			isUpdating = true;

			currencyLabel.Text = $"${currency}";

			var originalScale = currencyLabel.RectScale;
			var originalColor = currencyLabel.Modulate;

			currencyLabel.RectScale = new Vector2(1.2f, 1.2f);
			currencyLabel.Modulate = color;
			tweenNode.InterpolateProperty(currencyLabel, "rect_scale", currencyLabel.RectScale, originalScale, 0.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
			tweenNode.InterpolateProperty(currencyLabel, "modulate", currencyLabel.Modulate, originalColor, 0.5f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
			tweenNode.Start();
		}
	}

	private void _on_Tween_tween_all_completed()
	{
		isUpdating = false;
	}

}

