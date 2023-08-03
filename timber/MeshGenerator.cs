using System;
using Godot;
using LibTessDotNet;
using System.Collections.Generic;

public class MeshGenerator : Node
{
    public Godot.Mesh GenerateTesselatedMesh(Vector2[] points)
    {
        // Check if the points array is not empty or null
        if (points == null || points.Length < 3)
        {
            GD.Print("Not enough points to create a mesh.");
            return null;
        }

        // Create a Tess instance
        var tess = new Tess();

        // Convert the points to ContourVertex array
        var contour = new ContourVertex[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            contour[i].Position = new Vec3 { X = points[i].x, Y = points[i].y, Z = 0 };
        }

        // Add the contour to tessellator
        tess.AddContour(contour);

        // Tesselate
        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

        // Create a new array mesh to hold the tessellated mesh
        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        var vertices = new Godot.Collections.Array();
        var indices = new Godot.Collections.Array();

        // Iterate through the tessellated vertices
        for (int i = 0; i < tess.Vertices.Length; i++)
        {
            vertices.Add(new Vector3(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y, tess.Vertices[i].Position.Z));
        }

        // Iterate through the tessellated elements (indices)
        for (int i = 0; i < tess.ElementCount * 3; i++)
        {
            indices.Add(tess.Elements[i]);
        }

        // Add vertices and indices to the arrays
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;

        // Add arrays to the arrayMesh
        arrayMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);

        // Return the tessellated mesh
        return arrayMesh;
    }
}
