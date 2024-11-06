using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An abstract class representing a console command
/// </summary>
public abstract class ConsoleCommand : Node
{
    [Export] protected string commandWord = string.Empty;


    [Export] protected string usage = "";


    [Export] protected string commandOutput = "";

    [Export] protected bool needActorInfo = false;


    public string CommandWord => commandWord;

    public string Usage => usage;

    public string CommandOutput => commandOutput;

    public bool NeedActroInfo => needActorInfo;

    /// <summary>
    /// A list of predefined valid args for the command for error handling and autocomplete
    /// </summary>
    /// <returns></returns>
    public abstract List<string> ValidArgs();

    /// <summary>
    /// Parse then execute the command, use switch(arg) for different cases if needed
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract bool Process(string[] args);



    /// <summary>
    /// Find matching commands from valid console commands based on console input
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public virtual List<string> FindMatchingCommands(string[] args)
    {
        List<string> matchingCommand = new List<string>();
        if (args.Length > 1)
        {
            return new List<string>();
        }

        foreach (string validArg in ValidArgs())
        {
            if (validArg.ToLower().StartsWith(args[0].ToLower()))
            {
                matchingCommand.Add(validArg.ToLower());
            }
        }

        return matchingCommand;
    }

    /// <summary>
    /// Return the actor node by name and return null if no match found
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual Actor GetActorByName(string name)
    {
        ConsoleManager manager = (ConsoleManager)GetParent();
        return manager.ActorDict.TryGetValue(name, out var node) ? node : null;
    }

}