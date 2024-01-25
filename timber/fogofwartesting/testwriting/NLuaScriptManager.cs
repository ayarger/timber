using Godot;
using NAudio.Codecs;
using NLua;
using System;
using System.Collections;

public class NLuaScriptManager : Node
{

    Lua luaState;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    string className;

    public static int id = 0;
    public static string GenerateObjectName()
    {
        id++;
        return $"obj_{id}";
    }

    public static void Print(object a)
    {
        GD.Print(a);
    }
    public override void _Ready()
    {
        luaState = new Lua();
        luaState.RegisterFunction("print", typeof(NLuaScriptManager).GetMethod("Print"));

        //Initialize global lua template
        className = "otherscript";
        Godot.File x = new Godot.File();
        x.Open($"fogofwartesting/testwriting/{className}.lua", Godot.File.ModeFlags.Read);
        String master = x.GetAsText();
        GD.Print(master);
        luaState.DoString(master);

        for (int i = 0; i < 10000; i++)
        {


        }

        //ArborCoroutine.StartCoroutine(SimulateCoro(0));
        x.Close();
    }

    //IEnumerator SimulateCoro(int id)
    //{
    //    yield return ArborCoroutine.WaitForSeconds(3.0f);
    //    DateTime dateTime = DateTime.Now;
    //    luaNode.Call("runcoro", id);
    //    while (true)
    //    {
    //        yield return null;
    //        if (amt <= 0)
    //        {
    //            break;
    //        }
    //    }
    //    GD.Print($"Elapsed Time: {(DateTime.Now - dateTime).TotalMilliseconds}");
    //}


    // called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        string name = GenerateObjectName();
        luaState.DoString($"{name} = {{}}");
        luaState.DoString($"setmetatable({name}, {{__index = {className}}})");
        DNDStressTest.LogTimeOfEvent(() =>
        {
            //Copy lua template to new object
            luaState.DoString($"{name}:ready()");
            //for (int i = 0; i < 10000; i++)
            //{
            //    DynValue res = script.Call(script.Globals[$"{name}:ready()"], 4);
            //    script.Globals[name].
            //}
        });
    }
}

