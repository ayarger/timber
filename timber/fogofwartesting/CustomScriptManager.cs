using Godot;
using NAudio.Codecs;
using System;
using System.Collections;

public class CustomScriptManager : Node
{
    [Signal]
    public delegate void ResumeCoroutine(string a);

    [Export]
    public string testData = "This data is on C#!";


    Node luaNode;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    int amt = 900;
    DateTime prevdateTime = DateTime.Now;
    public override void _Ready()
    {
        //GD.Print("Hello");
        luaNode = GetChild(0);
        luaNode.Connect("awaitaction", this, "Response");
        //x.SetScript((Script)GD.Load("res://fogofwartesting/otherscript.lua"));
        luaNode.Call("init");
        ArborCoroutine.StartCoroutine(SimulateCoro(0));
    }

    public override void _Process(float delta)
    {
        luaNode.Call("startcoro", 0);
        luaNode.Call("runcoro", 0);
    }

    void Response(object a)
    {
        amt--;
        //GD.Print($"Elapsed Time: {(DateTime.Now - prevdateTime).TotalMilliseconds}");
        prevdateTime = DateTime.Now;
        EmitSignal("ResumeCoroutine","Wow, I got data back!");
    }

    IEnumerator SimulateCoro(int id)
    {
        yield return ArborCoroutine.WaitForSeconds(3.0f);
        luaNode.Call("startcoro", id);
        DateTime dateTime = DateTime.Now;
        luaNode.Call("runcoro", id);
        while (true)
        {
            yield return null;
            if (amt <= 0)
            {
                break;
            }
            //object o = luaNode.Call("runcoro", rand);
            //string code = "exit";
            //try
            //{
            //    code = (string)o;
            //    if (code == null)
            //    {
            //        throw new InvalidCastException();
            //    }
            //}
            //catch (InvalidCastException e)
            //{
            //    GD.PrintErr("Coroutine returned a non-string!\n", e.Message);
            //    break;
            //    //yield break;
            //}

            //if (code[0] == 'W')
            //{
            //    float time = float.Parse(code.Substring(1));
            //    //GD.Print($"Waiting {time} seconds");
            //    //yield return ArborCoroutine.WaitForSeconds(time);
            //    EmitSignal("ResumeCoroutine", rand);
            //}

            //GD.Print(o.GetType());
        }
        GD.Print($"Elapsed Time: {(DateTime.Now-dateTime).TotalMilliseconds}");
    }


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
