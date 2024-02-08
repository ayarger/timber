using Godot;
using NAudio.Codecs;
using Newtonsoft.Json.Linq.JsonPath;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Xml.Linq;
using Script = MoonSharp.Interpreter.Script;

public class NLuaScriptManager : Node
{

    public static int id = 0;
    public static Script luaState;
    public static HashSet<string> registeredClasses;
    public static string globalClass = "global";
    public static NLuaScriptManager Instance;

    //Global references to all objects. 
    public static HashSet<string> luaObjects;
    public static Dictionary<string,Spatial> luaActors;

    public static string testClassName = "testluaobject";

    //Must be ran before making any instances of a class. 
    public void RegisterClass(File rootFile, string className)
    {
        if (registeredClasses.Contains(className))
        {
            GD.PushWarning($"Class {className} is already registered!");
            return;
        }
        if (className == globalClass)
        {
            GD.PushError($"Cannot name a class identically to the global class \"{globalClass}\"!");
            return;
        }
        try
        {
            luaState.DoString(rootFile.GetAsText());
        }
        catch (InterpreterException e)
        {
            GD.PushError($"Exception caught in {className}\n{e.Message}");
            //Case of exception
            EventBus.Publish(new LuaExceptionEvent());
            return;
        }
        registeredClasses.Add(className);
        GD.Print($"Registered Lua Class: {className}");
    }

    //Create objects of a class. These objects will have their events ran.
    public bool CreateObject(string className, string objectName)
    {
        if (luaObjects.Contains(objectName))
        {
            GD.PushWarning($"Object {objectName} of class {className} already exists!");
            return false;
        }

        try
        {
            luaState.DoString($"{objectName} = {{}}\n" +
                $"setmetatable({objectName}, {{__index = {className}}})\n" +
                $"global:register_object({objectName}, \"{objectName}\")");
            luaObjects.Add(objectName);
        }
        catch (InterpreterException e)
        {
            GD.PushError($"Exception caught in {className}\n{e.Message}");
            //Case of exception
            EventBus.Publish(new LuaExceptionEvent());
            return false;
        }
        GD.Print($"Created {className} Object: {objectName}");
        return true;
    }

    //Create objects of a class, that hosts an actor. Should be the only "front facing" function for users.
    //Will probably replace Actor with ActorConfig.
    public void CreateActor(string className, string objectName, Spatial actor)
    {
        if (!CreateObject(className, objectName))
        {
            return;
        }
        luaActors[objectName] = actor;
    }

    public void RunUntilCompletion(string function, List<string> prms = null)
    {
        string res = "Start";
        luaState.DoString($"timber_runner = coroutine.create({function})\n");
        List<string> args = prms ?? new List<string>();
        while (res != "")
        {
            string prm = "";
            foreach(string arg in args)
            {
                prm += ","+arg;
            }

            res = luaState.DoString($"local co, res = coroutine.resume(timber_runner,global {prm})\n" +
                "return res").String;
            if (res == "" || res == null) break;
            GD.Print("Got response: "+res);
            foreach (string cmd in res.Split('\n'))
            {
                if (cmd == "") continue;
                string name = cmd.Split(':')[0];
                string command = cmd.Split(':')[1];
                if (command.StartsWith("M"))
                {
                    int amt = int.Parse(command.Substring(1));
                    TestMovement.SetDestination(luaActors[name] as Actor, luaActors[name].GlobalTranslation + new Vector3(Grid.tileWidth * amt, 0, 0));
                }
            }
        }
    }


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
        GD.Print("Lua initialized");
        Instance = this;
        luaState = new Script();
        registeredClasses = new HashSet<string>();
        luaObjects = new HashSet<string>();
        luaActors = new Dictionary<string, Spatial>();
        luaState.Globals["print"] = (Action<object>)Print;

        //Initialize global Lua manager. Make global a "global object"?
        //https://manual.gamemaker.io/monthly/en/GameMaker_Language/GML_Overview/Variables/Global_Variables.htm

        Godot.File global = new Godot.File();
        global.Open($"LuaEngine/{globalClass}.lua", Godot.File.ModeFlags.Read);
        luaState.DoString(global.GetAsText());

        //FOR TESTING
        Godot.File x = new Godot.File();
        x.Open($"LuaEngine/{testClassName}.lua", Godot.File.ModeFlags.Read);
        RegisterClass(x, testClassName);
        string objectName = GenerateObjectName();

        GD.Print("Lua initialized");
        GD.Print($"{luaState.DoString("return 5+20")}");



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

    float timer = 1f;
    // called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        RunUntilCompletion("global.tick", new List<string> { $"{delta}" });

        //Test code to run ready every 5 seconds
        timer -= delta;
        if (timer <= 0)
        {
            timer = 5f;
                DNDStressTest.LogTimeOfEvent(() =>
                {
                    RunUntilCompletion("global.receive", new List<string> { "\"ready\"" });
                });

                //int amt = int.Parse(res.Substring(1));
                //luaActors[obj].GlobalTranslation = new Vector3(amt, 0, 0);
        }
    }
}

public class LuaExceptionEvent
{
    public LuaExceptionEvent()
    {

    }
}