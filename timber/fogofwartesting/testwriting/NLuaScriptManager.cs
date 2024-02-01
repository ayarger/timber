using Godot;
using NAudio.Codecs;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Xml.Linq;

public class NLuaScriptManager : Node
{

    public static int id = 0;
    public static Lua luaState;
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
        catch (NLua.Exceptions.LuaScriptException e)
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
            luaState.DoString($"{objectName} = {{}}");
            luaState.DoString($"setmetatable({objectName}, {{__index = {className}}})");
            luaObjects.Add(objectName);
        }
        catch (NLua.Exceptions.LuaScriptException e)
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
        Instance = this;
        luaState = new Lua();
        registeredClasses = new HashSet<string>();
        luaObjects = new HashSet<string>();
        luaActors = new Dictionary<string, Spatial>();
        luaState.RegisterFunction("print", typeof(NLuaScriptManager).GetMethod("Print"));

        //Initialize global Lua manager. Make global a "global object"?
        //https://manual.gamemaker.io/monthly/en/GameMaker_Language/GML_Overview/Variables/Global_Variables.htm

        /*
        Godot.File global = new Godot.File();
        global.Open($"fogofwartesting/testwriting/{globalClass}.lua", Godot.File.ModeFlags.Read);
        luaState.DoString(global.GetAsText());
        */

        //FOR TESTING
        Godot.File x = new Godot.File();
        x.Open($"fogofwartesting/testwriting/{testClassName}.lua", Godot.File.ModeFlags.Read);
        RegisterClass(x, testClassName);
        string objectName = GenerateObjectName();





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
        //To be migrated to global.lua.
        timer -= delta;
        if(timer <= 0)
        {
            timer = 1f;
            foreach (string obj in luaObjects)
            {
                var res = luaState.DoString($"return {obj}:on_second()")[0] as string;
                GD.Print(res);
                int amt = int.Parse(res.Substring(1));
                luaActors[obj].GlobalTranslation = new Vector3(amt, 0, 0);
            }
        }
    }
}

/// <summary>
/// Represents an instance of a lua script.
/// There can be multiple LuaObjects per Lua script, achieveing OOP.
/// However, all objects need a unique name (objectName). NLuaScriptManager.GenerateObjectName() can help with this.
/// </summary>
public class LuaObject
{
}

public class LuaExceptionEvent
{
    public LuaExceptionEvent()
    {

    }
}
