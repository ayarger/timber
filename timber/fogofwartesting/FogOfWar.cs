using Godot;
using System;

public class FogOfWar : Viewport
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    //[Export] ImageTexture texture;
    [Export] Material shader;
    [Export] public float screenWidth;
    [Export] public float screenHeight;

    [Export] public float screenPosX;
    [Export] public float screenPosZ;


    [Export] NodePath testNode;

    float timer = 0f;
    public PackedScene scene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        timer = 0f;
        scene = GD.Load<PackedScene>("res://fogofwartesting/FOWLitArea.tscn");
        EventBus.Subscribe<SpawnLightSourceEvent>(AddLitArea);
        EventBus.Subscribe<TileDataLoadedEvent>((TileDataLoadedEvent e) =>
        {
            screenWidth = Grid.width * 2+50;
            screenHeight = Grid.height * 2 + 50;
            screenPosX = -25;
            screenPosZ = -25;

            GD.Print($"Fog Of War Set Screen Width: {screenWidth}");
            GD.Print($"Fog Of War Set Screen Height: {screenHeight}");
        });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

        //Image i = new Image();
        //i.Create(100, 1, false, Image.Format.Rgba8);
        //timer += delta;

        //var thing = Mathf.Sin(timer) * 5;

        //i.Lock();
        //i.SetPixel(0, 0, new Color((Mathf.Floor(thing)+54)/256.0f, 0 / 256.0f, 18 / 256.0f, 10 / 256.0f));
        //i.SetPixel(1, 0, new Color(thing- Mathf.Floor(thing), 0f,0f,0f));
        //i.SetPixel(2, 0, new Color(46/256.0f, 0 / 256.0f, 18 / 256.0f, 10 / 256.0f));
        //i.SetPixel(3, 0, new Color(0f,0f,0f,0f));
        //for(int j = 4; j < 100; j++)
        //{
        //    i.SetPixel(j, 0, new Color(0,0,0,0));
        //}
        //i.Unlock();

        //ImageTexture t = new ImageTexture();
        //t.CreateFromImage(i);
        //(shader as ShaderMaterial).SetShaderParam("positions", t);
        //(shader as ShaderMaterial).SetShaderParam("test", 200f);

        (shader as ShaderMaterial).SetShaderParam("fowTexture", GetTexture());
        (shader as ShaderMaterial).SetShaderParam("screenWidth", screenWidth);
        (shader as ShaderMaterial).SetShaderParam("screenHeight", screenHeight);
        (shader as ShaderMaterial).SetShaderParam("screenPosX", screenPosX);
        (shader as ShaderMaterial).SetShaderParam("screenPosZ", screenPosZ);

    }

    public void AddLitArea(SpawnLightSourceEvent e)
    {
        var newArea = scene.Instance<FOWLitArea>();
        newArea.follow = e.spatial;
        newArea.parent = this;
        AddChild(newArea);
    }
}

public class SpawnLightSourceEvent
{
    public Spatial spatial;
    public SpawnLightSourceEvent(Spatial _spatial)
    {
        spatial = _spatial;
    }
}