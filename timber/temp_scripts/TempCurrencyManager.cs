using Godot;
using System;
using System.Collections.Generic;
using Amazon.CloudFront.Model;


// TODO: 4.11 - 4.18
//		fix button press issue ?
//		ranged projectile
//		ko animation sprite
//		currency - done temp currency system
//		make different kind of tower - by disable/enable components. 
//		fix the issue that the tower is off the grid - fixed
public class TempCurrencyManager : Node
{
	private static int currency = 100;
	private static RichTextLabel currencyLabel;
	
	public override void _Ready()
	{
		currencyLabel = GetParent().GetNode<RichTextLabel>("CanvasLayer/CurrencyLabel");
		currencyLabel.Text = $"${currency}";
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
		if (currencyLabel != null)
		{
			currencyLabel.Text = $"${currency}";

			// Update the text
			currencyLabel.Text = $"${currency}";

			// Create a size bounce effect by animating the scale
			var originalScale = currencyLabel.RectScale;
			currencyLabel.RectScale = new Vector2(1.2f, 1.2f); 
			var originalColor = currencyLabel.Modulate;
			currencyLabel.Modulate = color; 

			currencyLabel.CreateTween().TweenProperty(currencyLabel, "rect_scale", originalScale, 0.5f)
				.SetTrans(Tween.TransitionType.Bounce)
				.SetEase(Tween.EaseType.Out);

			currencyLabel.CreateTween().TweenProperty(currencyLabel, "modulate", originalColor, 0.5f);
		}
	}
}
