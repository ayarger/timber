using Godot;
using System;
using System.Xml.Linq;

public class LuaAPI
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public static Actor currentActor;


    [LuaCommand("SetDestination")]
    public static void Move(int x, int z) {

        TestMovement.SetDestination(currentActor, new Vector3(Grid.tileWidth * x, currentActor.GlobalTranslation.y, Grid.tileWidth * z));
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
        foreach(Spatial i in NLuaScriptManager.luaActors.Values)
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
        return null;
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

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
