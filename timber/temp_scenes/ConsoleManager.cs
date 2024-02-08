using Godot;
using System;

public class ConsoleManager : Control
{
    [Export]
    public Panel consolePanel;
    private LineEdit consoleInput;
    private TextEdit consoleOutput;
    private Spatial curr_actor;

    public override void _Ready()
    {
        consolePanel = GetNode<Panel>("../ConsolePanel");
        consoleInput = GetNode<LineEdit>("../ConsolePanel/Input");
        consoleOutput = GetNode<TextEdit>("../ConsolePanel/Output");
        consoleInput.Connect("text_entered", this, nameof(OnCommandEntered));

        // make sure the console is accessible when the game is on pause
        PauseMode = PauseModeEnum.Process;
        consolePanel.PauseMode = PauseModeEnum.Process;
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
                if(consolePanel.Visible) consoleInput.GrabFocus();
                // Pause the game while accessing the command console
                GetTree().Paused = consolePanel.Visible;
            }
        }
    }

    private void OnCommandEntered(string input)
    {
        //clear last input
        consoleInput.GrabFocus();
        consoleInput.Clear();
        consoleOutput.Text = "";
        GD.Print("Command entered: " + input);
        //TODO parsing and

        ParseCommand(input);
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

            case "addHealth":
                if(args.Length == 2)
                {
                    if (args[0] == "Spot")
                    {
                        int amount = int.Parse(args[1]);
                        curr_actor = GetParent().GetParent().GetParent().GetParent().GetNode<Spatial>($"LuaLoader/{args[0]}");
                        Stat curr_stat = curr_actor.GetNode<HasStats>("HasStats").Stats["health"];
                        curr_stat.IncreaseCurrentValue(amount);
                        consoleOutput.Text += $"{args[0]} health changed to {curr_stat.currVal}";
                    }
                }
                break;

            default:
                consoleOutput.Text = "Invalid command";
                break;

        }

        //TODO: health handler
        // LuaLoader/Spot

        void ShowHelp()
        {
            consoleOutput.Text += $"addHealth [string]node_name [int]amount\n";
        }

    }
}