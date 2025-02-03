using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

public class ElementList : DataGridView, ISavable
{
    private List<Element> elements;

    public ElementList()
    {
        elements = new List<Element>();
    }

    public string ToJSON()
    {
        var jelements = elements.Select(e => e.ToJSON()).ToList();
        return Newtonsoft.Json.JsonConvert.SerializeObject(new { elements = jelements });
    }

    public void FromJSON(string json)
    {
        var j = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        elements.Clear();
        foreach (var jelement in j.elements)
        {
            var e = new Element(Element.Type.Dielectric);
            e.FromJSON(jelement.ToString());
            AddElement(e);
        }
    }

    public void AddElement(Element e)
    {
        elements.Add(e);
        e.TypeChanged += (sender, args) =>
        {
            var i = FindIndex(e);
            if (i != -1)
            {
                this.InvalidateRow(i);
            }
        };
        e.Disposed += (sender, args) => RemoveElement(e, false);
    }

    public bool RemoveElement(Element e, bool del = true)
    {
        int i = FindIndex(e);
        if (i != -1)
        {
            return RemoveElement(i, del);
        }
        else
        {
            return false;
        }
    }

    public bool RemoveElement(int index, bool del = true)
    {
        if (index < 0 || index >= elements.Count)
        {
            return false;
        }
        var e = elements[index];
        elements.RemoveAt(index);
        if (del)
        {
            e.Dispose();
        }
        return true;
    }

    public Element ElementAt(int index)
    {
        if (index >= 0 && index < elements.Count)
        {
            return elements[index];
        }
        else
        {
            return null;
        }
    }

    public double GetDielectricConstantAt(System.Drawing.PointF p)
    {
        foreach (var e in elements)
        {
            var poly = new System.Drawing.Drawing2D.GraphicsPath();
            poly.AddPolygon(e.GetVertices().ToArray());
            if (poly.IsVisible(p))
            {
                switch (e.GetType())
                {
                    case Element.Type.GND:
                    case Element.Type.TracePos:
                    case Element.Type.TraceNeg:
                        return 1.0;
                    case Element.Type.Dielectric:
                        return e.GetEpsilonR();
                    default:
                        return 1.0;
                }
            }
        }
        return 1.0;
    }

    private int FindIndex(Element e)
    {
        return elements.IndexOf(e);
    }
}

public class TypeDelegate : DataGridViewComboBoxColumn
{
    public TypeDelegate()
    {
        foreach (var t in Element.GetTypes())
        {
            this.Items.Add(Element.TypeToString(t));
        }
    }

    public void SetEditorData(DataGridViewComboBoxCell editor, Element e)
    {
        editor.Value = Element.TypeToString(e.GetType());
    }

    public void SetModelData(DataGridViewComboBoxCell editor, ElementList list, int rowIndex)
    {
        var e = list.ElementAt(rowIndex);
        e.SetType(Element.TypeFromString(editor.Value.ToString()));
    }
}
