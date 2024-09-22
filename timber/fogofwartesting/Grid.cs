using Godot;
using System;


public class Grid
{
    public static TileData[][] tiledata;
    public static float tileWidth = 2.0f;

    public static int width
    {
        get
        {
            return tiledata.Length > 0 ? tiledata[0].Length : 0;
        }
    }
    public static int height
    {
        get
        {
            return tiledata.Length;
        }
    }
    public static TileData Get(Coord c)
    {
        return Get(c.x, c.z);
    }
    public static TileData Get(int x, int z)
    {
        if (x < 0 || z < 0 || z >= Grid.tiledata.Length || x >= (Grid.tiledata.Length > 0 ? Grid.tiledata[0].Length : 0))
        {
            return new TileData('e', x, z);
        }
        else
        {
            return tiledata[z][x];
        }
    }
    public static TileData Get(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileWidth);
        int z = Mathf.RoundToInt(worldPos.z / tileWidth);
        return Get(x, z);
    }

    public static Coord ConvertToCoord(Vector3 worldPos)
    {
        return new Coord(Mathf.RoundToInt(worldPos.x / tileWidth), Mathf.RoundToInt(worldPos.z / tileWidth));
    }
    public static Vector3 ConvertToWorldPos(Coord coord)
    {
        return new Vector3(coord.x * tileWidth, 0, coord.z * tileWidth);
    }
    public static Vector3 LockToGrid(Vector3 worldPos)
    {
        return new Vector3(Mathf.RoundToInt(worldPos.x / tileWidth) * tileWidth, worldPos.y, Mathf.RoundToInt(worldPos.z / tileWidth) * tileWidth);
    }

}

public class TileData
{
    public Actor actor = null;
    public char value = 'e';
    public Coord coord;
    public int x
    {
        get
        {
            return coord.x;
        }
        set
        {
            coord.x = value;
        }
    }
    public int z
    {
        get
        {
            return coord.z;
        }
        set
        {
            coord.z = value;
        }
    }

    public Vector3 GlobalTranslation
    {
        get
        {
            return new Vector3(coord.x * Grid.tileWidth, 0f, coord.z * Grid.tileWidth);
        }
    }

    public TileData(char _value, Coord _coord)
    {
        value = _value;
        coord = _coord;
    }

    public TileData(char _value, int x, int z)
    {
        value = _value;
        coord = new Coord(x,z);
    }
    public Vector3[] GetCorners()
    {
        Vector3 gt = GlobalTranslation;
        Vector3[] ans = new Vector3[]
        {
            new Vector3(gt.x-Grid.tileWidth/2f,gt.y,gt.z-Grid.tileWidth/2f),
            new Vector3(gt.x+Grid.tileWidth/2f,gt.y,gt.z-Grid.tileWidth/2f),
            new Vector3(gt.x+Grid.tileWidth/2f,gt.y,gt.z+Grid.tileWidth/2f),
            new Vector3(gt.x-Grid.tileWidth/2f,gt.y,gt.z+Grid.tileWidth/2f)
        };
        return ans;
    }
}
public class Coord
{
    public int x;
    public int z;
    public Coord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
    public static Coord operator +(Coord a, Coord b)
    => new Coord(a.x + b.x, a.z + b.z);
    public static Coord operator -(Coord a, Coord b)
    => new Coord(a.x - b.x, a.z - b.z);
    public float Mag()
    {
        return Mathf.Sqrt(x * x + z * z);
    }
    public override bool Equals(object obj)
    {
        Coord other = obj as Coord;

        if (other == null)
        {
            return false;
        }

        return other.x == x && other.z == z;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            var result = 0;
            result = (result * 397) ^ x;
            result = (result * 397) ^ z;
            return result;
        }
    }

}