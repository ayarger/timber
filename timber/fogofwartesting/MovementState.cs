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
            Vector3 totalDist = waypoints[waypoints.Count - 1] - actor.GlobalTranslation;
            if(totalDist.Length() < 5)
            {
                //Attempt to claim the tile. If the tile is already claimed, flood fill to find the next tile. 
                Coord cur = new Coord(Mathf.RoundToInt(waypoints[waypoints.Count-1].x / 2.0f), Mathf.RoundToInt(waypoints[waypoints.Count - 1].z / 2.0f));
                if (Grid.Get(cur).actor == null || Grid.Get(cur).actor == actor)
                {
                    actor.currentTile.actor = null;
                    Grid.Get(cur).actor = actor;
                    actor.currentTile = Grid.Get(cur);
                }
                else
                {
                    TileData newDest = FindNearestUnclaimedTile(cur.x, cur.z);
                    actor.currentTile.actor = null;
                    newDest.actor = actor;
                    actor.currentTile = newDest;
                    waypoints = TestMovement.PathFind(actor.GlobalTranslation, newDest.GlobalTranslation);
                }

            }
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
            yield break;
        }

        //Flood fill
        Coord cur = new Coord(x, z);
        if (Grid.Get(cur).actor==null || Grid.Get(cur).actor == actor)
        {
            actor.currentTile.actor = null;
            Grid.Get(cur).actor = actor;
            actor.currentTile = Grid.Get(cur);
        }
        else
        {

            TileData dest = FindNearestUnclaimedTile(Mathf.RoundToInt(actor.GlobalTranslation.x / 2.0f), Mathf.RoundToInt(actor.GlobalTranslation.z / 2.0f));
            actor.currentTile.actor = null;
            dest.actor = actor;
            actor.currentTile = dest;

            manager.DisableState(name);
            //This is kind of jank, change this later
            waypoints = TestMovement.PathFind(actor.GlobalTranslation, new Vector3(dest.x * 2f, .1f, dest.z * 2f));
            for(int i = 0; i<100 && waypoints.Count> 0; i++)
            {
                Update(.016f);
                yield return null;
            }
            actor.GlobalTranslation = new Vector3(dest.x * 2f, .1f, dest.z * 2f);

        }

        //actor.movetotile;
        manager.DisableState(name);
        yield return null;
    }


    TileData FindNearestUnclaimedTile(int x, int z)
    {
        if (x < 0 || z < 0 || z >= Grid.height || x >= Grid.width)
        {
            //actor.movetotile(OOB);
            return Grid.Get(new Coord(0,0));
        }

        //Flood fill
        Coord cur = new Coord(x, z);

        bool foundNewTile = false;
        Coord dest = new Coord(0, 0);
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
        while (queue.Count > 0)
        {
            cur = queue.Dequeue();
            if (Grid.Get(cur).actor == null || Grid.Get(cur).actor == actor)
            {
                dest = cur;
                foundNewTile = true;
                break;
            }

            foreach (var d in directions)
            {
                Coord neighbor = cur + d;
                if (!visited.Contains(neighbor) && Grid.Get(neighbor).value != 'e')
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }

        }
        if (!foundNewTile)
        {
            GD.Print("Warning: Could not find a tile to stand on!");
        }
        return Grid.Get(dest);
    }

}