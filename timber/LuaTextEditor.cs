using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class LuaTextEditor : Tabs
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private TextEdit _textEdit;
    private VBoxContainer _vboxContainer;
    private Label _errorLabel;
    private ColorRect _errorBg;
    private Label _scriptName;

    private DateTime time;
    private bool changed = false;

    private string selected = "";

    [Export]
    public Color commentColor = new Color(0, 1, 0, 1);
    [Export]
    public Color stringColor = new Color(0, 1, 1, 1);
    [Export]
    public Color keywordColor = new Color(1, 0, 0, 1);
    [Export]
    public Color luaSyntaxColor = new Color(1, 0, 1, 1);
    [Export]
    public Color constantsColor = new Color(0, 0, 1, 1);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _textEdit = GetNode<TextEdit>("TextEdit");
        _errorLabel = GetNode<Label>("TextEdit/ErrorBG/ErrorLabel");
        _errorBg = GetNode<ColorRect>("TextEdit/ErrorBG");
        _vboxContainer = GetNode<ScrollContainer>("ScrollContainer").GetNode<VBoxContainer>("VBoxContainer");
        _scriptName = GetNode<Label>("ScriptName");

        _scriptName.Text = selected;

        time = DateTime.Now;

        _textEdit.AddColorRegion("--", "\n", commentColor, true);


        _textEdit.AddColorRegion("\"", "\"", stringColor);

        //TODO: Just for demonstration. We'll need to add a lot more keywords...
        List<string> keywords = new List<string>() { "self", "global" };
        foreach(string keyword in keywords)
        {
            _textEdit.AddKeywordColor(keyword, keywordColor);
        }


        List<string> syntaxKeywords = new List<string>() { "function", "end", "then", "if", "local", "elseif", "for", "do", "while" };
        foreach (string keyword in syntaxKeywords)
        {
            _textEdit.AddKeywordColor(keyword, luaSyntaxColor);
        }


        List<string> constants = new List<string>() { "false", "true", "not" };
        foreach (string keyword in constants)
        {
            _textEdit.AddKeywordColor(keyword, constantsColor);
        }

        selected = "";

        _textEdit.Text = "";

        ResetTabs();
    }

    public override void _Process(float delta)
    {
        if(changed && (DateTime.Now-time) > TimeSpan.FromSeconds(2))
        {
            changed = true;
            CheckErrors();
        }
    }

    public void ResetTabs()
    {
        foreach (Node n in _vboxContainer.GetChildren())
        {
            _vboxContainer.RemoveChild(n);
            n.QueueFree();
        }

        string[] scripts = new string[] { $"{NLuaScriptManager.testClassName}.lua",
            $"{NLuaScriptManager.testClassNameDialogue}.lua"};

        foreach (string script in scripts)
        {
            var newButton = ResourceLoader.Load<PackedScene>("res://EditorComponents/ScriptButton.tscn").Instance();
            newButton.Name = script;
            newButton.GetNode<Label>("MarginContainer/Label").Text = script;
            newButton.GetNode<Button>("Button").Connect("pressed", this, "OnSelected", new Godot.Collections.Array { script });
            _vboxContainer.AddChild(newButton);
        }

    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void OnTextChanged()
    {
        changed = true;
        time = DateTime.Now;
    }

    public void OnSelected(string script)
    {
        ArborCoroutine.StartCoroutine(OnSelectedCoro(script));
    }
    private IEnumerator OnSelectedCoro(string script)
    {
        SaveText();
        _textEdit.Text = "Loading...";
        ArborResource.Load<string>("scripts/" + script);
        yield return ArborResource.WaitFor("scripts/" + script);
        _textEdit.Text = ArborResource.Get<string>("scripts/" + script);
        selected = script;
        _scriptName.Text = selected;
        CheckErrors();
    }

    public void OnTextEditExitFocus()
    {
        SaveText();
    }

    public void CheckErrors()
    {
        List<NLuaScriptManager.ErrorData> errors = NLuaScriptManager.Linter(_textEdit.Text);

        for (int i = 0; i < _textEdit.GetLineCount(); i++)
        {
            _textEdit.SetLineAsBookmark(i, false);
        }
        _errorLabel.Text = "";
        _errorBg.Color = new Color(1, 1, 1, 0);
        foreach (var error in errors)
        {
            for (int i = error.FromLine; i <= error.ToLine; i++)
            {
                _textEdit.SetLineAsBookmark(i - 1, true);
                _errorLabel.Text = error.Message;
                _errorBg.Color = new Color(1, 1, 1, 1);
            }
        }
    }
    public void SaveText()
    {
        GD.Print("SAVE TEXT HERE!");
        if (selected == "")
        {
            return;
        }

       
        ArborResource.WriteObject<string>($"scripts/{selected}", _textEdit.Text);
    }
}
