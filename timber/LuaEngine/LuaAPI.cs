using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using YarnSpinnerGodot;

public class LuaAPI
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public static Actor currentActor;
    public static Actor dialogueActor;


    [LuaCommand("SetDestination")]
    public static void Move(int x, int z, bool ignoreCombat = false)
    {
        if (ignoreCombat || !(currentActor.IsStateActive("CombatState") || currentActor.IsStateActive("ChaseState")))
        {
            TestMovement.SetDestination(currentActor, new Vector3(Grid.tileWidth * x, currentActor.GlobalTranslation.y, Grid.tileWidth * z));
        }
        
    }

    [LuaCommand("Print", false)]
    public static void Print(string param)
    {
        GD.Print(param);
    }


    [LuaCommand("PrintName")]
    public static void PrintName()
    {
        GD.Print("My name is: " + currentActor.Name);
    }

    [LuaCommand("GetThree", false)]
    public static int GetThree()
    {
        return 3;
    }

    [LuaCommand("GetFarthestActor")]
    public static Spatial GetFarthestActor()
    {
        float max = -1;
        Spatial closest = null;
        foreach(Spatial i in LuaRegistry.luaActors.Values)
        {
            float dist = i.GlobalTranslation.DistanceSquaredTo(currentActor.GlobalTranslation);
            if(dist > max)
            {
                max = dist;
                closest = i;
            }
        }
        return closest;
    }

    [LuaCommand("GetKey", false)]
    public static bool GetKey(string key)
    {
        if (!LuaInputManager.previousKeys.ContainsKey(key))
        {
            throw new LuaException("GetKey key is invalid!");
        }

        return Input.IsKeyPressed(OS.FindScancodeFromString(key)) ? true : false;
    }


    [LuaCommand("GetKeyDown", false)]
    public static bool GetKeyDown(string key)
    {
        if (!LuaInputManager.previousKeys.ContainsKey(key))
        {
            throw new LuaException("GetKeyDown key is invalid!");
        }

        return Input.IsKeyPressed(OS.FindScancodeFromString(key)) && !LuaInputManager.previousKeys[key] ? true : false;
    }


    [LuaCommand("GetKeyUp", false)]
    public static bool GetKeyUp(string key)
    {
        if (!LuaInputManager.previousKeys.ContainsKey(key))
        {
            throw new LuaException("GetKeyUp key is invalid!");
        }

        return !Input.IsKeyPressed(OS.FindScancodeFromString(key)) && LuaInputManager.previousKeys[key] ? true : false;
    }

    [LuaCommand("InCombat")]
    public static bool InCombat()
    {
        return currentActor.IsStateActive("CombatState") || currentActor.IsStateActive("ChaseState");
    }

    [LuaCommand("TakeDamage")]
    public static void TakeDamage(int damage)
    {
        currentActor.Hurt(damage, false);
    }



    [LuaCommand("PrintNameOfActor", false)]
    public static void PrintNameOfActor(Spatial actor)
    {
        GD.Print("I got an actor!: " + actor.GlobalTranslation);
    }

    [LuaCommand("GetValue")]
    public static object Get(string param)
    {
        if (param == "x")
        {
            return currentActor.GlobalTranslation.x / Grid.tileWidth;
        }
        else if (param == "z")
        {
            return currentActor.GlobalTranslation.z / Grid.tileWidth;
        }
        else if (param == "team")
        {
            return currentActor.team;
        }
        return null;
    }

    [LuaCommand("GetNearestActorOfTeam")]
    public static Spatial GetNearestActorOfTeam(string team)
    {
        Spatial ans = null;
        float dist = -1f;
        foreach(Spatial actor in LuaRegistry.luaActors.Values)
        {
            if (typeof(Spatial).IsAssignableFrom(actor.GetType()))
            {
                Actor cur = actor as Actor;
                if (cur.team != team) continue;
                float curDist = cur.GlobalTranslation.DistanceTo(currentActor.GlobalTranslation);
                if (dist == -1 || curDist < dist)
                {
                    dist = curDist;
                    ans = cur;
                }
            }
        }

        return ans;
    }



    [LuaCommand("GetDistance", false)]
    public static float GetDistance(Spatial actor1, Spatial actor2)
    {
        return actor1.GlobalTranslation.DistanceTo(actor2.GlobalTranslation);
    }

    [LuaCommand("PrintToast")]
    public static void PrintToast(string toastString)
    {
        GD.Print(currentActor.ToString() + " just posted " + toastString + " to the toast!");
    }

    [LuaCommand("Hurt")]
    public static void Hurt(int damage)
    {
        currentActor.Hurt(damage, false, null);
    }

    [LuaCommand("Kill")]
    public static void Kill()
    {
        currentActor.Kill(currentActor);
    }

    [LuaCommand("Spawn")]
    public static Spatial Spawn(Spatial actorType)
    {
        if (!typeof(Actor).IsAssignableFrom(actorType.GetType()))
        {
            throw new LuaException("Given parameter is not an actor!");
        }
        Actor new_actor = LuaLoader.instance.SpawnActorOfType(((Actor)actorType).config, currentActor.GlobalTranslation);
        //Set up script
        NLuaScriptManager.Instance
            .CreateActor(NLuaScriptManager.emptyLuaFile, NLuaScriptManager.GenerateObjectName(), new_actor);
        TestMovement.SetDestination(new_actor, currentActor.GlobalTranslation);

        return new_actor;

    }

    [LuaCommand("SetScale")]
    public static void SetScale(float scale)
    {
        currentActor.Scale = new Vector3(scale, scale, scale);
    }

    [LuaCommand("SetTeam")]
    public static void SetTeam(string team)
    {
        currentActor.team = team;
    }

    [LuaCommand("StartDialogue")]
    public static void StartDialogue(string dialogueName)
    {
        try
        {
            DialogueRunner dr = YarnManager.DialogueRunner;
            if (dr.IsDialogueRunning) throw new LuaException("Dialogue is currently running!");

            dr.SetProject(YarnManager.projects[dialogueName]);
            dr.StartDialogue(dr.startNode);

            dialogueActor = currentActor;
            GameplayCamera.instance.ForceNewState(new FixedCameraState(dialogueActor));
            ArborCoroutine.StartCoroutine(AwaitDialogueFinished());
        }
        catch (Exception e)
        {
            GD.Print("Could not start Dialogue! " + e.Message);
        }
    }

    static System.Collections.IEnumerator AwaitDialogueFinished()
    {
        while (YarnManager.DialogueRunner.IsDialogueRunning)
        {
            yield return null;
        }
        GameplayCamera.instance.ForceNewState(new PlayerControlledCameraState());
    }

    [YarnCommand("SendSignal")]
    public static void SendSignal(string signal)
    {
        NLuaScriptManager.Instance.RunUntilCompletion("global.receive", new List<string> { $"\"{signal}\"" });
    }

    [YarnCommand("RunFunction")]
    public static void RunFunction(string signal)
    {
        string name = "";
        foreach(var i in LuaRegistry.luaActors)
        {
            if(i.Value == dialogueActor)
            {
                name = i.Key;
                break;
            }
        }
        if(name == "")
        {
            throw new LuaException("Could not find actor that started the dialogue. Was the actor destroyed?");
        }

        NLuaScriptManager.Instance.RunUntilCompletion("global.single_receive", new List<string> { name, $"\"{signal}\"" });
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}


public class LuaException : Exception
{
    public LuaException(string message) : base(message)
    {

    }
}