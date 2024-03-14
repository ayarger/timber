using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class StatCommand : ConsoleCommand
{
    public StatCommand()
    {
        commandWord = "stat";
        usage = "stat [operation] [actorName] [statname] ([amount])\n" + "operatons: increase decrease change max create get";
    }

    public override List<string> ValidArgs()
    {
        List<string> args = new List<string> { "increase", "decrease", "change", "max", "create","get" };
        return args;
    }     

    public override bool Process(string[] args)
    {
        GD.Print(args);

        if (args.Length >= 3)
        {
            string arg = args[0].ToLower();
            string actorName = args[1].ToLower();
            string statName = args[2].ToLower();
            Actor curr_actor = GetActorByName(actorName);
            if (args.Length == 3)
            {
                if (arg == "get")
                {
                    Stat curr_stat = curr_actor.GetNode<HasStats>("HasStats").GetStat(statName);
                    if (curr_stat != null)
                    {
                        commandOutput = $"{actorName} {statName} :  {curr_stat.currVal} / {curr_stat.maxVal}";
                        return true;
                    }
                }
            }

            if (args.Length == 4)
            {
                int amount = int.Parse(args[3]);

                if (curr_actor != null)
                {
                    Stat curr_stat = curr_actor.GetNode<HasStats>("HasStats").GetStat(statName);
                    switch (arg)
                    {
                        default:
                            commandOutput = "invalid arguments";
                            return false;

                        case "increase":
                            curr_stat.IncreaseCurrentValue(amount);
                            break;
                        case "decrease":
                            curr_stat.DecreaseCurrentValue(amount);
                            break;
                        case "change":
                            curr_stat.SetVal(amount);
                            break;
                        case "max":
                            curr_stat.SetMaxVal(amount);
                            break;
                        case "create":
                            curr_actor.GetNode<HasStats>("HasStats").AddStat(args[2], 0, 100, amount, false);
                            curr_stat = curr_actor.GetNode<HasStats>("HasStats").GetStat(args[2]);
                            break;
                    }

                    commandOutput = $"{actorName} {statName} changed to: {curr_stat.currVal} / {curr_stat.maxVal}";
                    return true;

                }
            }

        }
        return false;
    }

}
