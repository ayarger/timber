using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    public Dictionary<string, Actor> ActorDict;
    //autoComplete multiple matches
    private List<string> matchingCommands = new List<string>();
    private int autocompleteIndex = -1;
    private string baseInput = "";

    public override void _Ready()
    {
        consolePanel = GetNode<Panel>("../ConsolePanel");
        consoleInput = GetNode<LineEdit>("../ConsolePanel/Input");
        consoleOutput = GetNode<TextEdit>("../ConsolePanel/Output");
        // Initialize actor dictionary
        ActorDict = GetAllActors();
        consoleInput.Connect("text_entered", this, nameof(OnCommandEntered));
        consoleInput.Connect("text_changed", this, nameof(OnSuggestionSelected));
        GetAllCommands();
        InstantiateCommands();
        // make sure the console is accessible when the game is on pause
        PauseMode = PauseModeEnum.Process;
        consolePanel.PauseMode = PauseModeEnum.Process;
    }

    public void InstantiateCommands()
    {
        PackedScene scene = (PackedScene)ResourceLoader.Load("res://Commands/StatCommand.tscn");
        StatCommand statCommand = (StatCommand)scene.Instance();
        statCommand.Name = "StatCommand";
        AddChild(statCommand);
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
                GetTree().Paused = consolePanel.Visible;
            }

            if (eventKey.Scancode == (uint)KeyList.Up && consolePanel.Visible)
            {
                consoleInput.Text = lastCommand;
                consoleInput.CaretPosition = lastCommand.Length;
            }

            if (eventKey.Scancode == (uint)KeyList.Tab && consolePanel.Visible)
            {
                consoleInput.Text = consoleOutput.Text;
                consoleInput.CaretPosition = consoleInput.Text.Length;
            }
        }
    }

    /// <summary>
    /// Get all Actor nodes and names and store in a dictionary
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, Actor> GetAllActors()
    {
        Dictionary<string, Actor> foundNodes = new Dictionary<string, Actor>();
        Node rootNode = GetTree().Root;

        //Recursion
        void FindNodes(Node node)
        {
            if (node is Actor typedNode)
            {
                foundNodes[node.Name.ToLower()] = typedNode;
            }
            foreach (Node child in node.GetChildren())
            {
                FindNodes(child);
            }
        }

        FindNodes(rootNode);
        return foundNodes;
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
        consoleOutput.Text = "";
        GD.Print("Command entered: " + input);
        //TODO parsing and
        lastCommand = input;
        ParseCommand(input);
    }

    private void OnSuggestionSelected(string inputText)
    {
        string match = commands.FirstOrDefault(cmd => cmd.StartsWith(inputText));
        if (!string.IsNullOrEmpty(match))
        {
            consoleOutput.Text = $"{match}\n";
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

        //TODO: health handler
        // LuaLoader/Spot

        void ShowHelp()
        {
            commands.ForEach(item => consoleOutput.Text += $"{item}\n");
            consoleOutput.Text += $"stat increase [string]node_name [string]stat_name [int]amount \n";
            consoleOutput.Text += $"stat decrease [string]node_name [string]stat_name [int]amount \n";
            consoleOutput.Text += $"stat change [string]node_name [string]stat_name [int]amount \n";
            consoleOutput.Text += $"stat max [string]node_name [string]stat_name [int]amount \n";
            consoleOutput.Text += $"stat create [string]node_name [string]stat_name [int]amount \n";
            consoleOutput.Text += $"stat get [string]node_name \n";
        }

    }
}