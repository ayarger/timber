using Godot;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArborCoroutine : Node
{
    static ArborCoroutine instance;
    public override void _Ready()
    {
        instance = this;
        GD.Print("Coroutine System Initialized.");
    }

    public override void _Process(float delta)
    {
        AdvanceAllCoroutines();
    }

    static Dictionary<Node, List<IEnumerator>> all_coroutines = new Dictionary<Node, List<IEnumerator>>();

    static Dictionary<Node, List<IEnumerator>> GetCoroutines()
    {
        return all_coroutines;
    }

    static Dictionary<IEnumerator, IEnumerator> child_to_parent = new Dictionary<IEnumerator, IEnumerator>();
    static HashSet<IEnumerator> ienumerators_on_hold = new HashSet<IEnumerator>();

    static HashSet<Node> dead_monobehaviors = new HashSet<Node>();
    static List<Node> nodes_running_coroutines = new List<Node>();
    static HashSet<IEnumerator> dead_coroutines = new HashSet<IEnumerator>();
    static HashSet<IEnumerator> new_ienumerators = new HashSet<IEnumerator>();

    static void AdvanceAllCoroutines()
    {
        //GD.Print("Coroutine System Tick.");

        Dictionary<Node, List<IEnumerator>> coroutines = GetCoroutines();
        dead_monobehaviors.Clear();
        nodes_running_coroutines.Clear();
        foreach (Node m in coroutines.Keys)
            nodes_running_coroutines.Add(m);

        //GD.Print("Running coroutines : " + nodes_running_coroutines.Count);

        foreach (Node m in nodes_running_coroutines)
        {
            if (m == null)
            {
                dead_monobehaviors.Add(m);
                continue;
            }

            //GD.Print("Advance coroutine 1 [" + m.IsProcessing() + "] [" + m.Name + "]");


            //GD.Print("Advance coroutine 2");


            List<IEnumerator> coroutines_on_monobehavior = new List<IEnumerator>(coroutines[m]);
            dead_coroutines.Clear();
            new_ienumerators.Clear();

            if(m == null) /* If the node is destroyed, deactivate all coroutines */
            {
                dead_coroutines = new HashSet<IEnumerator>(coroutines_on_monobehavior);
            }
            else /* Otherwise, process them */
            {
                foreach (IEnumerator i in coroutines_on_monobehavior)
                {
                    if (!m.IsInsideTree())
                        ienumerators_on_hold.Add(i);

                    if (ienumerators_on_hold.Contains(i))
                        continue;

                    bool active = i.MoveNext();

                    if (!active)
                    {
                        dead_coroutines.Add(i);

                        if (child_to_parent.ContainsKey(i))
                        {
                            IEnumerator parent = child_to_parent[i];
                            child_to_parent.Remove(i);
                            ienumerators_on_hold.Remove(parent);
                        }

                        continue;
                    }

                    /* Check for spawned children IEnumerators */
                    if (i.Current is IEnumerator)
                    {
                        IEnumerator new_i = i.Current as IEnumerator;
                        child_to_parent[new_i] = i;
                        new_ienumerators.Add(new_i);
                        ienumerators_on_hold.Add(i);
                    }
                }
            }

            foreach (IEnumerator new_ie in new_ienumerators)
            {
                coroutines[m].Add(new_ie);
            }

            foreach (IEnumerator i in dead_coroutines)
            {
                coroutines[m].Remove(i);
            }
        }

        foreach (Node m in dead_monobehaviors)
        {
            coroutines.Remove(m);
        }
    }

    public static void StartCoroutine(IEnumerator i, Node m=null)
    {
        if (m == null)
            m = instance;

        if (!all_coroutines.ContainsKey(m))
            all_coroutines[m] = new List<IEnumerator>();

        all_coroutines[m].Add(i);
    }

    public static IEnumerator DoStartCoroutine(IEnumerator i, Node m)
    {
        StartCoroutine(i, m);

        /* Wait for coroutine to finish. */
        while (all_coroutines.ContainsKey(m) && all_coroutines[m].Contains(i))
            yield return null;
    }

    public static void StopCoroutinesOnNode(Node source)
    {
        if (!all_coroutines.ContainsKey(source))
            return;

        all_coroutines.Remove(source);
    }

    public static IEnumerator WaitForSeconds(float duration_sec)
    {
        float timer = duration_sec;

        while (timer > 0.0f)
        {
            timer -= instance.GetProcessDeltaTime();
            yield return null;
        }
    }

    public static IEnumerator MoveOverTime(Node2D node, float duration_sec, Vector2 initial_pos, Vector2 end_pos, Curve ease = null)
    {
        float initial_time = Time.GetTicksMsec() / 1000.0f;
        float progress = (Time.GetTicksMsec() / 1000.0f - initial_time) / duration_sec;

        while(progress < 1.0f)
        {
            progress = (Time.GetTicksMsec() / 1000.0f - initial_time) / duration_sec;

            float eased_progress = progress;
            if (ease != null)
                eased_progress = ease.Interpolate(progress);

            node.Position = initial_pos.LinearInterpolate(end_pos, eased_progress);
            yield return null;
        }

        node.Position = end_pos;
    }

    public static IEnumerator MoveOverTime(Control node, float duration_sec, Vector2 initial_pos, Vector2 end_pos, Curve ease = null)
    {
        float initial_time = Time.GetTicksMsec() / 1000.0f;
        float progress = (Time.GetTicksMsec() / 1000.0f - initial_time) / duration_sec;

        while (progress < 1.0f)
        {
            progress = (Time.GetTicksMsec() / 1000.0f - initial_time) / duration_sec;

            float eased_progress = progress;
            if (ease != null)
                eased_progress = ease.Interpolate(progress);

            node.RectPosition = initial_pos.LinearInterpolate(end_pos, eased_progress);
            yield return null;
        }

        node.RectPosition = end_pos;
    }

    public static IEnumerator DoOverTime(System.Action<float> action, float duration_sec, Curve ease=null)
    {
        float initial_time = Time.GetTicksMsec() / 1000.0f;
        float progress = (Time.GetTicksMsec() / 1000.0f - initial_time) / duration_sec;

        action(0.0f);

        while (progress < 1.0f)
        {
            progress = (Time.GetTicksMsec() / 1000.0f - initial_time) / duration_sec;

            float eased_progress = progress;
            if (ease != null)
                eased_progress = ease.Interpolate(progress);

            action(eased_progress);

            yield return null;
        }

        action(1.0f);
    }
}

public static class ArborCoroutineExtensions
{
    public static void StartCoroutine(this Node node, IEnumerator i)
    {
        ArborCoroutine.StartCoroutine(i, node);
    }

    public static IEnumerator DoStartCoroutine(this Node node, IEnumerator i)
    {
        yield return ArborCoroutine.DoStartCoroutine(i, node);
    }
} 