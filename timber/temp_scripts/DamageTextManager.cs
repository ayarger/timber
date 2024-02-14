using Godot;
using System;

public class DamageTextManager : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    // Called when the node enters the scene tree for the first time.
    static DamageTextManager instance;

    PackedScene scene;

    static float textDisplayOffset = 100;

    public override void _Ready()
    {
        instance = this;
        scene = GD.Load<PackedScene>("temp_scenes/DamageText.tscn");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        return;
        if (Input.IsActionJustPressed("right_click"))
        {
            var scene = GD.Load<PackedScene>("temp_scenes/DamageText.tscn");
            Control text = (Control)scene.Instance();
            AddChild(text);
            text.SetPosition(new Vector2(GD.Randf()*RectSize.x, GD.Randf()*RectSize.y));


        }
    }

    public static void DrawText(int num, Actor actor)
    {
        Control text = (Control)instance.scene.Instance();
        text.GetNode<Label>("Control/Label").Text = num.ToString();
        Vector2 pos = GameplayCamera.GetGameplayCamera().UnprojectPosition(actor.GlobalTranslation);
        text.SetPosition(pos + Vector2.Up * textDisplayOffset * GD.Randf());

        instance.AddChild(text);

    }
}
