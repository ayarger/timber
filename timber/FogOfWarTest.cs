using Godot;
using System;

public class FogOfWarTest : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    [Export] ImageTexture texture;
    [Export] Material shader;
    float timer = 0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        timer = 0f;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

        Image i = new Image();
        i.Create(100, 1, false, Image.Format.Rgba8);
        timer += delta;

        var thing = Mathf.Sin(timer) * 5;

        i.Lock();
        i.SetPixel(0, 0, new Color((Mathf.Floor(thing)+54)/256.0f, 0 / 256.0f, 18 / 256.0f, 10 / 256.0f));
        i.SetPixel(1, 0, new Color(thing- Mathf.Floor(thing), 0f,0f,0f));
        i.SetPixel(2, 0, new Color(46/256.0f, 0 / 256.0f, 18 / 256.0f, 10 / 256.0f));
        i.SetPixel(3, 0, new Color(0f,0f,0f,0f));
        for(int j = 4; j < 100; j++)
        {
            i.SetPixel(j, 0, new Color(0,0,0,0));
        }
        i.Unlock();

        ImageTexture t = new ImageTexture();
        t.CreateFromImage(i);
        (shader as ShaderMaterial).SetShaderParam("positions", t);
        (shader as ShaderMaterial).SetShaderParam("test", 200f);
    }
}
