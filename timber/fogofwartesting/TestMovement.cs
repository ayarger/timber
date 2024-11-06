using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Priority_Queue;
using Newtonsoft.Json.Serialization;
using System.Net;
using static Amazon.S3.Util.S3EventNotification;

public class TestMovement : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("right_click"))
		{
			//From SelectionSystem.cs
			var from = GameplayCamera.GetGameplayCamera().ProjectRayOrigin(SelectionSystem.GetCursorWindowPosition());
			var dir = GameplayCamera.GetGameplayCamera().ProjectRayNormal(SelectionSystem.GetCursorWindowPosition());

			Vector3 intersection_point = GetRayPlaneIntersection(new Ray(from, dir), Vector3.Up);
			//DebugSphere.VisualizePoint(intersection_point);

			/* Round */
			Vector3 rounded_point = Grid.LockToGrid(intersection_point);

			//Move to position

			foreach (var entity in SelectionSystem.GetCurrentActiveSelectables())
			{
				//Maybe better way to do this w/o reflection?
				if (typeof(Actor).IsAssignableFrom(entity.GetParent().GetType()))
				{
					var actor = (Actor)entity.GetParent();
                    string team = actor.GetNode<HasTeam>("HasTeam").team;
                    if (team != "player") return;
                    SetDestination(actor, rounded_point);

				}
			}
		}
	}
	public static void SetDestination(Actor actor, Vector3 dest)
    {
        //DebugSphere.VisualizePoint(intersection_point);

        /* Round */
        Vector3 rounded_point = Grid.LockToGrid(dest);
        StateManager sm = actor.FindNode("StateManager") as StateManager;
		if (!sm.states.ContainsKey("MovementState")) return;

        MovementState b = (sm.states["MovementState"] as MovementState);

        //ArborCoroutine.StopCoroutinesOnNode(b);
        ArborCoroutine.StartCoroutine(PathFindAsync(actor.GlobalTranslation, rounded_point, (List<Vector3> a) => {
            if (a.Count > 0)
            {
                sm.EnableState("MovementState");
                b.waypoints = a;
            }
        }), b);
    }

	public static List<Vector3> PathFind(Vector3 cur, Vector3 dest)
	{
		List<Vector3> ans = new List<Vector3>();
		IEnumerator i = PathFindAsync(cur, dest, (List<Vector3> a) => { ans = a; });
		while (i.MoveNext()) { }
		return ans;

	}
	
	public static Coord FindClosestTileInRange(Coord cur, Actor actor, int attackRange)
	{
		if (cur.x < 0 || cur.z < 0 || cur.z >= Grid.height || cur.x >= Grid.width)
		{
			//actor.movetotile(OOB);
			return new Coord(0, 0);
		}

		//Flood fill
		Coord actorPos = Grid.ConvertToCoord(actor.GlobalTranslation);

		Coord dist = actorPos - cur;
		Coord movement = new Coord(0, 0);
		if (Math.Abs(dist.x) + Math.Abs(dist.z) <= attackRange)
		{
			return actorPos;
		}

		while (Math.Abs(dist.x) + Math.Abs(dist.z) > attackRange)
		{
			if (Math.Abs(dist.x) > Math.Abs(dist.z))
			{
				movement.x += -(dist.x / Math.Abs(dist.x));
				dist.x += -(dist.x / Math.Abs(dist.x));

			}
			else
			{
				movement.z += -(dist.z / Math.Abs(dist.z));
				dist.z += -(dist.z / Math.Abs(dist.z));

			}
		}
		return actorPos + movement;

	}

	public static bool WithinRange(Coord pos, Actor actor, int attackRange)
	{
		Coord actorCoord = Grid.ConvertToCoord(actor.GlobalTranslation);

		float dist = Math.Abs(pos.x - actorCoord.x)
			+ Math.Abs(pos.z - actorCoord.z);

		//GD.Print("actorCoord: " + actorCoord.x + " " + actorCoord.z + " dist: " + dist);

		return dist <= attackRange || (pos.x == actorCoord.x && pos.z == actorCoord.z);
	}

	static bool LineOfSight(Coord a, Coord b)
	{
		if (a.x == b.x)
		{
			for (int i = a.z; i != b.z; i += (a.z - b.z > 0) ? -1 : 1)
			{
				if (Grid.tiledata[i][a.x].value != '.') return false;
			}
			return true;
		}
		if (a.x > b.x)
		{
			var temp = a;
			a = b;
			b = temp;
		}
		float slope = (b.z - a.z) / (float)(b.x - a.x);
		float curZ = a.z;
		bool isUp = slope > 0;
		for (int i = a.x; i < b.x; i++)
		{
			float newCurZ = curZ + .5f * slope;
			for (int j = Mathf.RoundToInt(curZ); isUp ? j <= Mathf.RoundToInt(newCurZ) : j >= Mathf.RoundToInt(newCurZ); j += isUp ? 1 : -1)
			{
				if (Grid.tiledata[j][i].value != '.') return false;

			}
			curZ = newCurZ;
			newCurZ = curZ + .5f * slope;
			for (int j = Mathf.RoundToInt(curZ); isUp ? j <= Mathf.RoundToInt(newCurZ) : j >= Mathf.RoundToInt(newCurZ); j += isUp ? 1 : -1)
			{
				if (Grid.tiledata[j][i + 1].value != '.') return false;
			}
			curZ = newCurZ;
		}

		return true;

	}

	Vector3 GetRayPlaneIntersection(Ray ray, Vector3 plane_normal)
	{
		float t = -(plane_normal.Dot(ray.start)) / (plane_normal.Dot(ray.direction));
		return ray.start + t * ray.direction;
	}


    public static IEnumerator PathFindAsync(Vector3 cur, Vector3 dest, System.Action<List<Vector3>> action) //Fake async, run this in a coroutine.
    {
		yield return null;
        //Theta* https://en.wikipedia.org/wiki/Theta*
        //Priority Queue https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
        Coord curCoord = Grid.ConvertToCoord(cur);

        Coord destCoord = Grid.ConvertToCoord(dest);
		if (Grid.Get(destCoord).value == 'e')
		{
			action.Invoke(new List<Vector3>());
			yield break;
		}

        //LuaLoader.tileData;

        FastPriorityQueue<ThetaNode> open = new FastPriorityQueue<ThetaNode>(10000);
        ThetaNode origin = new ThetaNode(curCoord, null);
        origin.gScore = 0f;
        open.Enqueue(origin, 0f);
        HashSet<ThetaNode> closed = new HashSet<ThetaNode>();
        Dictionary<Coord, ThetaNode> createdNodes = new Dictionary<Coord, ThetaNode>();
        createdNodes[curCoord] = origin;


        List<Coord> directions = new List<Coord>
        {
            new Coord(-1, 0),
            new Coord(1, 0),
            new Coord(0, -1),
            new Coord(0, 1)
        };

		int count = 0;

        while (open.Count > 0)
        {
			count++;
			if (count >= 300)
			{
				yield return null;
				count = 0;
			}
            ThetaNode curNode = open.Dequeue();
            if (curNode.x == destCoord.x && curNode.z == destCoord.z)
            {
                List<Vector3> ans = new List<Vector3>();
				ans.Add(new Vector3(curNode.x * Grid.tileWidth, cur.y, curNode.z * Grid.tileWidth));
				curNode = curNode.parent;
				if (curNode == null)
				{
					action.Invoke(ans);
					yield break;
				}
                while (curNode.parent != null)
                {
					if (!LineOfSight(Grid.ConvertToCoord(ans[ans.Count-1]), curNode.parent.coord))
                    {
                        ans.Add(new Vector3(curNode.x * Grid.tileWidth, cur.y, curNode.z * Grid.tileWidth));
                    }
                    curNode = curNode.parent;
                }
                ans.Reverse();
                action.Invoke(ans);
				yield break;
            }
            closed.Add(curNode);

            foreach (var dir in directions)
            {
                Coord neighbor = curNode.coord + dir;
                if (neighbor.x < 0 || neighbor.z < 0 || neighbor.x > Grid.width - 1 || neighbor.z > Grid.height - 1) continue;
                if (Grid.tiledata[neighbor.z][neighbor.x].value != '.') continue;

                if (!createdNodes.ContainsKey(neighbor))
                {
                    createdNodes[neighbor] = new ThetaNode(neighbor, curNode);
                }

                //Update Node
                ThetaNode neighborNode = createdNodes[neighbor];

                if (closed.Contains(neighborNode)) continue;

                //if (curNode.parent != null
                //    && curNode.parent.gScore + (curNode.parent.coord - neighbor).Mag() < neighborNode.gScore
                //    && LineOfSight(curNode.parent.coord, neighbor))
                //{
                //    neighborNode.gScore = curNode.parent.gScore + (curNode.parent.coord - neighbor).Mag();
                //    neighborNode.parent = curNode.parent;
                //    if (open.Contains(neighborNode))
                //    {
                //        open.Remove(neighborNode);
                //    }
                //    open.Enqueue(neighborNode, neighborNode.gScore + (destCoord - neighbor).Mag());
                //}
                //else
				if (curNode.gScore + (curNode.coord - neighbor).Mag() < neighborNode.gScore)
                {
                    neighborNode.gScore = curNode.gScore + (curNode.coord - neighbor).Mag();
                    neighborNode.parent = curNode;
                    if (open.Contains(neighborNode))
                    {
                        open.Remove(neighborNode);
                    }
                    open.Enqueue(neighborNode, neighborNode.gScore + (destCoord - neighbor).Mag());
                }
			}
		}

        action.Invoke(new List<Vector3>());
    }


    //IEnumerator MoveCharacter(Actor actor, List<Vector3> points, float mvmSpeed) //mvmSpeed should be a stat
    //{
    //    for (int i = 0; i < points.Count; i++)
    //    {
    //        Vector3 dest = points[i];
    //        while (actor.GlobalTranslation != dest)
    //        {
    //            Vector3 disp = dest - actor.GlobalTranslation;
    //            if (disp.Length() < mvmSpeed)
    //            {
    //                actor.GlobalTranslation = dest;
    //                break;
    //            }
    //            actor.GlobalTranslation += mvmSpeed * disp.Normalized();
    //            yield return null;
    //        }
    //    }

    //}
}



class ThetaNode : FastPriorityQueueNode
{
	public int x { get { return coord.x; } }
	public int z { get { return coord.z; } }
	public float gScore = float.MaxValue;
	public Coord coord;
	public ThetaNode parent;

	public ThetaNode(Coord _coord, ThetaNode _parent)
	{
		coord = _coord;
		this.parent = _parent;
	}
}



