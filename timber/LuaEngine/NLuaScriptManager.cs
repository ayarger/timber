using Godot;
using NAudio.Codecs;
using Newtonsoft.Json.Linq.JsonPath;
using Newtonsoft.Json;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Xml.Linq;
using Script = MoonSharp.Interpreter.Script;
using MoonSharp.Interpreter.Loaders;

public class NLuaScriptManager : Node
{

    public static int id = 0;
    public static Script luaState;
    public static HashSet<string> registeredClasses;
    public static string globalClass = "global";
    public static NLuaScriptManager Instance;

    //Global references to all objects. 
    public static HashSet<string> luaObjects;
    public static Dictionary<string,Spatial> luaActors; //This can have duplicate actors, if actors have more than one script.

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
                $"setmetatable({objectName}, {{__index = function(self, key) \r\n  if global.keywords[key]  then\r\n\treturn GetValue(rawget(self,\"object_name\"),key)\r\n  else\r\n\treturn rawget({className}, key)\r\n  end\r\nend}})\n" +
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
    //This can and should be called multiple times on the same actor to have multiple scripts per actor.
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
        string data = "";

        //Run until we do not have any commands from Lua
        while (res != "")
        {

            string prm = "";
            foreach(string arg in args)
            {
                prm += ","+arg;
            }

            //Run our coroutine with any required parameters (currently, just delta if we are running a tick update).
            res = luaState.DoString($"local co, res = coroutine.resume(timber_runner,global {prm}, {{{data}}})\n" +
                "return res").String;

            if (res == "{}" || res == ""||res == null) break;
            GD.Print("Got response: "+res);
            data = "";
            Dictionary<string, object>[] cmds = JsonConvert.DeserializeObject<Dictionary<string,object>[]>(res);

            //Run every command that was returned from Lua
            foreach (Dictionary<string, object> cmd in cmds)
            {
                object result = HandleCommand(cmd);
                if (result != null)
                {
                    string name = Convert.ToString(cmd["obj"]);
                    data += name + "=" + result; //Replace with serializing JSON?
                    data += ",";
                }

            }
        }
    }

    //Handles commands. The contents of cmd is determined by the appropriate command in Lua, and needs to be manually converted.
    //Every command should have a name (the object name), and a command type. Everything else is determined by the appropriate API.
    public static object HandleCommand(Dictionary<string, object> cmd)
    {
        string name = Convert.ToString(cmd["obj"]);
        string command = Convert.ToString(cmd["type"]);
        Actor actor = luaActors.ContainsKey(name) ? luaActors[name] as Actor : null;

        //Parse commands. Will need to refactor to a better system.
        if (command == "M")
        {
            int amtX = Convert.ToInt32(cmd["x"]);
            int amtZ = Convert.ToInt32(cmd["z"]);
            TestMovement.SetDestination(actor, new Vector3(Grid.tileWidth * amtX, luaActors[name].GlobalTranslation.y, Grid.tileWidth * amtZ));
        }
        else if (command == "P")
        {
            //Replace with toast later
            GD.Print(Convert.ToString(cmd["param"]));
        }
        else if (command == "R")
        {
            //Same deal with newlines
            //Get data, just x position rn for demonstration
            string key = Convert.ToString(cmd["param"]);
            if (key == "x")
            {
                return luaActors[name].GlobalTranslation.x / Grid.tileWidth;
            }
            else if (key=="z")
            {
                return luaActors[name].GlobalTranslation.z / Grid.tileWidth;
            }
        }
        else if(command == "T")
        {
            GD.Print(actor.ToString() + " just posted " + cmd["toastString"] + " to the toast!");

        }
        else if (command == "H")
        {
            int damage = Convert.ToInt32(cmd["damage"]);
            actor.Hurt(damage, false, null);
        }
        else if (command == "K")
        {
            string killSourceName = Convert.ToString(cmd["obj"]);
            Actor source = luaActors.ContainsKey(killSourceName) ? luaActors[killSourceName] as Actor : null;
            actor.Kill(source);
        }

        return null;
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
        luaState.Options.ScriptLoader = new FileSystemScriptLoader();

        //This may cause issues
        string abspath = $"{System.IO.Directory.GetCurrentDirectory()}/LuaEngine/testmodules/";
        ((ScriptLoaderBase)luaState.Options.ScriptLoader).ModulePaths = new string[] { abspath+"?", $"{abspath}?.lua", $"{abspath}/lunajson/?", $"{abspath}/lunajson/?.lua" };
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
                    RunUntilCompletion("global.receive", new List<string> { "\"testfunc\"" });
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