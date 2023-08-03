using Godot;
using System;

public class MeshIcon : Spatial
{
    public override void _Ready()
    {
        return;
        // Example points that define the shape
        Vector2[] points = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0.5f, 0.5f)
        };

        // Instantiate the MeshGenerator class
        MeshGenerator meshGenerator = new MeshGenerator();

        // Generate the tesselated mesh
        Mesh tesselatedMesh = meshGenerator.GenerateTesselatedMesh(points);

        // Create a MeshInstance node to display the mesh
        MeshInstance meshInstance = new MeshInstance
        {
            Mesh = tesselatedMesh,
            Transform = new Transform(Basis.Identity, new Vector3(0, 0, 0))
        };

        // Create a simple material
        SpatialMaterial material = new SpatialMaterial();
        material.AlbedoColor = new Color(1, 0, 0); // Red color for visibility

        // Assign the material to the MeshInstance
        meshInstance.SetSurfaceMaterial(0, material);

        // Add the MeshInstance node to the scene
        AddChild(meshInstance);
    }
}
