using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class EditModeEvent
{
    public bool activated;
    public EditModeEvent(bool _activated)
    {
        bool activated = _activated;
    }
}

public class ConsoleManager : Control
{
    [Export]
    public Panel consolePanel;
    private LineEdit consoleInput;
    private TextEdit consoleOutput;
    private Spatial curr_actor;
    private string lastCommand;

    [Export]
    List<string> commands = new List<string>();

    [Export]
    List<ConsoleCommand> commandList = new List<ConsoleCommand>();

    [Export]
    // this is configured through actor.cs when actors were loaded into the scene tree
    public Dictionary<string, Actor> ActorDict = new Dictionary<string, Actor>();

    [Export]
    ConsoleCommand curr_match;

    [Export]
    List<string> matchingCommands = new List<string>();


    private int autocompleteIndex = -1;
    private string baseInput = "";

    public override void _Ready()
    {
        consolePanel = GetNode<Panel>("../ConsolePanel");
        consoleInput = GetNode<LineEdit>("../ConsolePanel/Input");
        consoleOutput = GetNode<TextEdit>("../ConsolePanel/Output");
        consoleInput.Connect("text_entered", this, nameof(OnCommandEntered));
        consoleInput.Connect("text_changed", this, nameof(OnSuggestionSelected));
        //GetAllCommands();
        //hoInstantiateCommands();
        LoadCommands("DeveloperConsole/Commands/");

        // make sure the console is accessible when the game is on pause
        // TODO Pause cases
        //PauseMode = PauseModeEnum.Process;
        //consolePanel.PauseMode = PauseModeEnum.Process;
        curr_match = commandList.FirstOrDefault(cmd => cmd.CommandWord.StartsWith(""));
        foreach (string arg in curr_match.ValidArgs())
        {
            consoleOutput.Text += $"{curr_match.CommandWord} {arg}\n";
        }
    }

    //TODO iterate through the folder to find all command.tscn
    public void LoadCommands(string folderPath)
    {
        var systemFolderPath = ProjectSettings.GlobalizePath(folderPath);

        // Ensure the folder exists
        if (System.IO.Directory.Exists(systemFolderPath))
        {
            foreach (var filePath in System.IO.Directory.GetFiles(systemFolderPath))
            {
                if (filePath.EndsWith(".tscn"))
                {
                    var scenePath = "res://" + filePath;
                    var scene = (PackedScene)GD.Load(scenePath);
                    if (scene != null)
                    {
                        ConsoleCommand curr_command = (ConsoleCommand)scene.Instance();
                        AddChild(curr_command);
                        GD.Print(curr_command.GetType());
                        commandList.Add(curr_command);
                    }
                }
            }
        }

        else
        {
            GD.Print("Folder not found: ", folderPath);
        }

    }

    /// <summary>
    /// Input control for console panel toggle, fetching last command entered and autocompletion 
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        // Toggle console panel visibility on pressing [`]
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Scancode == 96)
            {
                consolePanel.Visible = !consolePanel.Visible;
                //Publish editMode event
                EventBus.Publish(new EditModeEvent(consolePanel.Visible));
                GD.Print($"EditModeOn:{consolePanel.Visible}");
                //automatically focus on the lineEdit when console is visible
                if (consolePanel.Visible) consoleInput.GrabFocus();
                // Pause the game while accessing the command console
                // TODO some of the functionality should remain accessible

                //GetTree().Paused = consolePanel.Visible;
            }

            // fetch last command entered
            if (eventKey.Scancode == (uint)KeyList.Up && consolePanel.Visible)
            {
                consoleInput.Text = lastCommand;
                consoleInput.GrabFocus();
                consoleInput.CaretPosition = lastCommand.Length;
                GD.Print(consoleInput.CaretPosition);
            }

            // autocompletion
            if (eventKey.Scancode == (uint)KeyList.Tab && consolePanel.Visible)
            {
                consoleInput.Text = curr_match.CommandWord;
                if(matchingCommands.Count > 0)
                {
                    consoleInput.Text += $" {matchingCommands[0]}";
                }
                consoleInput.GrabFocus();
                consoleInput.CaretPosition = consoleInput.Text.Length;
            }
        }
    }

    private void OnCommandEntered(string input)
    {
        //clear last input
        consoleInput.GrabFocus();
        consoleInput.Clear();
        //consoleOutput.Text = "";
        GD.Print("Command entered: " + input);

        lastCommand = input;
        //ParseCommand(input);
        ProcessCommand(input);
    }

    //TODO Show auto complete suggestion in a different window
    private void OnSuggestionSelected(string inputText)
    {
        string[] input_string = inputText.Split(' ');
        string input_command = input_string[0];
        string[] curr_args = new string[input_string.Length - 1];
        Array.Copy(input_string, 1, curr_args, 0, input_string.Length - 1);
        // search for matching command word
        curr_match = commandList.FirstOrDefault(cmd => cmd.CommandWord.StartsWith(input_command));


        // serach for matching arguments if curr_args is not empty
        if (curr_args.Length > 0)
        {
            GD.Print($"curr match in : {curr_match.CommandWord}\n");
            GD.Print($"curr args: {curr_args[0]}\n");
            matchingCommands = curr_match.FindMatchingCommands(curr_args);
            foreach (string match in matchingCommands)
            {
                GD.Print("match found");
                consoleOutput.Text = $"{curr_match.CommandWord} {match}";
                //show actor names if curr_match uses actor info after mathcing args found
                if (curr_match.NeedActroInfo)
                {
                    ShowActors();
                }
            }
        }
    }

    public void ProcessCommand (string input)
    {
        if(input.ToLower() == "help")
        {
            ShowHelp();
            return;
        }

        string[] input_string = input.Split(' ');
        consoleOutput.Text = $"Command: {input}\n{consoleOutput.Text}";
        string input_command = input_string[0]; //parse command
        string[] curr_args = new string[input_string.Length - 1];// the rest are arguments
        Array.Copy(input_string, 1, curr_args, 0, input_string.Length - 1);

        //starts comparing input to commandList
        foreach(var command in commandList)
        {
            if (!input_command.Equals(command.CommandWord, StringComparison.OrdinalIgnoreCase))
            {
                consoleOutput.Text = $"invalid command\n{consoleOutput.Text}";
                return;
            }

            if (command.Process(curr_args))
            {
                //update consoleOutput based on process result
                consoleOutput.Text = $"{command.CommandOutput}\n{consoleOutput.Text}";
            }

            else
            {
                consoleOutput.Text = $"invalid arguments\n{consoleOutput.Text}";
            }
        }
    }


    void ShowHelp()
    {
        commandList.ForEach(item => consoleOutput.Text = $"{item.Usage}\n{consoleOutput.Text}");
    }

    //TODO: show Actors in a popup or make the name tag available
    void ShowActors()
    {
        foreach (string key in ActorDict.Keys)
        {
            consoleOutput.Text = $"{consoleOutput.Text}\n{key}";
        }
    }

    void HideActors()
    {

    }
}