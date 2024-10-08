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

public static class TempCurrencyManager
{
	private static int currency = 100;

	public static void IncreaseMoney(int amount)
	{
		currency += amount;
	}
	
	public static void DecreaseMoney(int amount)
	{
		currency -= amount;
	}
	
	public static void ChangeMoney(int amount)
	{
		currency = amount;
	}

	public static int GetCurrencyAmount()
	{
		return currency;
	}

	public static bool CheckCurrencyGreaterThan(Node caller, int amount, bool sendNotice=false)
	{
		// returns false if the demanded amount is greater than what the player possesses
		if (amount > currency)
		{
			if (sendNotice) 
				ToastManager.SendToast(caller, "You do not possess enough amount of money for this action.", ToastMessage.ToastType.Warning, 1f);
			return false;
		}

		return true;
	}
}
