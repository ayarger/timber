using Godot;
using System;
using System.Collections.Generic;

public class LuaUtils
{
    public static string ConstructArray(IEnumerable<string> list)
    {
        string ans = "{";
        int counter = 1;
        foreach (string item in list)
        {
            ans += item + ",";
            counter++;
        }
        ans = ans.Trim(',');
        ans += "}";
        return ans;
    }
}
