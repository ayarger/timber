using Godot;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SelectionSystem : Node
{
    static SelectionSystem instance;
    public static Vector3 GetTilePosition()
    {
        return instance.active_cursor.GlobalTranslation;
    }

    public static SelectionRegion GetCurrentSelectionRegion() // will be null if no selection is ongoing.
    {
        if (!instance.dragging)
            return new SelectionRegion(instance.current_cursor_tile, instance.current_cursor_tile);

        return new SelectionRegion(instance.drag_start_point, instance.current_cursor_tile);
    }

    Vector3 current_cursor_tile = Vector3.Zero;

    [Export] PackedScene selection_square_scene;

    CSGMesh active_cursor;

    HashSet<IsSelectable> current_active_selectables = new HashSet<IsSelectable>();
    public static HashSet<IsSelectable>  GetCurrentActiveSelectables() { return instance.current_active_selectables; }
    public static bool IsSelected(IsSelectable selectable)
    {
        return instance.current_active_selectables.Contains(selectable);
    }

    public override void _Ready()
    {
        instance = this;
        active_cursor = GetNode<CSGMesh>("active_cursor");
        active_cursor.GetNode<CSGMesh>("active_cursor_tower").Visible = false;

        EventBus.Subscribe<EventTileCursorChangedLocation> (OnEventTileCursorChangedLocation);
        EventBus.Subscribe<TowerManager.EventToggleTowerPlacement> (OnEventToggleTowerPlacement);
        EventBus.Subscribe<TowerManager.EventCancelTowerPlacement> (OnEventCancelTowerPlacement);
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        DetermineLocation();
        ProcessMovement();
        ProcessScale();
    }

    void ProcessMovement()
    {
        current_cursor_tile = Grid.LockToGrid(current_cursor_tile);
        current_cursor_tile.y = 0.1f;
        active_cursor.GlobalTranslation += (current_cursor_tile - active_cursor.GlobalTranslation) * 0.4f;
    }

    float scale_velocity = 0.0f;
    void ProcessScale()
    {
        float desired_scale = 2.0f;
        if (left_click_down)
            desired_scale = 1.5f;

        float k = 0.1f;
        float x = desired_scale - active_cursor.Scale.x;
        float accel = k * x;

        scale_velocity += accel;
        scale_velocity *= 0.85f;

        active_cursor.Scale += scale_velocity * Vector3.One;
    }

    Vector2 current_mouse_screen_position = Vector2.Zero;
    public static Vector2 GetCursorWindowPosition() { return instance.current_mouse_screen_position; }
    bool left_click_down = false;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            current_mouse_screen_position = eventMouseMotion.Position;
        }

        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.ButtonIndex == (int)ButtonList.Left && eventMouseButton.Pressed)
                left_click_down = true;
            else
                left_click_down = false;
        }
    }

    bool cursor_inside_window = true;
    public static bool CursorInsideWindow() { return instance.cursor_inside_window; }
    bool window_focused = true;
    public static bool WindowFocused() { return instance.window_focused; }
    public override void _Notification (int what)
    {
        switch (what)
        {
            case SceneTree.NotificationWmMouseEnter:
                cursor_inside_window = true;
                break;
            case SceneTree.NotificationWmMouseExit:
                cursor_inside_window = false;
                break;

            case SceneTree.NotificationWmFocusIn:
                window_focused = true;
                break;

            case SceneTree.NotificationWmFocusOut:
                window_focused = false;
                break;
        }
    }

    Vector3 previous_frame_current_cursor_tile = Vector3.Zero;
    Vector3 drag_start_point = Vector3.Zero;
    bool dragging = false;
    void DetermineLocation()
    {
        var from = GameplayCamera.GetGameplayCamera().ProjectRayOrigin(current_mouse_screen_position);
        var dir = GameplayCamera.GetGameplayCamera().ProjectRayNormal(current_mouse_screen_position);

        Vector3 intersection_point = GetRayPlaneIntersection(new Ray(from, dir), Vector3.Up);
        //DebugSphere.VisualizePoint(intersection_point);

        /* Round */
        Vector3 rounded_point = Grid.LockToGrid(intersection_point);

        /* Check for drag operation */
        if (left_click_down)
        {
            if (!dragging)
            {
                EventBus.Publish(new EventSelectionBegun());
                drag_start_point = rounded_point;
                dragging = true;
            }
        }
        else
        {
            if (dragging)
            {
                ClearDragSelectionVisualizationTiles();

                /* Announce selection region */
                EventBus.Publish(new EventSelectionFinished(drag_start_point, rounded_point));
                dragging = false;

                /* Store  */
                current_active_selectables = IsSelectable.GetSelectablesWithinRegion(new SelectionRegion(drag_start_point, rounded_point));
            }
        }

        previous_frame_current_cursor_tile = current_cursor_tile;
        current_cursor_tile = rounded_point;

        if (current_cursor_tile != previous_frame_current_cursor_tile)
            EventBus.Publish(new EventTileCursorChangedLocation(previous_frame_current_cursor_tile, current_cursor_tile, dragging));
    }

    Vector3 GetRayPlaneIntersection(Ray ray, Vector3 plane_normal)
    {
        float t = -(plane_normal.Dot(ray.start)) / (plane_normal.Dot(ray.direction));
        return ray.start + t * ray.direction;
    }

    List<Node> selection_visualization_tiles = new List<Node>();
    void ClearDragSelectionVisualizationTiles()
    {
        for (int i = 0; i < selection_visualization_tiles.Count; i++)
        {
            selection_visualization_tiles[i].QueueFree();
        }
        selection_visualization_tiles.Clear();
    }
    void OnEventTileCursorChangedLocation(EventTileCursorChangedLocation e)
    {
        ClearDragSelectionVisualizationTiles();

        /* Repopulate tiles for new selection region */
        if (e.is_dragging)
        {
            Vector3 first_point = drag_start_point;
            Vector3 second_point = current_cursor_tile;

            Vector3 top_left_point = new Vector3(Mathf.Min(first_point.x, second_point.x), 0.1f, Mathf.Min(first_point.z, second_point.z));   // smallest x, smallest z
            Vector3 bottom_right_point = new Vector3(Mathf.Max(first_point.x, second_point.x), 0.1f, Mathf.Max(first_point.z, second_point.z)); // largest x, largest z

            for (float x = top_left_point.x; x <= bottom_right_point.x; x += Grid.tileWidth)
            {
                for (float z = top_left_point.z; z <= bottom_right_point.z; z += Grid.tileWidth)
                {
                    CSGMesh new_tile = (CSGMesh)selection_square_scene.Instance();
                    AddChild(new_tile);
                    new_tile.GlobalTranslation = new Vector3(x, 0.1f, z);

                    selection_visualization_tiles.Add(new_tile);
                }
            }
        }
    }
    
    
    void OnEventToggleTowerPlacement(TowerManager.EventToggleTowerPlacement e)
    {
        active_cursor.GetNode<CSGMesh>("active_cursor_tower").Visible = true;

    }

    void OnEventCancelTowerPlacement(TowerManager.EventCancelTowerPlacement e)
    {
        active_cursor.GetNode<CSGMesh>("active_cursor_tower").Visible = false;
    }
}

public class EventTileCursorChangedLocation {

    public Vector3 previous_tile;
    public Vector3 current_tile;
    public bool is_dragging = false;

    public EventTileCursorChangedLocation (Vector3 _previous_tile, Vector3 _current_tile, bool _is_dragging )
    {
        previous_tile = _previous_tile;
        current_tile = _current_tile;
        is_dragging = _is_dragging;
    }
}

public class EventSelectionFinished
{
    public SelectionRegion selection_region;

    public EventSelectionFinished (SelectionRegion _selection_region)
    {
        selection_region = _selection_region;
    }

    public EventSelectionFinished (Vector3 point_a, Vector3 point_b)
    {
        selection_region = new SelectionRegion(point_a, point_b);
    }
}

public class EventSelectionBegun { }

public class SelectionRegion
{
    public Vector3 top_left_coordinate;
    public Vector3 bottom_right_coordinate;

    public SelectionRegion(Vector3 point_a, Vector3 point_b)
    {
        top_left_coordinate = new Vector3(Mathf.Min(point_a.x,point_b.x) - Grid.tileWidth / 2f, 0.1f, Mathf.Min(point_a.z, point_b.z) - Grid.tileWidth / 2f);   // smallest x, smallest z
        bottom_right_coordinate = new Vector3(Mathf.Max(point_a.x, point_b.x)+ Grid.tileWidth / 2f, 0.1f, Mathf.Max(point_a.z, point_b.z)+ Grid.tileWidth / 2f); // largest x, largest z
    }

    public bool IsPointWithinRegion (Vector3 point)
    {
        return point.x >= top_left_coordinate.x && point.x <= bottom_right_coordinate.x && point.z >= top_left_coordinate.z && point.z <= bottom_right_coordinate.z;
    }
}

public class EventMovementCommand
{
    public List<Actor> actors;

    public EventMovementCommand(List<Actor> _actors)
    {
        actors = _actors;
    }
}