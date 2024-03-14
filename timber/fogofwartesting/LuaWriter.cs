using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public class LuaWriter : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string[] strings = { "a", "b", "c", "d", };
        string[] operations = { "print" };
        string[] binOps = { "+", "-" };

        //Programming pseudocode : In ready(), Set a = 1, b = 2, c = 3, d = 4, set a = b+c, and print a.

        string master = "";
        int tabAmt = 0;

        string className = "testloadafterwrite4";
        Block.ClassName = className;

        List<Variable> variables = new List<Variable>();
        variables.Add(new Variable() { variableName = "a" });
        variables.Add(new Variable() { variableName = "b" });
        variables.Add(new Variable() { variableName = "c" });
        variables.Add(new Variable() { variableName = "d" });
        //List<Variable> builtinVariables = new List<Variable>();
        //builtinVariables.Add(new Variable() { variableName = "hello" });
        List<Statement> statements = new List<Statement>(); //ready
        statements.Add(new AssignFloat() {variable = variables[0], other=new FloatConst() {val = 1f } });
        statements.Add(new AssignFloat() { variable = variables[1], other = new FloatConst() { val = 2f } });
        statements.Add(new AssignFloat() { variable = variables[2], other = new FloatConst() { val = 3f } });
        statements.Add(new AssignFloat() { variable = variables[3], other = new FloatConst() { val = 4f } });
        //statements.Add(new AssignFloat() { variable = builtinVariables[0], other = new BinaryOp() { op = "+", a = builtinVariables[0], b = new FloatConst {val = 1f } } });
        statements.Add(new AssignFloat() { variable = variables[0], other = new BinaryOp() { op="+", a = variables[1], b = variables[2] } });

        master += "local "+className+" = {}\n";

        foreach (Variable variable in variables)
        {
            master += className + "." + variable.variableName + " = 0\n";
        }
        master += $"function {className}:ready()\n";
        tabAmt++;
        foreach (Statement statement in statements)
        {
            for (int i = 0; i < tabAmt; i++) master += "\t";
            master += statement.ToString();
            master += "\n";
        }
        tabAmt--;
        master += "end\n";
        master += $"return {className}\n";
        GD.Print($"Wrote {className}");
        Godot.File x = new Godot.File();
        x.Open($"fogofwartesting/testwriting/{className}.lua", Godot.File.ModeFlags.Write);
        x.StoreString(master);
        x.Close();

        var y = GetChild(0);
        y.SetScript((Script)GD.Load($"res://fogofwartesting/testwriting/{className}.lua"));
        y.Call("ready");


    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}

public abstract class Block
{
    public static string ClassName = "";
    public abstract string ToString();
}
public abstract class FloatLike : Block
{
}

public class FloatConst : FloatLike
{
    public float val;

    public override string ToString()
    {
        return val.ToString();
    }
}
public abstract class Statement : Block
{
}

public class Variable : FloatLike
{
    public string variableName;
    public override string ToString()
    {
        return "self."+variableName;
    }
}

public class AssignFloat : Statement
{
    public Variable variable;
    public FloatLike other;
    public override string ToString()
    {
        return variable.ToString() + " = " + other.ToString();
    }
}
public class Print : Statement
{
    public FloatLike other;
    public override string ToString()
    {
        return $"print({other.ToString()})";
    }
}
public class BinaryOp : FloatLike
{
    public FloatLike a;
    public FloatLike b;
    public string op;
    public override string ToString()
    {
        return "("+a.ToString() + $" {op} " + b.ToString()+")";
    }
}