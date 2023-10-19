using Godot;
using System;

public class FOWLitArea : Sprite
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    public Spatial follow;
    public FogOfWar parent;
    public float baseRadius = 5f;
    public bool isLow = false;
    float timer = 0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        timer = 0f;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        timer += delta;
        float radius = isLow ? baseRadius * 1.3f : baseRadius * (1f + .075f * Mathf.Sin(2f*timer));

        if (follow == null)
        {
            QueueFree();
            return;
        }

        GlobalScale = new Vector2(2f * (parent.Size.x/1000f) * radius / parent.screenWidth, 2f * (parent.Size.y / 1000f) * radius / parent.screenHeight);
        Vector3 followPos = follow.GlobalTranslation;
        GlobalPosition = new Vector2(parent.Size.x*(followPos.x-parent.screenPosX) / parent.screenWidth, parent.Size.y * (followPos.z-parent.screenPosZ) / parent.screenHeight);


    }
}
