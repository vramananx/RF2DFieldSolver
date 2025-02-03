using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;

namespace RF2DFieldSolver
{
    public class Element : Savable
    {
        public enum Type
        {
            Dielectric,
            TracePos,
            TraceNeg,
            GND,
            Last
        }

        public string Name { get; set; }
        public Type ElementType { get; set; }
        public double EpsilonR { get; set; }
        public List<PointF> Vertices { get; private set; }

        public Element(Type type)
        {
            ElementType = type;
            EpsilonR = 4.3;
            Vertices = new List<PointF>();

            switch (type)
            {
                case Type.TracePos:
                    Name = "RF+";
                    break;
                case Type.TraceNeg:
                    Name = "RF-";
                    break;
                case Type.Dielectric:
                    Name = "Substrate";
                    break;
                case Type.GND:
                    Name = "GND";
                    break;
                case Type.Last:
                    break;
            }
        }

        public override string ToJSON()
        {
            var json = new
            {
                name = Name,
                type = TypeToString(ElementType),
                e_r = EpsilonR,
                vertices = Vertices
            };
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        public override void FromJSON(string json)
        {
            dynamic data = JsonConvert.DeserializeObject(json);
            Name = data.name;
            ElementType = TypeFromString(data.type);
            EpsilonR = data.e_r;
            Vertices.Clear();
            foreach (var vertex in data.vertices)
            {
                Vertices.Add(new PointF((float)vertex.x, (float)vertex.y));
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
                default:
                    return "";
            }
        }

        public static Type TypeFromString(string s)
        {
            foreach (Type type in Enum.GetValues(typeof(Type)))
            {
                if (s == TypeToString(type))
                {
                    return type;
                }
            }
            return Type.Last;
        }

        public static List<Type> GetTypes()
        {
            var types = new List<Type>();
            foreach (Type type in Enum.GetValues(typeof(Type)))
            {
                types.Add(type);
            }
            return types;
        }

        public void AddVertex(int index, PointF vertex)
        {
            Vertices.Insert(index, vertex);
        }

        public void AppendVertex(PointF vertex)
        {
            Vertices.Add(vertex);
        }

        public void RemoveVertex(int index)
        {
            if (index >= 0 && index < Vertices.Count)
            {
                Vertices.RemoveAt(index);
            }
        }

        public void ChangeVertex(int index, PointF newCoords)
        {
            if (index >= 0 && index < Vertices.Count)
            {
                Vertices[index] = newCoords;
            }
        }

        public GraphicsPath ToPolygon()
        {
            var path = new GraphicsPath();
            if (Vertices.Count > 0)
            {
                path.AddPolygon(Vertices.ToArray());
            }
            return path;
        }
    }
}
