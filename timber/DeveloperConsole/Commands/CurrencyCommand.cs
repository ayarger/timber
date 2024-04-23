using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class CurrencyCommand : ConsoleCommand
{
	public CurrencyCommand()
	{
		commandWord = "currency";
		usage = "currency [operation] [amount]\n" + "Operations: modify the amount of currency possessed";
	}

	public override List<string> ValidArgs()
	{
		List<string> args = new List<string> { "increase", "decrease", "change" };
		return args;
	}     

	public override bool Process(string[] args)
	{
		GD.Print(args);

		if (args.Length == 1)
		{
			string arg = args[0].ToLower();
			switch (arg)
			{
				case "checkamount":
					commandOutput = $"current currency: {TempCurrencyManager.GetCurrencyAmount()}";
					break;
				default:
					commandOutput = "invalid arguments";
					return false;
			}
			return true;
		}
		
		if (args.Length >= 2)
		{
			string arg = args[0].ToLower();
			int amount = args[1].ToInt();
			if (args.Length == 2)
			{
				switch (arg)
				{
					case "increase":
						TempCurrencyManager.IncreaseMoney(amount);
						break;
					case "decrease":
						TempCurrencyManager.DecreaseMoney(amount);
						break;
					case "change":
						TempCurrencyManager.ChangeMoney(amount);
						break;
					default:
						commandOutput = "invalid arguments";
						return false;
				}
				commandOutput = $"currency changed to: {TempCurrencyManager.GetCurrencyAmount()}";
				return true;
			}
		}
		return false;
	}

}
