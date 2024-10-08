using Godot;
using System;
using System.Collections.Generic;

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

    [Export] public bool isActorViewport;
    [Export] public bool isHighVisibility;
    [Export] public Texture actorFOWTexture;


    public PackedScene scene;
    public static FogOfWar instance;
    public static FogOfWar actorInstance;

    private Dictionary<Vector3, FOWLitArea> towerLitAreaDict;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (isActorViewport)
        {
            actorInstance = this;
        }
        else
        {
            instance = this;
        }
        scene = GD.Load<PackedScene>("res://fogofwartesting/FOWLitArea.tscn");
        EventBus.Subscribe<SpawnLightSourceEvent>(AddLitArea);
        EventBus.Subscribe<RemoveLightSourceEvent>(RemoveLitArea);
        EventBus.Subscribe<TileDataLoadedEvent>((TileDataLoadedEvent e) =>
        {
            screenWidth = Grid.width * 2+50;
            screenHeight = Grid.height * 2 + 50;
            screenPosX = -25;
            screenPosZ = -25;

            GD.Print($"Fog Of War Set Screen Width: {screenWidth}");
            GD.Print($"Fog Of War Set Screen Height: {screenHeight}");
        });

        towerLitAreaDict = new Dictionary<Vector3, FOWLitArea>();
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
        //texture.Lock();
        if (isActorViewport) return;

        if (isHighVisibility)
        {
            (shader as ShaderMaterial).SetShaderParam("fowTexture", GetTexture());
        }
        else
        {
            (shader as ShaderMaterial).SetShaderParam("lowVisibility_texture", GetTexture());
        }
        (shader as ShaderMaterial).SetShaderParam("screenWidth", screenWidth);
        (shader as ShaderMaterial).SetShaderParam("screenHeight", screenHeight);
        (shader as ShaderMaterial).SetShaderParam("screenPosX", screenPosX);
        (shader as ShaderMaterial).SetShaderParam("screenPosZ", screenPosZ);



    }

    public void AddLitArea(SpawnLightSourceEvent e)
    {
        var newArea = scene.Instance<FOWLitArea>();
        if (isActorViewport)
        {
            newArea.Texture = actorFOWTexture;
        }
        else if (!isHighVisibility)
        {
            newArea.isLow = true;
        }
        newArea.follow = e.spatial;
        newArea.parent = this;
        AddChild(newArea);

        if (e.forTower)
        {
            towerLitAreaDict[e.spatial.GlobalTranslation] = newArea;
        }
    }
    
    public void RemoveLitArea(RemoveLightSourceEvent e)
    {
        towerLitAreaDict[e.vec].QueueFree();
        towerLitAreaDict.Remove(e.vec);
    }
    
    //public Color GetAtPixel(Vector3 pos)
    //{
    //    int x = Mathf.RoundToInt(Size.x * (pos.x-screenPosX)/screenWidth);
    //    int y = Mathf.RoundToInt(Size.y * (pos.z - screenPosZ) / screenHeight);
    //    return texture.GetPixel(x, y);
    //}
}

public class SpawnLightSourceEvent
{
    public Spatial spatial;
    public bool forTower;
    public SpawnLightSourceEvent(Spatial _spatial, bool _forTower = false)
    {
        spatial = _spatial;
        forTower = _forTower;
    }
}

public class RemoveLightSourceEvent
{
    public Vector3 vec;
    public RemoveLightSourceEvent(Vector3 _vec)
    {
        vec = _vec;
    }
}