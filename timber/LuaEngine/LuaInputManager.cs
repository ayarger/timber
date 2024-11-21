using Godot;
using Godot.Collections;
using System;

public class LuaInputManager : LuaSingleton
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public static string[] usableKeys = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    public static Dictionary<string, bool> previousKeys = new Dictionary<string,bool>();

    public static LuaInputManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new LuaInputManager();
            }
            return _instance;
        }
    }

    private static LuaInputManager _instance = null;

    public override void Start()
    {
        GD.Print("staryting dsfjlkdsjf");
        foreach (var key in usableKeys)
        {
            previousKeys.Add(key, false);
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void Update()
    {
        foreach (var key in usableKeys)
        {
            previousKeys[key] = Input.IsKeyPressed(OS.FindScancodeFromString(key)) ? true : false;
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
