using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RF2DFieldSolver
{
    public class ElementList : Savable
    {
        public enum Column
        {
            Name,
            Type,
            EpsilonR,
            Last
        }

        private List<Element> elements;

        public ElementList()
        {
            elements = new List<Element>();
        }

        public override string ToJSON()
        {
            var json = new
            {
                elements = elements
            };
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        public override void FromJSON(string json)
        {
            dynamic data = JsonConvert.DeserializeObject(json);
            elements.Clear();
            foreach (var elementData in data.elements)
            {
                var element = new Element(Element.Type.Dielectric);
                element.FromJSON(JsonConvert.SerializeObject(elementData));
                AddElement(element);
            }
        }

        public void AddElement(Element element)
        {
            elements.Add(element);
            // TODO: Set up connections
            element.TypeChanged += (sender, e) =>
            {
                var index = FindIndex(element);
                if (index != -1)
                {
                    // Notify data changed
                }
            };
            element.Destroyed += (sender, e) =>
            {
                RemoveElement(element, false);
            };
        }

        public bool RemoveElement(Element element, bool del = true)
        {
            int index = FindIndex(element);
            if (index != -1)
            {
                return RemoveElement(index, del);
            }
            else
            {
                // Not found
                return false;
            }
        }

        public bool RemoveElement(int index, bool del = true)
        {
            if (index < 0 || index >= elements.Count)
            {
                return false;
            }
            var element = elements[index];
            elements.RemoveAt(index);
            if (del)
            {
                element.Dispose();
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

        public double GetDielectricConstantAt(PointF p)
        {
            foreach (var element in elements)
            {
                var polygon = element.ToPolygon();
                if (polygon.IsVisible(p))
                {
                    switch (element.ElementType)
                    {
                        case Element.Type.GND:
                        case Element.Type.TracePos:
                        case Element.Type.TraceNeg:
                            return 1.0;
                        case Element.Type.Dielectric:
                            return element.EpsilonR;
                        case Element.Type.Last:
                            return 1.0;
                    }
                }
            }
            // Not found, we are in the air
            return 1.0;
        }

        private int FindIndex(Element element)
        {
            return elements.IndexOf(element);
        }
    }

    public class TypeDelegate : DataGridViewComboBoxColumn
    {
        public TypeDelegate()
        {
            foreach (var type in Element.GetTypes())
            {
                Items.Add(Element.TypeToString(type));
            }
        }

        public void SetEditorData(DataGridViewComboBoxCell cell, Element element)
        {
            cell.Value = Element.TypeToString(element.ElementType);
        }

        public void SetModelData(DataGridViewComboBoxCell cell, Element element)
        {
            element.ElementType = Element.TypeFromString(cell.Value.ToString());
        }
    }
}
