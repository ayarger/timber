using Godot;
using MoonSharp.Interpreter.Interop.LuaStateInterop;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using Amazon.Runtime.Internal.Transform;
using Newtonsoft.Json.Linq.JsonPath;
using Antlr4.Runtime.Dfa;


//Contains all info relating to lua actors, classes, and objects.
public class LuaRegistry
{
    //Global references to all objects. 
    private static HashSet<LuaObjectData> luaObjects;
    private static Dictionary<string, Spatial> nameToActor; //This can have duplicate actors, if actors have more than one script.
    public static Dictionary<string, HashSet<LuaObjectData>> classToLuaObject { get; private set; }
    public static HashSet<string> needToRunReady { get; private set; }

    public static void Reset()
    {
        luaObjects = new HashSet<LuaObjectData>();
        needToRunReady = new HashSet<string>();
        nameToActor = new Dictionary<string, Spatial>();
        classToLuaObject = new Dictionary<string, HashSet<LuaObjectData>>();
    }

    public static Dictionary<string, Spatial> luaActors
    {
        get
        {
            return nameToActor;
        }
    }


    //Must be ran before making any instances of a class. 
    public static void RegisterClass(File rootFile, string className)
    {
        if (classToLuaObject.ContainsKey(className))
        {
            GD.PushWarning($"Class {className} is already registered! This can happen if multiple actors with the same script are loaded simultaneously.");
            return;
        }
        if (className == NLuaScriptManager.globalClass)
        {
            GD.PushError($"Cannot name a class identically to the global class \"{NLuaScriptManager.globalClass}\"!");
            return;
        }
        try
        {
            NLuaScriptManager.luaState.DoString(rootFile.GetAsText());
        }
        catch (InterpreterException e)
        {
            GD.PushError($"Exception caught in {className}\n{e.Message}");
            NLuaScriptManager.ShowException(className, e.Message);
            //Case of exception
            EventBus.Publish(new LuaExceptionEvent());
            return;
        }
        classToLuaObject[className] = new HashSet<LuaObjectData>();
        GD.Print($"Registered Lua Class: {className}");
    }
    public static void RegisterActor(LuaObjectData obj)
    {
        if (!classToLuaObject.ContainsKey(obj.ClassName))
        {
            return;
        }
        luaObjects.Add(obj);
        nameToActor[obj.Name] = obj.Actor;
        needToRunReady.Add(obj.Name);
        classToLuaObject[obj.ClassName].Add(obj);
        GD.Print(obj.Name);
    }

    public static bool ContainsClass(string className)
    {
        return classToLuaObject.ContainsKey(className);
    }

    public static void DeregisterActor(string name)
    {
        LuaObjectData toRemove = FindActorData(name);
        luaObjects.Remove(toRemove);
        nameToActor.Remove(name);
        classToLuaObject[toRemove.ClassName].Remove(toRemove);
        NLuaScriptManager.luaState.DoString($"{name} = nil\n" +
            $"global.game_objects[\"{name}\"] = nil");
    }

    public static LuaObjectData FindActorData(string name)
    {
        LuaObjectData answer = null;
        foreach (var i in luaObjects)
        {
            if (i.Name == name)
            {
                answer = i;
                break;
            }
        }
        return answer;
    }

    //FIX THIS LATER
    public static void RenameClass(string name, string newName)
    {
        if (!classToLuaObject.ContainsKey(name))
        {
            throw new LuaException("Class did not exist before renaming!");
        }
        NLuaScriptManager.luaState.DoString($"{newName}={name}\n{name}=nil");
        HashSet<LuaObjectData> objs = classToLuaObject[name];
        classToLuaObject.Remove(name);
        classToLuaObject[newName] = objs;
    }

    public static void DestroyClass(string name)
    {
        if (!classToLuaObject.ContainsKey(name))
        {
            throw new LuaException("Class did not exist before destroying!");
        }
        HashSet<LuaObjectData> objs = new HashSet<LuaObjectData>(classToLuaObject[name]);

        foreach(var obj in objs)
        {
            DeregisterActor(obj.Name);
        }
        classToLuaObject.Remove(name);
    }

    //Calling nluascript manager is cursed but oh well, might refactor later
    public static void ReloadClass(File rootFile, string name)
    {
        HashSet<LuaObjectData> objs = new HashSet<LuaObjectData>(classToLuaObject[name]);
        foreach (var obj in objs)
        {
            GD.Print("Reloading: "+obj.Name);
            NLuaScriptManager.Instance.KillActor(obj.Actor);
        }

        try
        {
            NLuaScriptManager.luaState.DoString(rootFile.GetAsText());
        }
        catch (InterpreterException e)
        {
            GD.PushError($"Exception caught in {name}\n{e.Message}");
            NLuaScriptManager.ShowException(name, e.Message);
            //Case of exception
            EventBus.Publish(new LuaExceptionEvent());
            classToLuaObject.Remove(name);
            return;
        }
        classToLuaObject[name] = new HashSet<LuaObjectData>();
        foreach(var obj in objs)
        {
            NLuaScriptManager.Instance.CreateActor(obj.ClassName, obj.Name, obj.Actor);
        }
        GD.Print($"Reloaded Lua Class: {name}");
    }

    public static Spatial GetActor(string name)
    {
        if (!luaActors.ContainsKey(name)) return null;
        return luaActors[name];
    }

    public static bool ContainsActor(string name)
    {
        return nameToActor.ContainsKey(name);
    }

    public static void ClearReady()
    {
        needToRunReady.Clear();
    }
}

public class LuaObjectData
{
    public LuaObjectData(string name, string className, Spatial actor)
    {
        Name = name;
        ClassName = className;
        Actor = actor;
    }

    public string Name;
    public string ClassName;
    public Spatial Actor;
}
