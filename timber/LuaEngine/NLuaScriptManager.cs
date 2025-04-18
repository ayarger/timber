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
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using YarnSpinnerGodot;

public class NLuaScriptManager : Node
{

    public bool verbose = false;

	[Export]
    public NodePath dialogueRunnerPath;

	public static int id = 0;
	public static Script luaState;
	public static string globalClass = "global";
	public static NLuaScriptManager Instance;


	public static string testClassName = "testluaobject";
    public static string testClassNameDialogue = "testluadialogue";
    public static string emptyLuaFile = "lua_empty";

    public static Dictionary<string, LuaMethodInfo> luaMethods;


    public static List<LuaSingleton> luaWorkers;

    static Stopwatch timeline;


	//Create objects of a class. These objects will have their events ran.
	public bool CreateObject(string className, string objectName)
	{
		if (LuaRegistry.ContainsActor(objectName))
		{
			GD.PushWarning($"Object {objectName} of class {className} already exists!");
			return false;
		}

		if (!LuaRegistry.ContainsClass(className))
		{
			return false;
		}

		try
		{
			luaState.DoString($"{objectName} = {{}}\n" +
				$"setmetatable({objectName}, {{__index = function(self, key) \r\n  if global.keywords[key]  then\r\n\treturn GetValue(rawget(self,\"object_name\"),key)\r\n  else\r\n\treturn rawget({className}, key)\r\n  end\r\nend}})\n" +
				$"global:register_object({objectName}, \"{objectName}\")");
		}
		catch (InterpreterException e)
		{
			GD.PushError($"Exception caught in {className}\n{e.Message}");
			//Case of exception
			EventBus.Publish(new LuaExceptionEvent());
			return false;
		}
		//GD.Print($"Created {className} Object: {objectName}");
		return true;
    }
    public void KillActor(Spatial actor)
    {
		foreach(var e in LuaRegistry.luaActors)
		{
			if(e.Value == actor)
			{
				RemoveActor(e.Key,e.Value);
                return;
			}
		}

		if (verbose)
		{
			GD.PushError("Could not find actor to delete! This may be a projectile.");
		}
    }

	private void RemoveActor(string name, Spatial actor)
    {
        LuaRegistry.DeregisterActor(name);

    }

    //Create objects of a class, that hosts an actor. Should be the only "front facing" function for users.
    //Will probably replace Actor with ActorConfig.
	//Should support multiple scripts per actor, but for now that is not supported.
    public void CreateActor(string className, string objectName, Spatial actor)
	{
		if (!CreateObject(className, objectName))
		{
			return;
		}
		LuaRegistry.RegisterActor(new LuaObjectData( objectName, className, actor));
	}

	public void RunUntilCompletion(string function, List<string> prms = null)
	{
		string res = "Start";
		luaState.DoString($"timber_runner = coroutine.create({function})\n");
		List<string> args = prms ?? new List<string>();
		string data = "";

		List<KeyValuePair<string, Spatial>> toDelete = new List<KeyValuePair<string, Spatial>>();
		foreach(var p in LuaRegistry.luaActors)
		{
			if (!IsInstanceValid(p.Value))
			{
				toDelete.Add(p);
			}
		}
		foreach(var p in toDelete)
		{
            RemoveActor(p.Key, p.Value);
        }

		//Run until we do not have any commands from Lua
		while (res != "")
		{

			string prm = "";
			foreach(string arg in args)
			{
				prm += ","+arg;
			}

			//Print(data);
			if (data != "" && verbose) Print(data);
			//Run our coroutine with any required parameters (currently, just delta if we are running a tick update).
			res = luaState.DoString($"local co, res = coroutine.resume(timber_runner,global {prm}, {{{data}}})\n" +
				"return res").String;

			if (res == "{}" || res == ""||res == null) break;

			if (verbose) GD.Print("Got response: "+res);
			data = "";
			Dictionary<string, object>[] cmds;

            try
			{
				cmds = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(res);
			}
			catch (Exception e)
			{
				GD.Print("Exception while parsing Lua response: " + res);
				throw e;
			}

			//Run every command that was returned from Lua
			foreach (Dictionary<string, object> cmd in cmds)
			{
				object result = HandleCommand(cmd);
				if (result != null)
				{
					string id = Convert.ToString(cmd["id"]);
					//TODO: Support returning actors
					if(result.GetType() == typeof(string))
					{
						result = $"\"{result}\"";
                    }
                    else if (result.GetType() == typeof(bool))
                    {
                        result = (bool) result ? "true" : "false";
                    }
                    else if (typeof(Spatial).IsAssignableFrom(result.GetType()))
                    {
						//Return actor's name, which in lua context is the actual object.
                        result = LuaRegistry.luaActors.Where((pair) => { return pair.Value == result; }).FirstOrDefault().Key;
                    }
                    //data is a manually created Lua table
                    data += id + "=" + result; //Replace with serializing JSON?
					data += ",";
				}

			}
		}
	}


	public static void ShowException(string msg, string className = null)
	{
		if(className == null)
        {
            GD.Print($"Exception encountered: " + msg);
        }
		else
		{
            GD.Print($"Exception while running lua script \"{className}\": " + msg);
        }
	}
	//Handles commands. The contents of cmd is determined by the appropriate command in Lua, and needs to be manually converted.
	//Every command should have a name (the object name), and a command type. Everything else is determined by the appropriate API.
	public static object HandleCommand(Dictionary<string, object> cmd)
	{
		

		string name = Convert.ToString(cmd["obj"]);
		string command = Convert.ToString(cmd["type"]);
		Actor actor = LuaRegistry.GetActor(name) as Actor;

        if (cmd.ContainsKey("ERROR"))
        {
			string className = LuaRegistry.FindActorData(name).ClassName;
			string message = cmd["msg"] as string;
			if (message.StartsWith("attempt to index"))
			{
				message += ". Make sure you have \"self\" as the first parameter when calling functions.";
			}
			ShowException(message, className);
			return null;
        }

        LuaAPI.currentActor = actor;

		try
		{
			return LuaCommand.RunMethod(luaMethods[command].MethodInfo, cmd);
		}
		catch (LuaException e)
        {
            string className = LuaRegistry.FindActorData(name).ClassName;
            ShowException(e.Message, className);
        }

		////Parse commands. Will need to refactor to a better system.
		//if (command == "M")
		//{
		//	int amtX = Convert.ToInt32(cmd["x"]);
		//	int amtZ = Convert.ToInt32(cmd["z"]);
		//	TestMovement.SetDestination(actor, new Vector3(Grid.tileWidth * amtX, luaActors[name].GlobalTranslation.y, Grid.tileWidth * amtZ));
		//}
		//else if (command == "P")
		//{
		//	//Replace with toast later
		//	GD.Print(Convert.ToString(cmd["param"]));
		//}
		//else if (command == "R")
		//{
		//	//Same deal with newlines
		//	//Get data, just x position rn for demonstration
		//	string key = Convert.ToString(cmd["param"]);
		//	if (key == "x")
		//	{
		//		return luaActors[name].GlobalTranslation.x / Grid.tileWidth;
		//	}
		//	else if (key=="z")
		//	{
		//		return luaActors[name].GlobalTranslation.z / Grid.tileWidth;
		//	}
		//}
		//else if(command == "T")
		//{
		//	GD.Print(actor.ToString() + " just posted " + cmd["toastString"] + " to the toast!");

		//}
		//else if (command == "H")
		//{
		//	int damage = Convert.ToInt32(cmd["damage"]);
		//	actor.Hurt(damage, false, null);
		//}
		//else if (command == "K")
		//{
		//	string killSourceName = Convert.ToString(cmd["obj"]);
		//	Actor source = luaActors.ContainsKey(killSourceName) ? luaActors[killSourceName] as Actor : null;
		//	actor.Kill(source);
		//}

		return null;
	}


	public static string GenerateObjectName()
	{
		id++;
		return $"obj_{id}";
	}

	public static void Print(object a)
	{
		if(a == null)
		{
			GD.Print("null");
			return;
		}
		if (typeof(MoonSharp.Interpreter.Table).IsAssignableFrom(a.GetType()))
        {
            GD.Print("{");
            foreach (var x in (a as MoonSharp.Interpreter.Table).Pairs)
			{
				GD.Print(x.Key + "=" + x.Value);
			}
			GD.Print("}");
		}
		else
		{

            GD.Print(a);
        }
	}

	[LuaCommand("TestName")]
	public static void Test()
	{

    }
    [LuaCommand("TestName2")]
    public static void Test2()
    {

    }


    public override void _Ready()
    {
		GD.Print("Start Lua Initialization");

		timeline = new Stopwatch();
		timeline.Start();
		GD.Print(timeline.Elapsed);

		YarnManager.DialogueRunner = GetNode<DialogueRunner>(dialogueRunnerPath);

        //C# attributes and code-gen
        GD.Print("Find all functions with value start: " + timeline.Elapsed);
        luaMethods = LuaCommand.FindAllFunctionsWithAttribute();
        GD.Print("Find all functions with value done: " + timeline.Elapsed);

        string codegen = "";

		foreach (var luaMethod in luaMethods) {
			string parametersString1 = "";
            string parametersString2 = "";
            foreach (var par in luaMethod.Value.MethodInfo.GetParameters())
            {
                parametersString1 += ", _" + par.Name;
                if (par.HasDefaultValue)
				{
					if (typeof(bool).IsAssignableFrom(par.ParameterType))
					{
                        string temp = ",\r\n\t\t" + par.Name + " = (_" + par.Name + " or " + par.DefaultValue.ToString().ToLower() + ")";
                        GD.Print(temp);
                        parametersString2 += temp;
                    }
					else
					{
                        string temp = ",\r\n\t\t" + par.Name + " = (_" + par.Name + " or " + par.DefaultValue.ToString() + ")";
                        GD.Print(temp);
                        parametersString2 += temp;
                    }
					
					continue;
                }
				//If parameter is an actor, make sure to get the actor's name
				if(typeof(Spatial).IsAssignableFrom(par.ParameterType))
                {
                    parametersString2 += ",\r\n\t\t" + par.Name + " = _" + par.Name + ".object_name";
                }
				else
                {
                    parametersString2 += ",\r\n\t\t" + par.Name + " = _" + par.Name;
                }
            }

			//TODO:Fix later
			string objectString = (luaMethod.Key == "GetValue" ? "obj" : "obj.object_name");

			if(!luaMethod.Value.UsesNode)
			{
				objectString = "\"global\"";
				parametersString1 = parametersString1.Length > 0 ? parametersString1.Substring(1) : "";
			}
			else
			{
				parametersString1 = "obj" + parametersString1;
			}


            bool hasReturnValue = luaMethod.Value.MethodInfo.ReturnType != typeof(void);
            string func = "function " + luaMethod.Value.Name + "(" + parametersString1 + ")\r\n\tlocal coroutine_data = {coroutine.yield(\r\n\t\t{obj = " + objectString + ",\r\n\t\ttype=\"" + luaMethod.Key + "\"" + parametersString2 + "}\r\n\t\t)}"
				+ (hasReturnValue ? "\r\n\treturn coroutine_data[#coroutine_data]" : "")
                + "\r\nend\r\n";
			codegen += func;
        }

		if (verbose)
        {
            GD.Print("Code Gen:");
            GD.Print(codegen);
            GD.Print("End Code Gen");
			GD.Print("files system start " + timeline.Elapsed);
        }

        GD.Print("Lua:" + ResourceLoader.Exists("res://LuaEngine/testmodules/protoc.lua"));
        Instance = this;
        luaState = new Script();
        //luaState.Globals["Mul"] = (System.Action<object>) Print;

        //This may cause issues
        string abspath = $"{System.IO.Directory.GetCurrentDirectory()}/LuaEngine/testmodules/".Replace("\\", "/");

		luaState.Options.ScriptLoader = new FileSystemScriptLoader();
		((ScriptLoaderBase)luaState.Options.ScriptLoader).ModulePaths = new string[] { abspath+"?", $"{abspath}?.lua", $"{abspath}/lunajson/?", $"{abspath}/lunajson/?.lua" };

		//TODO: Add a proper package manager, paths will likely not work on HTML ever

		//luaState.DoString("package.path = 'testmodules/?.lua;' .. package.path");
		//luaState.DoString($"package.path = '{abspath}?.lua;' .. package.path");
		//luaState.DoString("package.path = 'LuaEngine/testmodules/?.lua;' .. package.path");
		//luaState.AddModuleDir("res://LuaEngine/testmodules");
		LuaRegistry.Reset();
		string testc = "local pb = require \"pb\"\r\nlocal protoc = require \"protoc\"\r\n\r\n-- load schema from text (just for demo, use protoc.new() in real world)\r\nassert(protoc:load [[\r\n   message Phone {\r\n\t  optional string name        = 1;\r\n\t  optional int64  phonenumber = 2;\r\n   }\r\n   message Person {\r\n\t  optional string name     = 1;\r\n\t  optional int32  age      = 2;\r\n\t  optional string address  = 3;\r\n\t  repeated Phone  contacts = 4;\r\n   } ]])\r\n\r\n-- lua table data\r\nlocal data = {\r\n   name = \"ilse\",\r\n   age  = 18,\r\n   contacts = {\r\n\t  { name = \"alice\", phonenumber = 12312341234 },\r\n\t  { name = \"bob\",   phonenumber = 45645674567 }\r\n   }\r\n}\r\n\r\n-- encode lua table data into binary format in lua string and return\r\nlocal bytes = assert(pb.encode(\"Person\", data))\r\nreturn pb.tohex(bytes)";

		//GD.Print(luaState.DoString(testc));
		luaState.Globals["RawPrint"] = (Action<object>)Print;

		//Initialize global Lua manager. Make global a "global object"?
		//https://manual.gamemaker.io/monthly/en/GameMaker_Language/GML_Overview/Variables/Global_Variables.htm

		Godot.File global = new Godot.File();
		global.Open($"LuaEngine/{globalClass}.lua", Godot.File.ModeFlags.Read);
		luaState.DoString(codegen + "\n" + global.GetAsText());

		////FOR TESTING
		//Godot.File x = new Godot.File();
		//x.Open($"LuaEngine/{testClassName}.lua", Godot.File.ModeFlags.Read);
		//LuaRegistry.RegisterClass(x.GetAsText(), testClassName);


  //      Godot.File y = new Godot.File();
  //      y.Open($"LuaEngine/{testClassNameDialogue}.lua", Godot.File.ModeFlags.Read);
  //      LuaRegistry.RegisterClass(y.GetAsText(), testClassNameDialogue);

        Godot.File z = new Godot.File();
        z.Open($"LuaEngine/{emptyLuaFile}.lua", Godot.File.ModeFlags.Read);
        LuaRegistry.RegisterClass(z.GetAsText(), emptyLuaFile);

        GD.Print("Lua initialized");



		//ArborCoroutine.StartCoroutine(SimulateCoro(0));
		//x.Close();


		//Instantiate workers
		luaWorkers = new List<LuaSingleton>();

		luaWorkers.Add(LuaInputManager.Instance);

		foreach(var i in luaWorkers)
		{
			i.Start();
		}

        GD.Print("Lua workers initialized");

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

		//Ready for any object that hasn't started yet
        RunUntilCompletion("global.receive_specified", new List<string> { LuaUtils.ConstructArray(LuaRegistry.needToRunReady) , "\"ready\""});
		LuaRegistry.ClearReady();

        //Process
        RunUntilCompletion("global.receive", new List<string> { "\"update\"" });

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
        //Run workers
        foreach (var i in luaWorkers)
        {
            i.Update();
        }
    }
}

public class LuaExceptionEvent
{
	public LuaExceptionEvent()
	{

	}
}
[System.AttributeUsage(System.AttributeTargets.Method)]
public class LuaCommand : System.Attribute
{
	public string Name { get; set; }
    public bool UsesNode { get; set; }
    public LuaCommand(string name, bool usesNode = true)
    {
        Name = name;
        UsesNode = usesNode;
    }
    public static Dictionary<string, LuaMethodInfo> FindAllFunctionsWithAttribute()
    {
        Stopwatch timeline = new Stopwatch();
		timeline.Start();

        //var assemblies = AppDomain.CurrentDomain.GetAssemblies()
		//	.Where(a => !a.IsDynamic && a.FullName.Contains("TargetAssemblyName")); // Filter relevant assemblies

		// Shoud cache results? Parrellize?


		//var typesWithMyAttribute = (
		//	from a in AppDomain.CurrentDomain.GetAssemblies()
		//	from t in a.GetTypes()
		//	from f in t.GetMethods()
		//	let attributes = f.GetCustomAttributes(typeof(LuaCommand), true)
		//	where attributes != null && attributes.Length > 0
		//	select new { Method = f, Attribute = attributes.Cast<LuaCommand>().First() }
		//).ToList();

		// Parralelized with PlinQ
        var typesWithMyAttribute = AppDomain.CurrentDomain.GetAssemblies()
			.AsParallel() // Parallelize the assembly processing
			.SelectMany(a => a.GetTypes())
			.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
				.Select(f => new { Method = f, Attributes = f.GetCustomAttributes(typeof(LuaCommand), true) }))
			.Where(x => x.Attributes != null && x.Attributes.Length > 0)
			.Select(x => new { Method = x.Method, Attribute = x.Attributes.Cast<LuaCommand>().First() })
			.ToList();


        GD.Print("typesWithAttributes all found in " + timeline.Elapsed);

        Dictionary<string, LuaMethodInfo> ans = new Dictionary<string, LuaMethodInfo>();

		foreach (var attribute in typesWithMyAttribute)
		{
			//GD.Print(attribute.Attribute.Name);

			//Deprecated overloading logic, left here just in case
			string internalName = attribute.Attribute.Name;
			while (ans.ContainsKey(internalName))
            {
				internalName = internalName + "1";

            }
			ans[internalName] = new LuaMethodInfo(attribute.Attribute.Name, attribute.Method, attribute.Attribute.UsesNode);
            //GD.Print("attribute: " + attribute.ToString() + " found at " + timeline.Elapsed);
        }

        //GD.Print("Number of funcs with attribute:" + typesWithMyAttribute.Count());

        GD.Print("FindAllFunctionsFinished in " + timeline.Elapsed);

        return ans;
    }

    public static object RunMethod(MethodInfo methodInfo, Dictionary<string, object> cmd)
	{
		List<object> args = new List<object>();
		string currentParameter = "";
		try
		{
            foreach (var pi in methodInfo.GetParameters())
            {
                object o;
				currentParameter = pi.Name;
                if (pi.ParameterType == typeof(string))
                {
                    o = Convert.ToString(cmd[pi.Name]);
                }
                else if (pi.ParameterType == typeof(int))
                {
                    o = Convert.ToInt32(cmd[pi.Name]);
                }
                else if (pi.ParameterType == typeof(float))
                {
                    o = (float)Convert.ToDouble(cmd[pi.Name]);
                }
                else if (pi.ParameterType == typeof(double))
                {
                    o = Convert.ToDouble(cmd[pi.Name]);
                }
                else if (typeof(Spatial).IsAssignableFrom(pi.ParameterType))
                {
                    //THIS SHOULD ONLY BE GODOT OBJECTS LISTED IN luaActors
                    o = LuaRegistry.GetActor(Convert.ToString(cmd[pi.Name]));
                }
                else if (pi.ParameterType == typeof(bool))
                {
                    o = (bool)cmd[pi.Name] ? true : false;
                }
                else
                {
                    throw new Exception("unsupported datatype in lua api!");
                }
                args.Add(o);
            }

        }
		catch (System.InvalidCastException)
		{
			throw new LuaException($"Invalid Cast Exception when calling function {methodInfo.Name}, are you sure you used the right parameters?");
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            throw new LuaException($"Could not find parameter {currentParameter} while running {methodInfo.Name}, are you sure you used the right parameters?");
        }

        return methodInfo.Invoke(null, args.ToArray());
	}

}

public class LuaMethodInfo
{
	public string Name { get; set; }
	public MethodInfo MethodInfo { get; private set; }

	public bool UsesNode { get; private set; }

	public LuaMethodInfo(string name, MethodInfo methodInfo, bool usesNode)
	{
		Name = name;
		MethodInfo = methodInfo;
		UsesNode = usesNode;
	}

}