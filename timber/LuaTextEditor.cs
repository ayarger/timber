using Godot;
using System;

public class LuaTextEditor : Tabs
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private TextEdit _textEdit;
    private TabContainer _tabContainer;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _textEdit = GetNode<TextEdit>("TextEdit");
        //_tabContainer = GetNode<TabContainer>("TabContainer");

        Godot.File x = new Godot.File();
        x.Open($"LuaEngine/{NLuaScriptManager.testClassName}.lua", Godot.File.ModeFlags.Read);
        string fileContents = x.GetAsText();

        _textEdit.Text = fileContents;

        ResetTabs();
    }

    public void ResetTabs()
    {
        //foreach (Node n in _tabContainer.GetChildren())
        //{
        //    _tabContainer.RemoveChild(n);
        //    n.QueueFree();
        //}

        //string[] scripts = new string[] { $"LuaEngine/{NLuaScriptManager.testClassName}.lua",
        //    $"LuaEngine/{NLuaScriptManager.testClassNameDialogue}.lua"};

        //foreach (string script in scripts)
        //{
        //    Tabs newTab = new Tabs();
        //    newTab.Name = script;
        //    _tabContainer.AddChild(newTab);
        //}

    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnTextEditExitFocus()
    {
        SaveText();
    }

    public void SaveText()
    {
        GD.Print("SAVE TEXT HERE!");
    }
}
