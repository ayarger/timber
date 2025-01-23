using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class LuaConsoleCommand : ConsoleCommand
{
    public LuaConsoleCommand()
    {
        commandWord = "lua";
        usage = "dont use this (for now)";
    }

    public override List<string> ValidArgs()
    {
        List<string> args = new List<string> { "reload" };
        return args;
    }

    public override bool Process(string[] args)
    {
        GD.Print(args);

        if (args.Length >= 2)
        {
            string arg = args[0].ToLower();
            string arg2 = args[1].ToLower();
            if (args.Length == 2)
            {
                switch (arg)
                {
                    case "reload":
                        Godot.File y = new Godot.File();
                        y.Open($"LuaEngine/{arg2}.lua", Godot.File.ModeFlags.Read);
                        LuaRegistry.ReloadClass(y, arg2);
                        break;
                    default:
                        commandOutput = "invalid arguments";
                        return false;
                }
                commandOutput = $"lua command finished";
                return true;
            }
        }
        return false;
    }

}
