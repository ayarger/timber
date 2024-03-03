using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
    //autoComplete multiple matches
    private List<string> matchingCommands = new List<string>();
    private int autocompleteIndex = -1;
    private string baseInput = "";

    public override void _Ready()
    {
        consolePanel = GetNode<Panel>("../ConsolePanel");
        consoleInput = GetNode<LineEdit>("../ConsolePanel/Input");
        consoleOutput = GetNode<TextEdit>("../ConsolePanel/Output");
        consoleInput.Connect("text_entered", this, nameof(OnCommandEntered));
        //consoleInput.Connect("text_changed", this, nameof(OnSuggestionSelected));
        //GetAllCommands();
        //hoInstantiateCommands();
        LoadCommands("DeveloperConsole/Commands/");
        // make sure the console is accessible when the game is on pause
        // TODO Pause cases
        //PauseMode = PauseModeEnum.Process;
        //consolePanel.PauseMode = PauseModeEnum.Process;
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

    public override void _Input(InputEvent @event)
    {
        // Toggle console panel visibility on pressing [`]
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Scancode == 96)
            {
                GD.Print("console toggle");
                consolePanel.Visible = !consolePanel.Visible;
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
                consoleInput.Text = consoleOutput.Text;
                consoleInput.GrabFocus();
                consoleInput.CaretPosition = consoleInput.Text.Length;
            }
        }
    }

    private void GetAllCommands()
    {
        commands.Add("stat");
        commands.Add("help");
        commands.Add("random");
        commands.Add("test");
        commands.Add("show");
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

    private void OnSuggestionSelected(string inputText)
    {
        string match = commands.FirstOrDefault(cmd => cmd.StartsWith(inputText));
        if (!string.IsNullOrEmpty(match))
        {
            consoleOutput.Text = $"{match}\n";
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
        consoleOutput.Text += $"Command: {input}\n";
        string input_command = input_string[0]; //parse command
        string[] args = new string[input_string.Length - 1];// the rest are arguments
        Array.Copy(input_string, 1, args, 0, input_string.Length - 1);
        GD.Print(args);
        //starts comparing input to commandList
        foreach(var command in commandList)
        {
            if (!input_command.Equals(command.CommandWord, StringComparison.OrdinalIgnoreCase))
            {
                consoleOutput.Text = $"invalid command\n {consoleOutput.Text}";
                return;
            }

            if (command.Process(args))
            {
                //update consoleOutput based on process result
                consoleOutput.Text = $"{command.CommandOutput}\n {consoleOutput.Text}";
            }

            else
            {
                consoleOutput.Text = $"invalid arguments\n {consoleOutput.Text}";
            }
        }
    }

    //TODO: create a function that resize the output box based on output text

    void ParseCommand(string input)
    {
        string[] inputString = input.Split(' ');
        consoleOutput.Text += $"Command: {input}\n";

        string command = inputString[0]; //parse command
        string[] args = new string[inputString.Length - 1];// the rest are arguments
        Array.Copy(inputString, 1, args, 0, inputString.Length - 1);

        //TODO refacotr stat releated commands(start with name? stat? operations)
        switch (command)
        {
            case "help":
                if (args.Length == 0)
                {
                    ShowHelp();
                }
                break;

            case "stat":
                if (args.Length > 2) {
                    //Get actor
                    curr_actor = GetParent().GetParent().GetParent().GetParent().GetNode<Spatial>($"LuaLoader/{args[1]}");
                    GD.Print(args);
                }

                if (curr_actor != null)
                {
                    //Get specific stat
                    Stat curr_stat = curr_actor.GetNode<HasStats>("HasStats").GetStat(args[2]);

                    if (args.Length == 3)
                    {
                        consoleOutput.Text += $"{args[1]} {curr_stat.name}: {curr_stat.currVal}";
                    }
                    if (args.Length == 4)
                    {
                        int amount = int.Parse(args[3]);
                        switch (args[0])
                        {
                            case "increse":
                                curr_stat.IncreaseCurrentValue(amount);
                                consoleOutput.Text += $"{args[1]} {args[2]} changed to {curr_stat.currVal}";
                                break;
                            case "decrease":
                                curr_stat.DecreaseCurrentValue(amount);
                                consoleOutput.Text += $"{args[1]} {args[2]} changed to {curr_stat.currVal}";
                                break;
                            case "change":
                                curr_stat.SetVal(amount);
                                consoleOutput.Text += $"{args[1]} {args[2]} changed to {curr_stat.currVal}";
                                break;
                            case "max":
                                curr_stat.SetMaxVal(amount);
                                consoleOutput.Text = $"{args[1]} max {args[2]} set to {amount}";
                                break;
                            case "create":
                                curr_actor.GetNode<HasStats>("HasStats").AddStat(args[2],0,100,amount,false);
                                curr_stat = curr_actor.GetNode<HasStats>("HasStats").GetStat(args[2]);
                                consoleOutput.Text = $"{args[1]} {args[2]} created. (current value: {curr_stat.currVal})";
                                break;
                        }
                        
                    }
                }
                break;

            case "random":
                break;

            default:
                consoleOutput.Text = "Invalid command";
                lastCommand = "";
                break;

        }

    }
    void ShowHelp()
    {
        commandList.ForEach(item => consoleOutput.Text += $"{item.Usage}\n");
    }
}