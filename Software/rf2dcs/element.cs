using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json.Linq;

public class Element
{
    public enum Type
    {
        Dielectric,
        TracePos,
        TraceNeg,
        GND,
        Last
    }

    private List<PointF> vertices;
    private string name;
    private Type type;
    private double epsilon_r;

    public Element(Type type)
    {
        this.type = type;
        epsilon_r = 4.3;
        switch (type)
        {
            case Type.TracePos:
                name = "RF+";
                break;
            case Type.TraceNeg:
                name = "RF-";
                break;
            case Type.Dielectric:
                name = "Substrate";
                break;
            case Type.GND:
                name = "GND";
                break;
            case Type.Last:
                break;
        }
    }

    public JObject ToJSON()
    {
        JObject j = new JObject();
        j["name"] = name;
        j["type"] = TypeToString(type);
        j["e_r"] = epsilon_r;
        JArray jvertices = new JArray();
        foreach (var v in vertices)
        {
            JObject jvertex = new JObject();
            jvertex["x"] = v.X;
            jvertex["y"] = v.Y;
            jvertices.Add(jvertex);
        }
        j["vertices"] = jvertices;
        return j;
    }

    public void FromJSON(JObject j)
    {
        name = j.Value<string>("name") ?? name;
        type = TypeFromString(j.Value<string>("type") ?? "");
        epsilon_r = j.Value<double?>("e_r") ?? epsilon_r;
        vertices.Clear();
        if (j.ContainsKey("vertices"))
        {
            foreach (var jvertex in j["vertices"])
            {
                PointF p = new PointF();
                p.X = jvertex.Value<float>("x");
                p.Y = jvertex.Value<float>("y");
                vertices.Add(p);
            }
        }
    }

    public static string TypeToString(Type type)
    {
        switch (type)
        {
            case Type.Dielectric:
                return "Dielectric";
            case Type.GND:
                return "GND";
            case Type.TracePos:
                return "Trace+";
            case Type.TraceNeg:
                return "Trace-";
            case Type.Last:
                return "";
        }
        return "";
    }

    public static Type TypeFromString(string s)
    {
        foreach (Type t in Enum.GetValues(typeof(Type)))
        {
            if (s == TypeToString(t))
            {
                return t;
            }
        }
        return Type.Last;
    }

    public static List<Type> GetTypes()
    {
        List<Type> ret = new List<Type>();
        foreach (Type t in Enum.GetValues(typeof(Type)))
        {
            ret.Add(t);
        }
        return ret;
    }

    public void AddVertex(int index, PointF vertex)
    {
        vertices.Insert(index, vertex);
    }

    public void AppendVertex(PointF vertex)
    {
        vertices.Add(vertex);
    }

    public void RemoveVertex(int index)
    {
        if (index >= 0 && index < vertices.Count)
        {
            vertices.RemoveAt(index);
        }
    }

    public void ChangeVertex(int index, PointF newCoords)
    {
        if (index >= 0 && index < vertices.Count)
        {
            vertices[index] = newCoords;
        }
    }

    public void SetType(Type t)
    {
        type = t;
        // Emit typeChanged event
    }

    public List<PointF> ToPolygon()
    {
        return new List<PointF>(vertices);
    }

    public string GetName() { return name; }
    public Type GetType() { return type; }
    public double GetEpsilonR() { return epsilon_r; }
    public List<PointF> GetVertices() { return vertices; }
    public void SetName(string s) { name = s; }
    public void SetEpsilonR(double er) { epsilon_r = er; }
}
