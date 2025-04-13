using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using LibTessDotNet;

public class Icon3D : MeshInstance
{
    [Export]
    public bool PerformTrace = true;

    [Export]
    public float pixels_to_world_unit = 100.0f;

    public override void _Ready()
    {
        ArborCoroutine.StartCoroutine(DoThing(), this);
    }

    IEnumerator DoThing()
    {
        ArborResource.Load<Texture>("public/images/lives_icon.png");
        yield return ArborResource.WaitFor("public/images/lives_icon.png");
        ImageTexture tex = (ImageTexture)ArborResource.Get<Texture>("public/images/lives_icon.png");


        //GD.Print("has alpha " + tex.HasAlpha());
        Image image = tex.GetData();

        var border_pixels = FindBorderPixels(image);
        image.Lock();
        foreach (var pixel in border_pixels)
        {
            image.SetPixel(Mathf.RoundToInt(pixel.x), Mathf.RoundToInt(pixel.y), new Color(1, 0, 0, 1));
        }
        image.Unlock();

        //texture.SetData(trace_result.marked_image);
        tex.CreateFromImage(image, (uint)Godot.Texture.FlagsEnum.Filter);

        SpatialMaterial mat = new SpatialMaterial();
        mat.AlbedoTexture = tex;
        SetSurfaceMaterial(0, mat);

        if (!PerformTrace)
            yield break;

        List<ContourVertex> world_points = new List<ContourVertex>();
        for (int i = 0; i < border_pixels.Count; i++)
        {
            float x_world = border_pixels[i].x / pixels_to_world_unit;
            float y_world = border_pixels[i].x / pixels_to_world_unit;

            ContourVertex vert = new ContourVertex()
            {
                Position = new Vec3() { X = x_world, Y = y_world, Z = 0 }
            };

            world_points.Add(vert);
        }

        // Your points representing the concave shape (scaled down).
        world_points = new List<ContourVertex>
        {
            new ContourVertex { Position = new Vec3 { X = 0, Y = 0 } },
            new ContourVertex { Position = new Vec3 { X = 1, Y = 0 } },
            new ContourVertex { Position = new Vec3 { X = 1, Y = 0.5f } },
            new ContourVertex { Position = new Vec3 { X = 0.5f, Y = 0.2f } },
            new ContourVertex { Position = new Vec3 { X = 1, Y = 1 } },
            new ContourVertex { Position = new Vec3 { X = 0, Y = 1 } }
        };

        // Create a tessellator
        Tess tess = new Tess();

        // Tesselate the polygon
        tess.AddContour(world_points.ToArray());
        //tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);
        tess.Tessellate();
        // Create SurfaceTool
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Godot.Mesh.PrimitiveType.Triangles);

        // Normal facing the camera (assuming camera is looking down the Z-axis).
        Vector3 normal = (GameplayCamera.GetGameplayCamera().GlobalTranslation - GlobalTranslation).Normalized();

        // Add vertices for the triangles
        for (int i = 0; i < tess.ElementCount; i++)
        {
            int index = tess.Elements[i];
            var vertex = tess.Vertices[index].Position;
            Vector3 position = new Vector3(vertex.X, vertex.Y, 0);
            surfaceTool.AddNormal(normal);
            surfaceTool.AddVertex(position);

            SpawnSphere(position);
        }

        // Get or create the mesh
        var mesh = Mesh as ArrayMesh;
        if (mesh == null)
        {
            mesh = new ArrayMesh();
        }

        // Commit the surface to the mesh
        surfaceTool.Commit(mesh);

        // Create a simple material
        var material = new SpatialMaterial
        {
            AlbedoColor = new Color(1, 0, 0) // Set to red
        };

        // Assign the material to the mesh
        //mesh.SurfaceSetMaterial(0, material);

        // Update the MeshInstance
        Mesh = mesh;










    }

    private void SpawnSphere(Vector3 position)
    {
        // Create a new MeshInstance for the sphere
        var sphereMeshInstance = new MeshInstance();

        // Create a new SphereMesh and assign it to the MeshInstance
        var sphereMesh = new SphereMesh();
        sphereMesh.Radius = 0.1f; // You can adjust the radius to change the size of the sphere
        sphereMeshInstance.Mesh = sphereMesh;

        // Set the position of the MeshInstance
        sphereMeshInstance.Translation = position;

        // Add the MeshInstance to the scene
        AddChild(sphereMeshInstance);
        sphereMeshInstance.Translation = Vector3.Zero;
        GD.Print("NEW SPHERE [" + sphereMeshInstance.GlobalTranslation + "]");
    }

    private List<Vector2> FindBorderPixels(Image image)
    {
        image.Lock();

        int width = image.GetWidth();
        int height = image.GetHeight();

        var borderPixels = new List<Vector2>();

        int skip_pixels = 5;
        int countdown = skip_pixels;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);

                // Check if the pixel is fully transparent
                if (pixelColor.a == 0)
                {
                    // Check adjacent pixels
                    var adjacentPixels = GetAdjacentPixels(x, y, width, height);
                    foreach (var adjacentPixel in adjacentPixels)
                    {
                        Color adjacentColor = image.GetPixel(Mathf.RoundToInt(adjacentPixel.x), Mathf.RoundToInt(adjacentPixel.y));

                        // If adjacent pixel is not fully transparent, this is a border pixel
                        if (adjacentColor.a != 0)
                        {
                            countdown--;
                            if(countdown <= 0)
                            {
                                countdown = skip_pixels;
                                borderPixels.Add(new Vector2(x, y));
                            }

                            break;
                        }
                    }
                }
            }
        }

        image.Unlock();

        return borderPixels;
    }

    public override void _Process(float delta)
    {
        Rotate(Vector3.Right, 0.05f);
    }

    private List<Vector2> GetAdjacentPixels(int x, int y, int width, int height)
    {
        var adjacentPixels = new List<Vector2>();

        if(x < width - 1)
            adjacentPixels.Add(new Vector2(x+1, y));
        if (x > 0)
            adjacentPixels.Add(new Vector2(x-1, y));
        if (y < height - 1)
            adjacentPixels.Add(new Vector2(x, y+1));
        if (y > 0)
            adjacentPixels.Add(new Vector2(x, y - 1));

        return adjacentPixels;
    }
}



