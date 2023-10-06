using Godot;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;

public class MovementState : ActorState
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public override string name
    {
        get { return "MovementState"; }
    }
    public List<Vector3> waypoints = new List<Vector3>();
    float mvmSpeed = .1f; //Pull from HasStats


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    public override void Start()
    {
        inclusiveStates = new HashSet<string>();
        waypoints = new List<Vector3>();

    }

    public override void Update(float delta)
    {

        //Manage waypoints
        if (waypoints.Count > 0)
        {
            Vector3 dest = waypoints[0];
            Vector3 disp = dest - actor.GlobalTranslation;
            if (disp.Length() < mvmSpeed)
            {
                actor.GlobalTranslation = dest;
                waypoints.RemoveAt(0);
                if (waypoints.Count == 0)
                {
                    ArborCoroutine.StartCoroutine(MoveToNearestTile(), this);
                }
            }
            else
            {
                actor.GlobalTranslation += mvmSpeed * disp.Normalized();
            }
        }
        else
        {
        }
    }

    public override void Stop()
    {
        ArborCoroutine.StartCoroutine(MoveToNearestTile(), this);
        //Place actor in tilemap
    }

    IEnumerator MoveToNearestTile()
    {
        int x = Mathf.RoundToInt(actor.GlobalTranslation.x / 2.0f);
        int z = Mathf.RoundToInt(actor.GlobalTranslation.z / 2.0f);
        if (x < 0 || z < 0||z >= Grid.height || x >= Grid.width)
        {
            //actor.movetotile(OOB);
        }

        //Flood fill
        Coord cur = new Coord(x, z);
        GD.Print(cur.x, cur.z);
        if (Grid.Get(cur).actor==null || Grid.Get(cur).actor == actor)
        {
            actor.currentTile.actor = null;
            Grid.Get(cur).actor = actor;
            actor.currentTile = Grid.Get(cur);
        }
        else
        {
            bool foundNewTile = false;
            Coord dest = new Coord(0,0);
            Queue<Coord> queue = new Queue<Coord>();
            HashSet<Coord> visited = new HashSet<Coord>();
            visited.Add(cur);

            List<Coord> directions = new List<Coord>
            {
                new Coord(-1, 0),
                new Coord(1, 0),
                new Coord(0, -1),
                new Coord(0, 1)
            };
            queue.Enqueue(cur);
            while(queue.Count > 0)
            {
                cur = queue.Dequeue();
                if (Grid.Get(cur).actor == null || Grid.Get(cur).actor == actor)
                {
                    actor.currentTile.actor = null;
                    Grid.Get(cur).actor = actor;
                    actor.currentTile = Grid.Get(cur);
                    dest = cur;
                    foundNewTile = true;
                    break;
                }

                foreach(var d in directions)
                {
                    Coord neighbor = cur + d;
                    if (!visited.Contains(neighbor) && Grid.Get(neighbor).value != 'e')
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }

            }

            //This is kind of jank, change this later
            waypoints = TestMovement.PathFind(actor.GlobalTranslation, new Vector3(dest.x * 2f, .1f, dest.z * 2f));
            for(int i = 0; i<100 && waypoints.Count> 0; i++)
            {
                Update(0.016f);
                yield return null;
            }
            actor.GlobalTranslation = new Vector3(dest.x * 2f, .1f, dest.z * 2f);

            if (!foundNewTile)
            {
                GD.Print("Warning: Could not find a tile to stand on!");
            }
        }

        //actor.movetotile;
        yield return null;
        manager.DisableState(name);
    }

}