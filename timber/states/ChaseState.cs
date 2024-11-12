using Godot;
using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChaseState : ActorState
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public override string name
    {
        get { return "ChaseState"; }
    }
    public List<Vector3> waypoints = new List<Vector3>();
    float mvmSpeed = 4f; //Pull from HasStats
    int attackRange = 2;
    public Actor TargetActor;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    public override void Start()
    {
        timer = 0.0f;
        inclusiveStates = new HashSet<string>();
        waypoints = new List<Vector3>();
        ArborCoroutine.StopCoroutinesOnNode(this);
    }

    public override void Config(StateConfig stateConfig)
	{
		attackRange = (int)stateConfig.stateStats["attackRange"];
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
                Coord cur = Grid.ConvertToCoord(waypoints[waypoints.Count - 1]);
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
            if (disp.Length() < mvmSpeed * delta)
            {
                actor.GlobalTranslation = dest;
                waypoints.RemoveAt(0);
            }
            else
            {
                actor.GlobalTranslation += mvmSpeed * disp.Normalized()*delta;
            }
            
        }
        else
        {
            Coord dest = Grid.ConvertToCoord(TargetActor.GlobalTranslation);
            if(TestMovement.WithinRange(dest, actor, attackRange))
            {
                CombatState cs = manager.states["CombatState"] as CombatState;
                cs.TargetActor = TargetActor;
                manager.EnableState("CombatState");
                manager.DisableState(name);
            }else{
                Coord coordDest = TestMovement.FindClosestTileInRange(Grid.ConvertToCoord(TargetActor.GlobalTranslation), actor, attackRange);
                Vector3 vectorDest = new Vector3(coordDest.x * Grid.tileWidth, .1f, coordDest.z * Grid.tileWidth);
                List<Vector3> a = TestMovement.PathFind(actor.GlobalTranslation, vectorDest);
                if (a.Count > 0)
                {
                    waypoints = a;
                }else{
                    manager.DisableState(name);
                }
                return;
            }
        }
    }

    public override void Stop()
    {
        ArborCoroutine.StopCoroutinesOnNode(this);
        if(!actor.IsQueuedForDeletion())
            ArborCoroutine.StartCoroutine(MoveToNearestTile(), this);
        //Place actor in tilemap
    }

    float timer = 0.0f;
    public override void Animate(float delta)
    {
        timer += delta;

        /* Rotation */
        const float rot_frequency = 10f;
        const float rot_amplitude = 0.1f;
        actor.view.Rotation = actor.initial_rotation + new Vector3(0, 0, rot_amplitude * Mathf.Sin(timer * rot_frequency));

        /* Position */
        const float pos_amplitude = 0.5f;
        const float posfrequency = 10f;
        float signal = Mathf.Abs(Mathf.Sin(timer * posfrequency)) * pos_amplitude;
        Vector3 desired_position = Vector3.Up * signal;
        actor.view.Translation += (desired_position -= actor.view.Translation) * 0.4f;

        /* Paper Turning */
        float current_scale_x = actor.view.Scale.x;
        current_scale_x += (actor.GetDesiredScaleX() - current_scale_x) * 0.2f;
        actor.view.Scale = new Vector3(current_scale_x, actor.view.Scale.y, actor.view.Scale.z);
    }

    IEnumerator MoveToNearestTile()
    {
        Coord c = Grid.ConvertToCoord(actor.GlobalTranslation);
        int x = c.x;
        int z = c.z;
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
            c = Grid.ConvertToCoord(actor.GlobalTranslation);
            TileData dest = FindNearestUnclaimedTile(c.x,c.z);
            actor.currentTile.actor = null;
            dest.actor = actor;
            actor.currentTile = dest;

            manager.DisableState(name);
            //This is kind of jank, change this later
            waypoints = TestMovement.PathFind(actor.GlobalTranslation, new Vector3(dest.x * Grid.tileWidth, .1f, dest.z * Grid.tileWidth));
            for(int i = 0; i<100 && waypoints.Count> 0; i++)
            {
                Update(.016f);
                yield return null;
            }
            actor.GlobalTranslation = new Vector3(dest.x * Grid.tileWidth, .1f, dest.z * Grid.tileWidth);

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
                foundNewTile = true;
                dest = cur;
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